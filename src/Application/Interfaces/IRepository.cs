using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkerService.Domain.Models;

namespace WorkerService.Application.Interfaces;

public interface IRepository
{
    Task<IEnumerable<MyEntity>> GetPendingItemsAsync(CancellationToken cancellationToken);
    Task MarkAsProcessedAsync(int id, CancellationToken cancellationToken);
}
