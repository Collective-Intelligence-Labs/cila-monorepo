using Cila;

public class InfrastructureEventsWorkerService : BackgroundService
{
    private KafkaConsumer _consumer;

    public InfrastructureEventsWorkerService(KafkaConsumer consumer)
    {
        this._consumer =  consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.ConsumeAsync("infr", stoppingToken);
    }
}