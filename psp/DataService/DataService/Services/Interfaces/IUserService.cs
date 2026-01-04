using DataService.Contracts;

namespace DataService.Services.Interfaces;

public interface IUserService
{
    Task<LoginDto> Login(string username, string password);
}
