namespace WorkerService.Domain.Models;

public class MyEntity
{
    public int Id { get; set; }
    public bool IsProcessed { get; set; }
    public string Name { get; set; } = string.Empty;
}
