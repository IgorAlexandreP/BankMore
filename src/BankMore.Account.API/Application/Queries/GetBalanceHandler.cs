namespace BankMore.Account.API.Application.Queries;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Account.API.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, Result<BalanceDto>>
{
    private readonly IAccountRepository _repository;
    private readonly IMemoryCache _cache;

    public GetBalanceHandler(IAccountRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Result<BalanceDto>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"balance_{request.AccountId}";
        
        if (_cache.TryGetValue(cacheKey, out Result<BalanceDto>? cachedResult))
        {
            if (cachedResult != null)
                return cachedResult;
        }

        var account = await _repository.GetByIdAsync(request.AccountId);
        if (account == null)
            return Result<BalanceDto>.Failure("Conta não encontrada", "INVALID_ACCOUNT");
             
        if (!account.Ativo)
            return Result<BalanceDto>.Failure("Conta inativa", "INACTIVE_ACCOUNT");

        var movimentos = await _repository.GetMovimentosAsync(request.AccountId);
        var balance = movimentos.Sum(m => m.TipoMovimento == "C" ? m.Valor : -m.Valor);

        var result = Result<BalanceDto>.Success(new BalanceDto(
            account.Numero,
            account.Nome,
            DateTime.Now,
            balance
        ));

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(10)) // Cache por 10 segundos
            .SetSlidingExpiration(TimeSpan.FromSeconds(5));

        _cache.Set(cacheKey, result, cacheEntryOptions);

        return result;
    }
}
