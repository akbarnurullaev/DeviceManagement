namespace DeviceManager;

/// <summary>
/// Defines methods for loading and saving devices.
/// </summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Loads devices from the repository.
    /// </summary>
    /// <returns>An enumerable collection of devices.</returns>
    IEnumerable<Device> LoadDevices();

    /// <summary>
    /// Saves the specified devices to the repository.
    /// </summary>
    /// <param name="devices">The devices to save.</param>
    void SaveDevices(IEnumerable<Device> devices);
}