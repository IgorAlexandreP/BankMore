namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Core.Domain;
using BankMore.Account.API.Domain;
using BankMore.Account.API.Infrastructure.Repositories;
using BankMore.Account.API.Infrastructure.Services;

public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Result<int>>
{
    private readonly IAccountRepository _repository;
    private readonly IPasswordService _passwordService;

    public CreateAccountHandler(IAccountRepository repository, IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<Result<int>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var cpfResult = Cpf.Create(request.Cpf);
        if (!cpfResult.IsSuccess)
            return Result<int>.Failure(cpfResult.Error, cpfResult.ErrorType);

        var passwordResult = Password.Create(request.Senha);
        if (!passwordResult.IsSuccess)
            return Result<int>.Failure(passwordResult.Error, passwordResult.ErrorType);

        var existing = await _repository.GetByCpfAsync(cpfResult.Value.Value);
        if (existing != null)
            return Result<int>.Failure("Conta já existe para este CPF", "DUPLICATE_ACCOUNT");

        var salt = Guid.NewGuid().ToString();
        var hash = _passwordService.HashPassword(passwordResult.Value.Value);

        var account = ContaCorrente.Create(cpfResult.Value.Value, request.Nome, hash, salt);

        await _repository.AddAsync(account);

        return Result<int>.Success(account.Numero);
    }
}
