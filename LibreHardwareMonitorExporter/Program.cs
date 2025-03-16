using LibreHardwareMonitorExporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

var builder = Host.CreateDefaultBuilder(args);
builder.UseContentRoot(Directory.GetCurrentDirectory());

builder.ConfigureServices((hostBuilderContext, services) => {
    services.AddOptions<CollectedMetricsOptions>().BindConfiguration("CollectedMetrics");
    services.AddOptions<MetricsServerOptions>().BindConfiguration("MetricsServer");

    services
        .AddHostedService<PrometheusHostedService>()
        .AddSingleton<MetricsCollector>(
            provider => new MetricsCollector(
                Metrics.DefaultRegistry,
                provider.GetRequiredService<IOptions<CollectedMetricsOptions>>(),
                provider.GetRequiredService<ILogger<MetricsCollector>>()
            )
        );
});

var host = builder.Build();
await host.RunAsync();
