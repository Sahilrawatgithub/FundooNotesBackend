using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTO;
using ModelLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        Task<ResponseDTO<string>> SignUp(UserRequest userRequest);

        Task<ResponseDTO<UserEntity>> DisplayDetails(string email);

        Task<ResponseDTO<string>> Login(LoginRequest loginRequest); 

        Task<ResponseDTO<string>> UpdateEmail(UpdateEmailRequest updateEmailRequest);

        Task<ResponseDTO<List<UserEntity>>> ViewAllUsersAsync();

        Task<ResponseDTO<string>> DeleteUserByEmailAsync(string email);  

        Task<ResponseDTO<string>> ForgotPassword(string email);
    }
}
