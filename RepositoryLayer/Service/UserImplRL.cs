using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Interface;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using ModelLayer.Entity;
using RepositoryLayer.Helper;
using ConsumerLayer;
using ConsumerLayer.Helper;
using System.Text.Json;
using Microsoft.Extensions.Logging;
//using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using Azure.Core;

namespace RepositoryLayer.Service
{
    public class UserImplRL : IUserRL
    {
        private readonly UserContext _context;
        private readonly PasswordHasherRL _passwordHasher;
        private readonly AuthService _authService;
        private readonly GeneratesOtp _generatesOtp;
        private readonly ILogger<UserImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;

        public UserImplRL(UserContext context, PasswordHasherRL passwordHasherRL, AuthService authService, GeneratesOtp generatesOtp, ILogger<UserImplRL> logger, IConnectionMultiplexer redis)
        {
            _context = context;
            _passwordHasher = passwordHasherRL;
            _authService = authService;
            _generatesOtp = generatesOtp;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
        }

        private string GetUserCacheKey(string email) => $"user:{email}";
        private string GetUserByNameCacheKey(string firstName) => $"userByName:{firstName}";

        public async Task<ResponseDTO<UserEntity>> DisplayDetails(string email)
        {
            try
            {
                _logger.LogInformation("Fetching user details by email");

                string cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedUser.HasValue)
                {
                    var userFromCache = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    return new ResponseDTO<UserEntity> { Success = true, Message = "User found (cached)", Data = userFromCache };
                }

                var user = _context.Users.SingleOrDefault(x => x.Email == email);
                if (user != null)
                {
                    await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));
                    return new ResponseDTO<UserEntity> { Success = true, Message = "User found!", Data = user };
                }

                return new ResponseDTO<UserEntity> { Success = false, Message = "No such user exists!", Data = null };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching user details: {ex.Message}");
                return new ResponseDTO<UserEntity> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ResponseDTO<string>> SignUp(UserRequest userRequest)
        {
            _logger.LogInformation("Signing up a new user");

            try
            {
                var cacheKey = GetUserCacheKey(userRequest.Email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedUser.HasValue)
                {
                    return new ResponseDTO<string> { Success = false, Message = "Failed to Sign Up", Data = "User already exists with this email" };
                }

                var user = _context.Users.FirstOrDefault(x => x.Email == userRequest.Email);
                if (user != null)
                {
                    return new ResponseDTO<string> { Success = false, Message = "Failed to Sign Up", Data = "User already exists with this email" };
                }

                var hashedPwd = await _passwordHasher.HashPasswordAsync(userRequest.Password);
                var newUser = new UserEntity
                {
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    Email = userRequest.Email,
                    Password = hashedPwd
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(newUser), TimeSpan.FromMinutes(30));
                await _redisDatabase.StringSetAsync(GetUserByNameCacheKey(newUser.FirstName), JsonSerializer.Serialize(newUser), TimeSpan.FromMinutes(30));

                return new ResponseDTO<string> { Success = true, Message = "Sign Up Successful", Data = $"{newUser.FirstName} {newUser.LastName} Signed Up Successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"SignUp error: {ex.Message}");
                return new ResponseDTO<string> { Success = false, Message = "Failed to Sign Up", Data = ex.Message };
            }
        }

        public async Task<ResponseDTO<string>> Login(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Attempting to login for {loginRequest.Email}");

                var cacheKey = GetUserCacheKey(loginRequest.Email);
                UserEntity user;

                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedUser.HasValue)
                {
                    user = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                }
                else
                {
                    user = _context.Users.SingleOrDefault(x => x.Email == loginRequest.Email);
                    if (user == null)
                    {
                        return new ResponseDTO<string> { Success = false, Message = "User does not exist", Data = null };
                    }
                    await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));
                }

                var isPasswordValid = await _passwordHasher.VerifyPasswordAsync(loginRequest.Password, user.Password);
                if (!isPasswordValid)
                {
                    return new ResponseDTO<string> { Success = false, Message = "Invalid Password", Data = null };
                }

                var token = _authService.GenerateJWTToken(user);
                return new ResponseDTO<string> { Success = true, Message = "Login Successful", Data = token };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return new ResponseDTO<string> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ResponseDTO<string>> UpdateEmail(UpdateEmailRequest updateEmail)
        {
            _logger.LogInformation($"Updating email for {updateEmail.fName} {updateEmail.lName}");
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.FirstName == updateEmail.fName && x.LastName == updateEmail.lName);
                if (user != null)
                {
                    user.Email = updateEmail.newEmail;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    var updatedCacheKey = GetUserCacheKey(user.Email);
                    await _redisDatabase.StringSetAsync(updatedCacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));

                    return new ResponseDTO<string> { Success = true, Message = "Email Updated Successfully", Data = $"Email Updated to {user.Email}" };
                }

                return new ResponseDTO<string> { Success = false, Message = "Failed to Update Email", Data = "User does not exist" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email update error: {ex.Message}");
                return new ResponseDTO<string> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ResponseDTO<List<UserEntity>>> ViewAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all users");
                var users = _context.Users.ToList();
                return new ResponseDTO<List<UserEntity>> { Success = true, Message = "Users found!", Data = users };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fetch all users error: {ex.Message}");
                return new ResponseDTO<List<UserEntity>> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ResponseDTO<string>> DeleteUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Deleting user with email: {email}");
                var user = _context.Users.SingleOrDefault(x => x.Email == email);

                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    await _redisDatabase.KeyDeleteAsync(GetUserCacheKey(email));
                    await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(user.FirstName));

                    return new ResponseDTO<string> { Success = true, Message = $"User with email:{email} removed", Data = null };
                }

                return new ResponseDTO<string> { Success = false, Message = "No such user exists!", Data = null };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete user error: {ex.Message}");
                return new ResponseDTO<string> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ResponseDTO<string>> ForgotPassword(string email)
        {
            try
            {
                _logger.LogInformation($"Sending OTP for password reset to {email}");

                var cacheKey = GetUserCacheKey(email);
                UserEntity user;

                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedUser.HasValue)
                {
                    user = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                }
                else
                {
                    user = _context.Users.FirstOrDefault(u => u.Email == email);
                    if (user == null)
                    {
                        return new ResponseDTO<string> { Success = false, Message = "User not found", Data = null };
                    }
                    await _redisDatabase.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(30));
                }

                string resetToken = _generatesOtp.GenerateOtp();
                var emailMsg = new EmailMessage
                {
                    Subject = "Password Change Request",
                    Body = $"Hello From Team Fundoo, your OTP is {resetToken}",
                    Email = email
                };

                var message = JsonSerializer.Serialize(emailMsg);
                new Publisher().PublishToQueue("fundoo", message);

                return new ResponseDTO<string> { Success = true, Message = "OTP sent", Data = resetToken };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                return new ResponseDTO<string> { Success = false, Message = ex.Message, Data = null };
            }
        }
    }
}
