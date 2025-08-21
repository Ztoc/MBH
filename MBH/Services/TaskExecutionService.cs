using Microsoft.EntityFrameworkCore;

namespace MBH.Services
{
    public class TaskExecutionService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<TaskExecutionService> _logger;

        public TaskExecutionService(IServiceProvider sp, ILogger<TaskExecutionService> logger)
        {
            _sp = sp; _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TaskExecutionService started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
                    var now = DateTime.UtcNow;

                    var dueTasks = await db.ScheduledTasks
                        .Where(t => !t.IsExecuted && t.ScheduledTime <= now)
                        .OrderBy(t => t.ScheduledTime)
                        .ToListAsync(stoppingToken);

                    foreach (var t in dueTasks)
                    {
                        _logger.LogInformation("Executing task {Id}: {Title} (scheduled {Scheduled})",
                            t.Id, t.Title, t.ScheduledTime);

                        t.IsExecuted = true;
                        t.ExecutedAt = now;
                    }

                    if (dueTasks.Count > 0)
                        await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in TaskExecutionService loop");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
