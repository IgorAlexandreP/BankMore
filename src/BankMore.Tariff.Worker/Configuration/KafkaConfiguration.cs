namespace BankMore.Tariff.Worker.Configuration;

using KafkaFlow;
using KafkaFlow.Serializer;
using BankMore.Tariff.Worker.Consumers;

public static class KafkaConfiguration
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var brokers = configuration["Kafka:Brokers"];

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { brokers })
                .CreateTopicIfNotExists("transferencias-realizadas", 1, 1)
                .CreateTopicIfNotExists("tarifacoes-realizadas", 1, 1)
                .AddConsumer(consumer => consumer
                    .Topic("transferencias-realizadas")
                    .WithGroupId("tariff-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(1)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<NewtonsoftJsonDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<TransferenciaRealizadaConsumer>())
                    )
                )
                .AddProducer("tarifa-producer", producer => producer
                    .DefaultTopic("tarifacoes-realizadas")
                    .AddMiddlewares(m => m.AddSerializer<NewtonsoftJsonSerializer>())
                )
            )
        );

        return services;
    }
}
