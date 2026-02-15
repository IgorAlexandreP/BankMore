namespace BankMore.Account.API.Consumers;

using BankMore.Account.API.Domain;
using BankMore.Account.API.Infrastructure.Repositories;
using KafkaFlow;
using Microsoft.Extensions.Caching.Memory;

public class TarifacaoRealizadaEvent
{
    public required string AccountId { get; set; }
    public decimal ValorTarifado { get; set; }
}

public class TarifacaoRealizadaConsumer : IMessageHandler<TarifacaoRealizadaEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TarifacaoRealizadaConsumer> _logger;
    private readonly IMemoryCache _cache;

    public TarifacaoRealizadaConsumer(IServiceProvider serviceProvider, ILogger<TarifacaoRealizadaConsumer> logger, IMemoryCache cache)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(IMessageContext context, TarifacaoRealizadaEvent message)
    {
        if (message != null)
        {
            _logger.LogInformation($"Applying tariff for account {message.AccountId}");
            
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var movimento = Movimento.Create(message.AccountId, "D", message.ValorTarifado);
            await repository.AddMovimentoAsync(movimento);

            _cache.Remove($"balance_{message.AccountId}");
        }
    }
}
