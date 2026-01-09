using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace DataService.Helpers;

public class JwtGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInMinutes;

    public JwtGenerator(IConfiguration config)
    {
        _secretKey = config["Jwt:SecretKey"] ?? throw new ArgumentNullException("JWT Secret Key is not configured.");
        _issuer = config["Jwt:Issuer"] ?? throw new ArgumentNullException("JWT Issuer is not configured.");
        _audience = config["Jwt:Audience"] ?? throw new ArgumentNullException("JWT Audience is not configured.");
        _expiresInMinutes = int.Parse(config["Jwt:ExpiresInMinutes"] ?? "60");

    }

    public string GenerateAccessToken(Guid userId, string username)
    {
        var handler = new JsonWebTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
        };


        string token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.AddMinutes(_expiresInMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(claims)
        });

        return token;
    }
}
