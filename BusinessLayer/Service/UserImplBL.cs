using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class UserImplBL : IUserBL
    {
        private readonly IUserRL _user;

        public UserImplBL(IUserRL user)
        {
            _user = user;
        }

        public async Task<ResponseDTO<string>> SignUp(UserRequest request)
        {
            return await _user.SignUp(request);
        }

        public async Task<ResponseDTO<UserEntity>> DisplayDetails(string email)
        {
            return await _user.DisplayDetails(email);
        }

        public async Task<ResponseDTO<string>> Login(LoginRequest loginRequest)
        {
            return await _user.Login(loginRequest);
        }

        public async Task<ResponseDTO<string>> UpdateEmail(UpdateEmailRequest updateEmail)
        {
            return await _user.UpdateEmail(updateEmail);
        }   

        public async Task<ResponseDTO<List<UserEntity>>> ViewAllUsersAsync()
        {
            return await _user.ViewAllUsersAsync();
        }  
        
        public async Task<ResponseDTO<string>> DeleteUserByEmailAsync(string email)
        {
            return await _user.DeleteUserByEmailAsync(email);
        }

        public async Task<ResponseDTO<string>> ForgotPassword(string email)
        {
            return await _user.ForgotPassword(email);
        }
    }
}
