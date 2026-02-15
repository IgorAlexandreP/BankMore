namespace BankMore.Tariff.Worker;

using KafkaFlow;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IKafkaBus _bus;

    public Worker(ILogger<Worker> logger, IKafkaBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _bus.StartAsync(cancellationToken);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _bus.StopAsync();
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}

