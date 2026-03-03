using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModulerMonolith.Infrastructure.Persistence;

namespace ModulerMonolith.Infrastructure.Outbox;

internal sealed class OutboxProcessor : BackgroundService
{
    private const int MaxRetries = 5;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly string _webhookUrl;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<OutboxProcessor> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _webhookUrl = configuration["N8n:WebhookUrl"]
            ?? throw new InvalidOperationException("N8n:WebhookUrl configuration is missing.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started. Webhook: {WebhookUrl}", _webhookUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingMessagesAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        var client = _httpClientFactory.CreateClient("n8n");

        foreach (var message in pending)
        {
            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    eventType = message.EventType,
                    payload = JsonSerializer.Deserialize<object>(message.Payload),
                    createdAt = message.CreatedAt
                });

                var response = await client.PostAsync(
                    _webhookUrl,
                    new StringContent(body, Encoding.UTF8, "application/json"),
                    ct);

                response.EnsureSuccessStatusCode();

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                _logger.LogInformation(
                    "Outbox: {EventType} [{Id}] sent to n8n successfully",
                    message.EventType, message.Id);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                _logger.LogWarning(
                    "Outbox: {EventType} [{Id}] failed (attempt {Retry}/{Max}): {Error}",
                    message.EventType, message.Id, message.RetryCount, MaxRetries, ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
