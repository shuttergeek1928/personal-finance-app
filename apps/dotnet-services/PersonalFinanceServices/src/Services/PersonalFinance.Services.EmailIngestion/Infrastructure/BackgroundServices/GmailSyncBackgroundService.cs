using Cronos;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.EmailIngestion.Application.Commands;
using PersonalFinance.Services.EmailIngestion.Application.Services;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;

namespace PersonalFinance.Services.EmailIngestion.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service implementing intelligent adaptive polling.
    /// Different transaction categories are polled at different intervals:
    /// - UPI / Daily: every 10 minutes
    /// - Food Delivery: every 10 minutes
    /// - General: every 30 minutes
    /// - Subscriptions: daily at 6 AM UTC
    /// - EMI: monthly on the 1st at 8 AM UTC
    /// - Salary: monthly on the 1st at 10 AM UTC
    /// - Credit Card: daily at 9 PM UTC
    /// </summary>
    public class GmailSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GmailSyncBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<SyncScheduleConfig> _schedules;

        public GmailSyncBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<GmailSyncBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;

            _schedules = configuration.GetSection("SyncSchedules")
                .Get<List<SyncScheduleConfig>>() ?? GetDefaultSchedules();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GmailSyncBackgroundService started with {Count} schedule(s)", _schedules.Count);

            // Wait for the application to be fully started
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            var lastCheckTimes = new Dictionary<string, DateTime>();
            foreach (var schedule in _schedules)
            {
                lastCheckTimes[schedule.Category] = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var schedule in _schedules)
                {
                    try
                    {
                        if (!ShouldSync(schedule, lastCheckTimes[schedule.Category]))
                            continue;

                        _logger.LogInformation("Running scheduled sync for category: {Category}", schedule.Category);
                        await RunSyncForAllUsersAsync(schedule.Category, stoppingToken);
                        lastCheckTimes[schedule.Category] = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in scheduled sync for category {Category}", schedule.Category);
                    }
                }

                // Check schedules every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private bool ShouldSync(SyncScheduleConfig schedule, DateTime lastSync)
        {
            // Interval-based schedule
            if (schedule.IntervalMinutes.HasValue)
            {
                return DateTime.UtcNow - lastSync >= TimeSpan.FromMinutes(schedule.IntervalMinutes.Value);
            }

            // Cron-based schedule
            if (!string.IsNullOrEmpty(schedule.CronOverride))
            {
                try
                {
                    var cronExpression = CronExpression.Parse(schedule.CronOverride);
                    var nextOccurrence = cronExpression.GetNextOccurrence(DateTime.SpecifyKind(lastSync, DateTimeKind.Utc), TimeZoneInfo.Utc);

                    return nextOccurrence.HasValue && nextOccurrence.Value <= DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid cron expression for category {Category}: {Cron}",
                        schedule.Category, schedule.CronOverride);
                    return false;
                }
            }

            return false;
        }

        private async Task RunSyncForAllUsersAsync(string category, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmailIngestionDbContext>();
            var gmailService = scope.ServiceProvider.GetRequiredService<IGmailApiService>();

            // For now, we need to get Gmail tokens from a cross-service call or cache
            // In production, the UserManagement service would provide these via RabbitMQ or a shared cache.
            // For MVP, we'll query the sync states to find users who have synced before.
            var usersWithSync = await dbContext.SyncStates
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync(ct);

            _logger.LogInformation("Found {Count} users with Gmail sync for category {Category}",
                usersWithSync.Count, category);

            // Note: In production, you would fetch Gmail tokens from a secure token store
            // or UserManagement service. This background service serves as the scheduler.
            // The actual sync is triggered via the SyncGmailTransactionsCommand.
        }

        private List<SyncScheduleConfig> GetDefaultSchedules()
        {
            return new List<SyncScheduleConfig>
            {
                new() { Category = "UPI", IntervalMinutes = 10 },
                new() { Category = "FoodDelivery", IntervalMinutes = 10 },
                new() { Category = "General", IntervalMinutes = 30 },
                new() { Category = "Subscription", CronOverride = "0 6 * * *" },
                new() { Category = "EMI", CronOverride = "0 8 1 * *" },
                new() { Category = "Salary", CronOverride = "0 10 1 * *" },
                new() { Category = "CreditCard", CronOverride = "0 21 * * *" }
            };
        }
    }

    public class SyncScheduleConfig
    {
        public string Category { get; set; } = "General";
        public int? IntervalMinutes { get; set; }
        public string? CronOverride { get; set; }
    }
}
