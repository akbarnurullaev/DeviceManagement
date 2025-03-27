namespace DeviceManager;

/// <summary>
/// Factory for creating <see cref="DeviceManager"/> instances.
/// </summary>
public static class DeviceManagerFactory
{
    /// <summary>
    /// Creates a <see cref="DeviceManager"/> instance using a file path.
    /// </summary>
    /// <param name="filePath">The file path for device data.</param>
    /// <returns>A new instance of <see cref="DeviceManager"/>.</returns>
    public static DeviceManager Create(string filePath)
    {
        var repository = new FileDeviceRepository(filePath);
        return new DeviceManager(repository);
    }
}