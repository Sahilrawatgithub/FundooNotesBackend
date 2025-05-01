using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.DTO;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserBL userBL,ILogger<UserController> logger)
        {
            _userBL = userBL;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with the provided information.
        /// </summary>
        /// <param name="request">User registration details.</param>
        /// <returns>Returns success or error response after registration attempt.</returns>
        [HttpPost("Signup")]
        public async Task<IActionResult> SignUp(UserRequest request)
        {
                
                var result = await _userBL.SignUp(request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                else
                {
                    return Ok(result);
                }
        }

        /// <summary>
        /// Retrieves user details based on the provided email.
        /// </summary>
        /// <param name="email">Email of the user whose details are to be retrieved.</param>
        /// <returns>Returns user details if found, otherwise error response.</returns>
        [HttpGet("ViewDetails")]
        public async Task<IActionResult> DisplayDetails(string email)
        {
            var result=await _userBL.DisplayDetails(email);
            if(result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Authenticates the user and returns a token upon successful login.
        /// </summary>
        /// <param name="loginRequest">User login credentials.</param>
        /// <returns>Returns token and success status if login is successful.</returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var result = await _userBL.Login(loginRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Updates the email of a logged-in user.
        /// </summary>
        /// <param name="updateEmailRequest">Old and new email information.</param>
        /// <returns>Returns success or error response after attempting email update.</returns>
        [HttpPatch("UpdateEmail")]
        [Authorize]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest updateEmailRequest)
        {
            var result = await _userBL.UpdateEmail(updateEmailRequest);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Deletes a user by their email address. Authorization required.
        /// </summary>
        /// <param name="email">Email of the user to be deleted.</param>
        /// <returns>Returns success or failure message after deletion attempt.</returns>
        [HttpDelete("DeleteUserByEmail")]
        [Authorize]
        public async Task<IActionResult> DeletUserByEmail(string email)
        {
            var result = await _userBL.DeleteUserByEmailAsync(email);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }

        /// <summary>
        /// Sends a password reset link to the provided email. Authorization required.
        /// </summary>
        /// <param name="email">Email to send the password reset link to.</param>
        /// <returns>Returns success or failure message for password reset operation.</returns>
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _userBL.ForgotPassword(email);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Retrieves a list of all users.
        /// </summary>
        /// <returns>Returns list of all users with success or failure status.</returns>
        [HttpGet("ViewAllUsers")]

        public async Task<IActionResult> ViewAllUsers()
        {
            var result = await _userBL.ViewAllUsersAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
