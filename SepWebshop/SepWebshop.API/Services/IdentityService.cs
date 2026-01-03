using SepWebshop.Application.Abstractions.IdentityService;
using System.Security.Claims;

namespace SepWebshop.API.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                          ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public string? UserIdentity => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
}
