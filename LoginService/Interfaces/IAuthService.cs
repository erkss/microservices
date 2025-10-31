using LoginService.Dtos;
using LoginService.Models;

namespace LoginService.Interfaces
{
    public interface IAuthService
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> RegisterAsync(UserRegisterDto dto);
        Task<string> LoginAsync(UserLoginDto dto);
        Task<bool> DeleteUserAsync(int id);
    }
}
