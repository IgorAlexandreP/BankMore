namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Account.API.Infrastructure.Repositories;
using BankMore.Account.API.Infrastructure.Services;

public class InactivateAccountHandler : IRequestHandler<InactivateAccountCommand, Result>
{
    private readonly IAccountRepository _repository;
    private readonly IPasswordService _passwordService;

    public InactivateAccountHandler(IAccountRepository repository, IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<Result> Handle(InactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId);
        if (account == null)
            return Result.Failure("Conta não encontrada", "INVALID_ACCOUNT");
            
        if (!_passwordService.VerifyPassword(request.Senha, account.Senha))
            return Result.Failure("Senha inválida", "INVALID_CREDENTIALS");
            
        account.Inactivate();
        await _repository.UpdateAsync(account);
        
        return Result.Success();
    }
}
