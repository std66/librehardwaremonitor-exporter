using LibreHardwareMonitor.Hardware;
using Prometheus;

namespace LibreHardwareMonitorExporter {
    internal class MetricsVisitor : IVisitor {
        private CollectorRegistry _registry;
        private readonly string prefix;

        public MetricsVisitor(CollectorRegistry registry, string prefix) {
            _registry = registry;
            this.prefix = prefix;
        }

        public void VisitComputer(IComputer computer) {

        }

        public void VisitHardware(IHardware hardware) {
            hardware.Update();
            hardware.Traverse(this);
        }

        public void VisitParameter(IParameter parameter) {
        }

        public void VisitSensor(ISensor sensor) {
            var metricName = GetMetricName(sensor);
            var hw = sensor.Hardware.Identifier.ToString();

            string help;
            switch (sensor.SensorType) {
                case SensorType.Clock:
                    help = "Clock [MHz]";
                    break;
                case SensorType.Load:
                    help = "Load [%]";
                    break;
                case SensorType.Temperature:
                    help = "Temperature [C]";
                    break;
                case SensorType.Power:
                    help = "Power consumption [W]";
                    break;
                default:
                    help = sensor.SensorType.ToString();
                    break;
            }

            if (sensor.Value.HasValue) {
                var gauge = Metrics.WithCustomRegistry(_registry).CreateGauge(metricName, help, "hw", "name");
                gauge.Labels(hw, sensor.Name).Set(sensor.Value.Value);
            }
        }

        private string GetMetricName(ISensor sensor) {
            return $"{prefix}_{sensor.Hardware.HardwareType.ToString().ToLower()}_{sensor.SensorType.ToString().ToLowerInvariant()}";
        }
    }
}
