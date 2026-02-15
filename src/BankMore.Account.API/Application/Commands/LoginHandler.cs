namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Account.API.Domain;
using BankMore.Account.API.Infrastructure.Repositories;
using BankMore.Account.API.Infrastructure.Services;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IAccountRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginHandler(IAccountRepository repository, IPasswordService passwordService, ITokenService tokenService)
    {
        _repository = repository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ContaCorrente? account = null;

        if (request.Login.Length < 11 && int.TryParse(request.Login, out int accountNumber))
        {
            account = await _repository.GetByNumberAsync(accountNumber);
        }
        else
        {
             var cpfClean = request.Login.Replace(".", "").Replace("-", "");
             account = await _repository.GetByCpfAsync(cpfClean);
        }

        if (account == null)
            return Result<string>.Failure("Credenciais inválidas", "USER_UNAUTHORIZED");

        if (!_passwordService.VerifyPassword(request.Senha, account.Senha))
            return Result<string>.Failure("Credenciais inválidas", "USER_UNAUTHORIZED");

        var token = _tokenService.GenerateToken(account);
        return Result<string>.Success(token);
    }
}
