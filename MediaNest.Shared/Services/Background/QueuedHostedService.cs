﻿using DnsClient.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediaNest.Shared.Services.Background;

public class QueuedHostedService : BackgroundService {
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger) {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Background task launch.");
        while (!stoppingToken.IsCancellationRequested) {
            var workItem = await _taskQueue.DequeueTask(stoppingToken);
            try {
                await workItem(stoppingToken);
            }
            catch (Exception ex) {
                _logger.LogError("Background Task Error :" + ex.Message);
            }
        }
        _logger.LogInformation("Background task done.");
    }
}
