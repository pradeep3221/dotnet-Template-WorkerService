using Xunit;
using WorkerService.Infrastructure.Repositories;
using System.Threading.Tasks;

public class RepositoryIntegrationTests
{
    [Fact]
    public async Task MarkAsProcessedAsync_ShouldUpdateEntity()
    {
        var repo = new SqlRepository();
        var items = await repo.GetPendingItemsAsync(default);
        var first = items.FirstOrDefault();
        Assert.NotNull(first);
        await repo.MarkAsProcessedAsync(first.Id, default);
        var updated = await repo.GetPendingItemsAsync(default);
        Assert.DoesNotContain(updated, x => x.Id == first.Id);
    }
}
