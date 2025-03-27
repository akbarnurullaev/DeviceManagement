namespace DeviceManager;

/// <summary>
/// A file-based repository that loads and saves devices in CSV format.
/// </summary>
public class FileDeviceRepository : IDeviceRepository
{
    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDeviceRepository"/> class.
    /// </summary>
    /// <param name="filePath">The path to the CSV file containing device data.</param>
    public FileDeviceRepository(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        _filePath = filePath;
    }

    /// <inheritdoc/>
    public IEnumerable<Device> LoadDevices()
    {
        var devices = new List<Device>();

        foreach (var line in File.ReadLines(_filePath))
        {
            try
            {
                var device = DeviceFactory.CreateDevice(line);
                if (device != null)
                {
                    devices.Add(device);
                }
                else
                {
                    Console.WriteLine($"Skipping invalid device line: {line}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing line: {line}, Error: {e.Message}");
            }
        }

        return devices;
    }

    /// <inheritdoc/>
    public void SaveDevices(IEnumerable<Device> devices)
    {
        var lines = devices.Select(d => d.ToCsv());
        File.WriteAllLines(_filePath, lines);
    }
}