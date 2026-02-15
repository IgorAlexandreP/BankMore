namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Account.API.Domain;
using BankMore.Account.API.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class MakeTransactionHandler : IRequestHandler<MakeTransactionCommand, Result>
{
    private readonly IAccountRepository _repository;
    private readonly IMemoryCache _cache;

    public MakeTransactionHandler(IAccountRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result> Handle(MakeTransactionCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.IsIdempotentAsync(request.RequestId))
            return Result.Success(); 

        ContaCorrente? targetAccount = null;
        if (!string.IsNullOrEmpty(request.AccountNumber))
        {
            if (int.TryParse(request.AccountNumber, out int num))
                targetAccount = await _repository.GetByNumberAsync(num);
        }
        else
        {
            targetAccount = await _repository.GetByIdAsync(request.AuthenticatedAccountId);
        }

        if (targetAccount == null)
            return Result.Failure("Conta não encontrada", "INVALID_ACCOUNT");

        if (!targetAccount.Ativo)
            return Result.Failure("Conta inativa", "INACTIVE_ACCOUNT");

        if (request.Value <= 0)
            return Result.Failure("Valor deve ser positivo", "INVALID_VALUE");

        if (request.Type != "C" && request.Type != "D")
             return Result.Failure("Tipo de transação inválido", "INVALID_TYPE");

        if (targetAccount.Id != request.AuthenticatedAccountId && request.Type == "D")
             return Result.Failure("Não é possível debitar de outra conta", "INVALID_TYPE");

        if (request.Type == "D")
        {
            var movimentos = await _repository.GetMovimentosAsync(targetAccount.Id);
            var balance = movimentos.Sum(m => m.TipoMovimento == "C" ? m.Valor : -m.Valor);
            if (balance < request.Value)
                 return Result.Failure("Saldo insuficiente", "INSUFFICIENT_FUNDS");
        }

        var movimento = Movimento.Create(targetAccount.Id, request.Type, request.Value);
        await _repository.AddMovimentoAsync(movimento);
        
        await _repository.RegisterIdempotencyAsync(request.RequestId, "Request", "Success");

        _cache.Remove($"balance_{targetAccount.Id}");

        return Result.Success();
    }
}
