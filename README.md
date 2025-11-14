# Async-Safe Worker Service Template

A redistributable .NET 9 Worker Service template following Clean Architecture principles and async-safe defaults.
<p>
For Background services or Microservice

## Features
- **Async Execution:** All services and repositories use async/await for scalability and reliability.
- **Clean Architecture:** Separation of concerns across Application, Domain, Infrastructure, and Shared layers.
- **Configurable Scheduling:** Control job interval and concurrency via `appsettings.json`.
- **Dependency Injection:** Easily swap implementations for queues, repositories, and services.
- **Graceful Shutdown:** Cancellation tokens passed throughout for safe termination.
- **Logging:** Structured logging with Serilog (default).
- **Unit & Integration Tests:** Pre-wired test projects for rapid validation.

## Usage
1. **Install the template:**
   ```pwsh
   dotnet new install .
   ```
2. **Create a new worker project:**
   ```pwsh
   dotnet new worker-async -n MyBackgroundWorker
   ```
3. **Configure settings:**
   Edit `src/Shared/Configuration/appsettings.json` for interval and concurrency.

## Customization Options
- **Queue Type:** RabbitMQ, Kafka, SQS, Azure Queue
- **Scheduler Type:** Cron, Timed Loop
- **Logging Framework:** Serilog, NLog
- **ORM/Database:** EF Core, Dapper, None
- **Health Checks:** Enable endpoints to monitor DB, queue, and service health.
- **Metrics:** Integrate Prometheus or OpenTelemetry for monitoring and observability.
- **Distributed Locking:** Use Redis, SQL, or in-memory locking for safe concurrency.
- **Retry Policies:** Polly or custom retry logic for resilient operations.
- **Dead Letter Queue (DLQ):** Route failed messages to a DLQ for later inspection.
- **Custom Startup Tasks:** Run migrations, warm-up routines, or other startup logic.

## Advanced Features Overview

### Health Checks
Enable health endpoints to monitor the status of dependencies (database, queue, etc.). Useful for Kubernetes and cloud deployments.

### Metrics
Integrate Prometheus or OpenTelemetry to collect and expose metrics for job execution, errors, and performance.

### Distributed Locking
Prevent duplicate job execution across multiple worker instances using Redis, SQL, or in-memory locks.

### Retry Policies
Use Polly or custom logic to automatically retry failed operations (queue, DB, etc.) with configurable backoff.

### Dead Letter Queue (DLQ)
Failed messages can be routed to a DLQ for later analysis and recovery, improving reliability.

### Custom Startup Tasks
Run database migrations, cache warm-up, or other initialization logic before the worker starts processing jobs.

All features are configurable via template parameters and `appsettings.json`.

## Folder Structure
```
/src
  /Worker
  /Application
  /Domain
  /Infrastructure
  /Shared
/tests
```

## Example Worker Loop
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await _jobService.RunJobsAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

## Async Best Practices in Worker Services

### Why Async Matters
Async/await enables scalable, non-blocking background processing. It prevents thread starvation, deadlocks, and improves reliability in production workloads.

### Key Patterns
- **Always use `async Task` for service/repository methods.**
- **Never block with `.Result` or `.Wait()`.** Use `await` everywhere.
- **Pass `CancellationToken` through all async calls** for graceful shutdown.
- **Use `Task.Delay` instead of `Thread.Sleep`** for intervals.
- **For concurrency, use `Task.WhenAll` or `SemaphoreSlim`** to control parallelism.

### Example: Async Service
```csharp
public async Task RunJobsAsync(CancellationToken cancellationToken)
{
   var items = await _repository.GetPendingItemsAsync(cancellationToken);
   var tasks = items.Select(item => ProcessItemAsync(item, cancellationToken));
   await Task.WhenAll(tasks);
}

private async Task ProcessItemAsync(MyEntity item, CancellationToken token)
{
   await Task.Delay(500, token); // Simulate async I/O
   await _repository.MarkAsProcessedAsync(item.Id, token);
}
```

