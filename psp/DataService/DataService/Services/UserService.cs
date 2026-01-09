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
    public async Task<bool> Register(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username == request.Username || u.MearchantId == request.MearchantId);

        if (existingUser) return false;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            MearchantId = request.MearchantId,
            PasswordHash = PasswordHasher.Hash(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<List<PaymentMethodDto>> GetUserPaymentMethods(Guid userId)
    {
        var allMethods = await _context.PaymentMethods.ToListAsync();

        var user = await _context.Users
            .Include(u => u.PaymentMethods)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return new List<PaymentMethodDto>();

        var userMethodIds = user.PaymentMethods.Select(m => m.Id).ToList();

        return allMethods.Select(m => new PaymentMethodDto(
            m.Id,
            m.Name,
            userMethodIds.Contains(m.Id)
        )).ToList();
    }

    public async Task<bool> UpdateUserPaymentMethods(Guid userId, List<int> methodIds)
    {
        if (methodIds == null || !methodIds.Any())
        {
            return false;
        }

        var user = await _context.Users
            .Include(u => u.PaymentMethods)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        var selectedMethods = await _context.PaymentMethods
            .Where(m => methodIds.Contains(m.Id))
            .ToListAsync();

        if (!selectedMethods.Any())
        {
            return false;
        }

        user.PaymentMethods = selectedMethods;
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<List<string>> GetActiveMethodsByMerchantId(string merchantId)
    {
        var user = await _context.Users
            .Include(u => u.PaymentMethods)
            .FirstOrDefaultAsync(u => u.MearchantId == merchantId);

        if (user == null) return new List<string>();

        // Vraćamo samo listu imena: ["Card", "PayPal", "Crypto"]
        return user.PaymentMethods
            .Select(m => m.Name)
            .ToList();
    }
}
