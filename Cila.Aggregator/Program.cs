using Cila;
using Cila.Database;
using Cila.Domain.MessageQueue;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    public static OmniChainSettings AppSettings {get;set;}

    public static ServiceProvider _serviceProvider;

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var cnfg = new ConfigurationBuilder();
        var configuration = cnfg.AddJsonFile("settings.json")
            .Build();
        AppSettings = configuration.GetSection("Settings").Get<OmniChainSettings>();

        var services = builder.Services;

        // Register your dependencies here
        services.AddSingleton<IServiceLocator,ServiceLocator>();
        services.AddSingleton(AppSettings);
        services.AddScoped<AggregatorService>()
            .AddScoped<ChainsService>()
            .AddScoped<AggregagtedEventsService>()
            .AddScoped<ChainClientsFactory>();
            
        services.AddSingleton<EventsDispatcher>();

        services.AddScoped<KafkaConfigProvider>();
        services.AddSingleton<KafkaConsumer>();
        services.AddSingleton<KafkaProducer>();

        services.AddTransient<InfrastructureEventsHandler>();
        services.AddTransient<EventsHandler>();
        services.AddTransient<MongoDatabase>();
    
        // Build the service provider
        
        services.AddSingleton<ServiceProvider>(x=> _serviceProvider);
        _serviceProvider = services.BuildServiceProvider();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddCors();
        
        builder.Services.AddHostedService<EventsAggregatorWorkerService>();
        builder.Services.AddHostedService<InfrastructureEventsWorkerService>();

        //Initialize chains in database
        _serviceProvider.GetService<ChainsService>().InitializeFromSettings(AppSettings);

        var app = builder.Build();

        

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
    }
}