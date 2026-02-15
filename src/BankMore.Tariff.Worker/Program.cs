using BankMore.Tariff.Worker;
using BankMore.Tariff.Worker.Infrastructure;
using BankMore.Tariff.Worker.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;
using BankMore.Tariff.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITariffRepository, TariffRepository>();
builder.Services.AddSingleton<DbInitializer>();

builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092" })
        .CreateTopicIfNotExists("transferencias-realizadas", 1, 1)
        .CreateTopicIfNotExists("tarifacoes-realizadas", 1, 1)
        .AddConsumer(consumer => consumer
            .Topic("transferencias-realizadas")
            .WithGroupId("tariff-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<BankMore.Tariff.Worker.Infrastructure.CustomTransferenciaDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<TransferenciaRealizadaConsumer>())
            )
        )
        .AddProducer("tarifa-producer", producer => producer
            .DefaultTopic("tarifacoes-realizadas")
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
    )
);

builder.Services.AddSingleton<IKafkaBus>(sp => sp.CreateKafkaBus());

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

host.Run();
