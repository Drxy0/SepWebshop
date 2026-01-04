using DataService.Contracts;
using DataService.Helpers;
using DataService.Models;
using DataService.Persistance;
using DataService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataService.Services;

public class UserService : IUserService
{
    private readonly DataServiceDbContext _context;
    private readonly JwtGenerator _jwtGenerator;
    public UserService(DataServiceDbContext context, JwtGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<LoginDto> Login(string username, string password)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return new LoginDto(false, "Invalid credentials");
        }

        if (!PasswordHasher.Verify(user.PasswordHash, password))
        {
            return new LoginDto(false, "Invalid credentials");
        }

        string accessToken = _jwtGenerator.GenerateAccessToken(user.Id, user.Username);

        return new LoginDto(true, accessToken);
    }
}
