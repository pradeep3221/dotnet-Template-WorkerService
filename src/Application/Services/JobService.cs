using WorkerService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WorkerService.Application.Services;

public class JobService : IJobService
{
    private readonly IRepository _repository;
    private readonly ILogger<JobService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public JobService(IRepository repository, ILogger<JobService> logger)
    {
        _repository = repository;
        _logger = logger;
        _semaphore = new SemaphoreSlim(3); // Max concurrency configurable
    }

    public async Task RunJobsAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var items = await _repository.GetPendingItemsAsync(cancellationToken);
            var tasks = new List<Task>();
            foreach (var item in items)
            {
                tasks.Add(ProcessItemAsync(item, cancellationToken));
            }
            await Task.WhenAll(tasks);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessItemAsync(MyEntity item, CancellationToken token)
    {
        _logger.LogInformation($"Processing item {item.Id}");
        await Task.Delay(500, token); // Simulate async I/O
        await _repository.MarkAsProcessedAsync(item.Id, token);
    }
}
