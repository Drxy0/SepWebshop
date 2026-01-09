using DataService.Contracts;

namespace DataService.Services.Interfaces;

public interface IUserService
{
    Task<LoginDto> Login(string username, string password);
    Task<bool> Register(RegisterRequest request);
    Task<List<PaymentMethodDto>> GetUserPaymentMethods(Guid userId);
    Task<bool> UpdateUserPaymentMethods(Guid userId, List<int> methodIds);
    Task<List<string>> GetActiveMethodsByMerchantId(string merchantId);
}
