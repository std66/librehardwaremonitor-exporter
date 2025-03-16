using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace LibreHardwareMonitorExporter;
internal class PrometheusHostedService(IHostApplicationLifetime hostApplication, ILogger<PrometheusHostedService> logger, MetricsCollector collector, IOptions<MetricsServerOptions> options) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        const int defaultPort = 21543;
        const string defaultPath = "metrics/";

        int port = options.Value.Port;

        if (port == default) {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("MetricsServer.Port is not configured in appsettings.json, using default port {defaultPort}.", defaultPort);

            port = defaultPort;
        }

        string? path = options.Value.Path;
        if (path == default) {
            if (logger.IsEnabled(LogLevel.Warning))
                logger.LogWarning("MetricsServer.Path is not configured in appsettings.json, using default path {defaultPath}.", defaultPath);

            path = defaultPath;
        }

        using var server = new MetricServer(port, path);
        try {
            server.Start();
        }
        catch (Exception ex) {
            if (logger.IsEnabled(LogLevel.Critical))
                logger.LogCritical("Cannot start server due to the following error: {exception}", ex);

            hostApplication.StopApplication();
        }

        Metrics.SuppressDefaultMetrics();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Endpoint '{path}' ready on port {port}.", path, port);

        collector.Open();
        await RunLoop(stoppingToken);
        collector.Close();
        await server.StopAsync();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Stopped collecting metrics.");
    }

    private async Task RunLoop(CancellationToken stoppingToken) {
        do {
            try {
                collector.UpdateMetrics();
            }
            catch (Exception e) {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("Exception occurred while updating metrics: {exception}", e.ToString());
            }

            if (logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Metrics updated.");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}
