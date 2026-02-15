namespace BankMore.Account.API.Consumers;

using BankMore.Account.API.Domain;
using BankMore.Account.API.Infrastructure.Repositories;
using BankMore.Core.Shared;
using KafkaFlow;

public class TariffCalculatedConsumer : IMessageHandler<TarifaCalculadaEvent>
{
    private readonly IAccountRepository _repository;

    public TariffCalculatedConsumer(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(IMessageContext context, TarifaCalculadaEvent message)
    {
        var movimento = Movimento.Create(message.IdContaCorrente, "D", message.Valor);
        await _repository.AddMovimentoAsync(movimento);
    }
}
