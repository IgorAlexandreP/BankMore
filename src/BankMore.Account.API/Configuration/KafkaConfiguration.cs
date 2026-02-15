namespace BankMore.Account.API.Configuration;

using KafkaFlow;
using KafkaFlow.Serializer;
using BankMore.Account.API.Consumers;

public static class KafkaConfiguration
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var brokers = configuration["Kafka:Brokers"];

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { brokers })
                .AddConsumer(consumer => consumer
                    .Topic("tarifacoes-realizadas")
                    .WithGroupId("account-tariff-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(1)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<NewtonsoftJsonDeserializer>()
                        .AddTypedHandlers(h => h.WithHandlerLifetime(InstanceLifetime.Scoped).AddHandler<TarifacaoRealizadaConsumer>())
                    )
                )
            )
        );

        return services;
    }
}
