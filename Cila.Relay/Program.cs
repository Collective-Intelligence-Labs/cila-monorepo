
using Cila;
using Confluent.Kafka;

internal class Program
{
    private static OmniChainSettings? _appSettings;
    private static RelayService _relayService;

    private static void Main(string[] args)
    {
        var cnfg = new ConfigurationBuilder();
        var configuration = cnfg.AddJsonFile("settings.json")
            .Build();
        _appSettings = configuration.GetSection("Settings").Get<OmniChainSettings>();
        Console.WriteLine("Number of chains in settiings: {0}", _appSettings.Chains.Count);
        var relayService = new RelayService(_appSettings);
        var timer = new Timer(ExecuteTask, relayService, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        // Wait indefinitely
        Console.WriteLine("Press Ctrl+C to exit.");
        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, e) => exitEvent.Set();
        exitEvent.WaitOne();
    }

    private static void ExecuteTask(object state)
    {
        var relayService = (RelayService)state;
        relayService.SyncAllChains();
        Console.WriteLine($"Task executed at {DateTime.UtcNow}");
    }
}