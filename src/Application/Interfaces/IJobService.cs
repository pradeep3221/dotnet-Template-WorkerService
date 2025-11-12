using System.Threading;
using System.Threading.Tasks;

namespace WorkerService.Application.Interfaces;

public interface IJobService
{
    Task RunJobsAsync(CancellationToken cancellationToken);
}
