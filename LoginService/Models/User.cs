using System.ComponentModel.DataAnnotations;

namespace LoginService.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string Username { get; set; }  = null!;

        [Required]
        public string Password { get; set; }  = null!;

        [Required]
        public string Role { get; set; } = "user";
    }
}