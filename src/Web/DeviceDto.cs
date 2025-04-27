namespace Web;

public class DeviceDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? OperatingSystem { get; set; }
    public string? IpAddress { get; set; }
    public string? NetworkName { get; set; }
    public int? BatteryPercentage { get; set; }
}