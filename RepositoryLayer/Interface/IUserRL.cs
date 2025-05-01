using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
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
