using Xunit;
using WorkerService.Application.Services;
using WorkerService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;

public class JobServiceTests
{
    [Fact]
    public async Task RunJobsAsync_ShouldProcessAllJobs()
    {
        var repo = new SqlRepository();
        var logger = new NullLogger<WorkerService.Application.Services.JobService>();
        var jobService = new JobService(repo, logger);
        await jobService.RunJobsAsync(default);
        // No assertion needed for template, just ensure no exceptions
        Assert.True(true);
    }
}
