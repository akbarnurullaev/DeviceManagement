using System.Text.RegularExpressions;

namespace DeviceManager.EmbeddedDevice;

/// <summary>
/// Represents an embedded device.
/// </summary>
public class EmbeddedDevice : Device
{
    private string _ipAddress;

    /// <summary>
    /// Gets or sets the IP address.
    /// </summary>
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (!Regex.IsMatch(value, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
                throw new ArgumentException();
            _ipAddress = value;
        }
    }

    /// <summary>
    /// Gets or sets the network name.
    /// </summary>
    public string NetworkName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedDevice"/> class.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="name">The device name.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="networkName">The network name.</param>
    public EmbeddedDevice(string id, string name, string ipAddress, string networkName)
        : base(id, name)
    {
        _ipAddress = ipAddress;
        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    /// <summary>
    /// Connects the device to its network.
    /// </summary>
    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException();
    }

    /// <summary>
    /// Turns on the embedded device.
    /// </summary>
    public override void TurnOn()
    {
        Connect();
        IsTurnedOn = true;
    }

    /// <summary>
    /// Converts the embedded device to its CSV representation.
    /// </summary>
    /// <returns>A CSV string representing the embedded device.</returns>
    public override string ToCsv()
    {
        return $"{Id},{Name},{IpAddress},{NetworkName}";
    }

    /// <summary>
    /// Returns a string that represents the embedded device.
    /// </summary>
    /// <returns>A string representation of the embedded device.</returns>
    public override string ToString() =>
        $"Embedded - ID: {Id}, Name: {Name}, IP: {IpAddress}, Network: {NetworkName}, On: {IsTurnedOn}";
}