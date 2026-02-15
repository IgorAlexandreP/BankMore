namespace BankMore.Account.API.Infrastructure.Services;

using BankMore.Account.API.Domain;

public interface ITokenService
{
    string GenerateToken(ContaCorrente account);
}
