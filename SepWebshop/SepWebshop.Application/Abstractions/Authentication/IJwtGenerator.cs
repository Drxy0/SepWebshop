namespace SepWebshop.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    string Generate(Guid userId, string email);
}
