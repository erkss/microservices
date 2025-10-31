using System.ComponentModel.DataAnnotations;

namespace LoginService.Dtos
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Role is required.")]
        [MinLength(1, ErrorMessage = "Role cannot be empty.")]
        public string Role { get; set; } = "user";
    }
}