### Common Async Pitfalls
| ❌ Anti-Pattern                          | ✅ Recommended              |
| --------------------------------------- | -------------------------- |
| `.Result` or `.Wait()` on tasks         | Always use `await`         |
| `async void` methods                    | Use `async Task`           |
| `Task.Run()` inside `BackgroundService` | Use built-in async pattern |
| `Thread.Sleep()`                        | Use `await Task.Delay()`   |
| Ignoring `CancellationToken`            | Pass it through everywhere |

### Graceful Shutdown
When the app stops, the cancellation token is triggered and the worker loop exits cleanly. Always check `stoppingToken.IsCancellationRequested` and pass the token to all async operations.

### Advanced: Throttled Parallelism
```csharp
var throttler = new SemaphoreSlim(3); // max 3 concurrent
var tasks = items.Select(async item =>
{
   await throttler.WaitAsync(cancellationToken);
   try
   {
      await ProcessItemAsync(item, cancellationToken);
   }
   finally
   {
      throttler.Release();
   }
});
await Task.WhenAll(tasks);
```

### Summary Table
| Area           | Async Practice                          | Benefit                         |
| -------------- | --------------------------------------- | ------------------------------- |
| Worker Loop    | `ExecuteAsync` with `await`             | Non-blocking background loop    |
| Services       | Fully async interfaces                  | End-to-end async flow           |
| I/O Operations | `await` DB, queue, and network calls    | Scalable and responsive         |
| Shutdown       | Use `CancellationToken`                 | Graceful termination            |
| Concurrency    | `Task.WhenAll` or throttled parallelism | Efficient multi-item processing |

## Advanced Feature Samples & Technical Details

### Health Checks
**Sample Registration:**
```csharp
services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddRabbitMQ(rabbitMqConnectionString);
```
**Configuration:**
```json
{
  "HealthChecks": {
    "Enabled": true,
    "Endpoints": ["/health"]
  }
}
```
**Explanation:**
Health checks allow external systems (e.g., Kubernetes) to monitor service health and trigger restarts if dependencies fail.

---
### Metrics
**Sample Registration:**
```csharp
services.AddOpenTelemetryMetrics(builder =>
{
    builder.AddAspNetCoreInstrumentation();
    builder.AddPrometheusExporter();
});
```
**Configuration:**
```json
{
  "Metrics": {
    "Provider": "Prometheus",
    "Enabled": true
  }
}
```
**Explanation:**
Metrics provide visibility into job throughput, error rates, and performance. Prometheus/OpenTelemetry can scrape and visualize these metrics.

---
### Distributed Locking
**Sample Usage:**
```csharp
await _distributedLock.AcquireAsync("job-lock", cancellationToken);
try {
    // Process job
} finally {
    await _distributedLock.ReleaseAsync("job-lock");
}
```
**Configuration:**
```json
{
  "DistributedLocking": {
    "Provider": "Redis",
    "ConnectionString": "localhost:6379"
  }
}
```
**Explanation:**
Distributed locks prevent multiple workers from processing the same job concurrently, ensuring data consistency in distributed environments.

---
### Retry Policies
**Sample Polly Policy:**
```csharp
var policy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt));
await policy.ExecuteAsync(() => _queueService.ReceiveMessagesAsync());
```
**Configuration:**
```json
{
  "RetryPolicy": {
    "Type": "Polly",
    "MaxRetries": 3,
    "BackoffSeconds": 2
  }
}
```
**Explanation:**
Retry policies automatically handle transient failures, improving reliability for queue and database operations.

---
### Dead Letter Queue (DLQ)
**Sample Usage:**
```csharp
if (processingFailed)
    await _dlqService.SendAsync(failedMessage);
```
**Configuration:**
```json
{
  "DeadLetterQueue": {
    "Enabled": true,
    "QueueName": "dlq-jobs"
  }
}
```
**Explanation:**
DLQ stores failed messages for later inspection and recovery, preventing data loss and enabling troubleshooting.

---
### Custom Startup Tasks
**Sample Registration:**
```csharp
services.AddHostedService<MigrationStartupTask>();
```
**Configuration:**
```json
{
  "StartupTasks": {
    "RunMigrations": true,
    "WarmUpCache": true
  }
}
```
**Explanation:**
Custom startup tasks allow you to run migrations, warm-up routines, or other initialization logic before the worker starts processing jobs.

---

## License
MIT

