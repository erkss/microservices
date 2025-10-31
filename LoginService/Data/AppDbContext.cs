using LoginService.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginService.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public required DbSet<User> Users { get; set; }
    }
}