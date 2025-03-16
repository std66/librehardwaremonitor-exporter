namespace LibreHardwareMonitorExporter;
public class CollectedMetricsOptions {
    public bool IsBatteryEnabled { get; set; }
    public bool IsControllerEnabled { get; set; }
    public bool IsCpuEnabled { get; set; }
    public bool IsGpuEnabled { get; set; }
    public bool IsMemoryEnabled { get; set; }
    public bool IsMotherboardEnabled { get; set; }
    public bool IsNetworkEnabled { get; set; }
    public bool IsPsuEnabled { get; set; }
    public bool IsStorageEnabled { get; set; }
    public string? MetricsPrefix { get; set; }
}
