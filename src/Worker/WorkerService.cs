using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService;

public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly IJobService _jobService;

    public WorkerService(ILogger<WorkerService> logger, IJobService jobService)
    {
        _logger = logger;
        _jobService = jobService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _jobService.RunJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running job service.");
            }
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
        _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);
    }
}
