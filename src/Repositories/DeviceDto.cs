namespace Web;

/// <summary>
/// Data Transfer Object representing a device (short or detailed view).
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// Unique identifier of the device.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Indicates whether the device is enabled (only used in list view).
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Type of the device: "device", "pc", "embedded", or "smartwatch".
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Operating system (for PC devices).
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// IP address (for Embedded devices).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Network name (for Embedded devices).
    /// </summary>
    public string? NetworkName { get; set; }

    /// <summary>
    /// Current battery percentage (for Smartwatch devices).
    /// </summary>
    public int? BatteryPercentage { get; set; }

    /// <summary>
    /// Concurrency token (SQL rowversion) for optimistic locking.
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;
}