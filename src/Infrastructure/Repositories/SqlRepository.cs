using WorkerService.Application.Interfaces;
using WorkerService.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService.Infrastructure.Repositories;

public class SqlRepository : IRepository
{
    private readonly List<MyEntity> _entities = new()
    {
        new MyEntity { Id = 1, IsProcessed = false, Name = "Job1" },
        new MyEntity { Id = 2, IsProcessed = false, Name = "Job2" }
    };

    public Task<IEnumerable<MyEntity>> GetPendingItemsAsync(CancellationToken cancellationToken)
    {
        var pending = _entities.Where(e => !e.IsProcessed);
        return Task.FromResult(pending.AsEnumerable());
    }

    public Task MarkAsProcessedAsync(int id, CancellationToken cancellationToken)
    {
        var entity = _entities.FirstOrDefault(e => e.Id == id);
        if (entity != null)
        {
            entity.IsProcessed = true;
        }
        return Task.CompletedTask;
    }
}
