using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WorkerService.Infrastructure.Logging;
using WorkerService.Application.Services;

Log.Logger = SerilogConfiguration.CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IRepository, SqlRepository>();
        services.AddHostedService<WorkerService.WorkerService>();
    })
    .Build();

await host.RunAsync();
