using Cila;

public class EventsAggregatorWorkerService : BackgroundService
{
    private const int generalDelay = 1 * 5 * 1000; // 5 seconds
    private readonly IServiceLocator _locator;

    public EventsAggregatorWorkerService()
    {
        this._locator =  new ServiceLocator(Program._serviceProvider);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _aggregatorService = _locator.GetService<AggregatorService>();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(generalDelay, stoppingToken);
            await PullEvents(_aggregatorService);
        }
    }

    private async Task PullEvents(AggregatorService aggregatorService)
    {
        await aggregatorService.Aggregate();
    }
}