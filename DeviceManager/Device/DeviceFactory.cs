using DeviceManager.SmartWatch;

namespace DeviceManager;

/// <summary>
/// Provides methods for creating <see cref="Device"/> instances from CSV strings.
/// </summary>
public static class DeviceFactory
{
    /// <summary>
    /// Creates a device instance from a CSV line.
    /// </summary>
    /// <param name="csvLine">The CSV line representing a device.</param>
    /// <returns>A <see cref="Device"/> instance if the CSV line is valid; otherwise, null.</returns>
    public static Device? CreateDevice(string csvLine)
    {
        var tokens = csvLine.Split(',');
        if (tokens.Length is < 2 or > 5)
        {
            Console.WriteLine($"Skipping invalid line: {csvLine}");
            return null;
        }

        var id = tokens[0];
        var type = id.Split('-')[0];

        if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(type))
        {
            Console.WriteLine($"Skipping invalid ID in line: {csvLine}");
            return null;
        }

        var name = tokens[1];
        Device? device = type switch
        {
            "SW" when tokens.Length == 3 && int.TryParse(tokens[2].TrimEnd('%'), out int battery)
                => new Smartwatch(id, name, battery),
            "P" when tokens.Length == 3
                => new PersonalComputer.PersonalComputer(id, name, tokens[2]),
            "P" when tokens.Length == 2
                => new PersonalComputer.PersonalComputer(id, name, null),
            "ED" when tokens.Length == 4
                => new EmbeddedDevice.EmbeddedDevice(id, name, tokens[2], tokens[3]),
            _ => null
        };

        return device;
    }
}