using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Text.Json;

namespace LibreHardwareMonitorExporter {
    internal class MetricsCollector : IDisposable {
        private MetricsVisitor _visitor;
        private Computer _computer;

        public MetricsCollector(CollectorRegistry registry, IOptions<CollectedMetricsOptions> options, ILogger<MetricsCollector> logger) {
            _visitor = new MetricsVisitor(registry, options.Value.MetricsPrefix ?? "lhm");

            _computer = new Computer() {
                IsBatteryEnabled = options.Value.IsBatteryEnabled,
                IsControllerEnabled = options.Value.IsControllerEnabled,
                IsCpuEnabled = options.Value.IsCpuEnabled,
                IsGpuEnabled = options.Value.IsGpuEnabled,
                IsMemoryEnabled = options.Value.IsMemoryEnabled,
                IsMotherboardEnabled = options.Value.IsMotherboardEnabled,
                IsNetworkEnabled = options.Value.IsNetworkEnabled,
                IsPsuEnabled = options.Value.IsPsuEnabled,
                IsStorageEnabled = options.Value.IsStorageEnabled,
            };

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Active configuration for metric collector: {activeConfiguration}", JsonSerializer.Serialize(options.Value));
        }

        public void Open() {
            _computer.Open();
        }

        public void Close() {
            _computer.Close();
        }

        public void Dispose() {
            Close();
        }

        public void UpdateMetrics() {
            _computer.Traverse(_visitor);
        }
    }
}
