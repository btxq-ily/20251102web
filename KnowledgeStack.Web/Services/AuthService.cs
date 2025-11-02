using BCrypt.Net;
using KnowledgeStack.Web.Data.Repositories;
using KnowledgeStack.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeStack.Web.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(string username, string email, string password);
        Task<User?> ValidateUserAsync(string usernameOrEmail, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> RegisterAsync(string username, string email, string password)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
            if (exists) return null;
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
            if (user == null) return null;
            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return ok ? user : null;
        }
    }
}



