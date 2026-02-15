namespace BankMore.Tariff.Worker.Consumers;

using KafkaFlow;
using KafkaFlow.Producers;
using BankMore.Tariff.Worker.Domain;
using BankMore.Tariff.Worker.Infrastructure.Repositories;
using BankMore.Core.Shared;
using System.Text.Json;

using System.Text.Json.Serialization;

public class TransferenciaRealizadaEvent
{
    [JsonPropertyName("IdRequisicao")]
    public required string RequestId { get; set; }
    
    [JsonPropertyName("IdContaCorrente")]
    public required string AccountId { get; set; }
    
    [JsonPropertyName("Valor")]
    public decimal Value { get; set; }
    
    [JsonPropertyName("DataHora")]
    public DateTime Date { get; set; }
}

public class TarifacaoRealizadaEvent
{
    [JsonPropertyName("AccountId")]
    public required string AccountId { get; set; }
    
    [JsonPropertyName("ValorTarifado")]
    public decimal ValorTarifado { get; set; }
}

public class TransferenciaRealizadaConsumer : IMessageHandler<TransferenciaRealizadaEvent>
{
    private readonly ITariffRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TransferenciaRealizadaConsumer> _logger;
    private readonly IProducerAccessor _producerAccessor;

    public TransferenciaRealizadaConsumer(ITariffRepository repository, IConfiguration configuration, ILogger<TransferenciaRealizadaConsumer> logger, IProducerAccessor producerAccessor)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
        _producerAccessor = producerAccessor;
    }

    public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
    {
        if (message != null)
        {
            _logger.LogInformation($"Processing tariff for transfer {message.RequestId}");

            var tariffValue = _configuration.GetValue<decimal>("TariffValue");
            
            var tarifa = Tarifa.Create(message.AccountId, tariffValue);
            await _repository.AddAsync(tarifa);

            var producer = _producerAccessor.GetProducer("tarifa-producer");
            await producer.ProduceAsync("tarifacoes-realizadas", Guid.NewGuid().ToString(), 
                new TarifacaoRealizadaEvent { AccountId = message.AccountId, ValorTarifado = tariffValue });
        }
    }
}
