using System.Text.RegularExpressions;

namespace DeviceManager.EmbeddedDevice;

public class EmbeddedDevice : Device
{
    private string _ipAddress;
    public string NetworkName { get; set; }

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

    public EmbeddedDevice(string id, string name, string ipAddress, string networkName) : base(id, name)
    {
        _ipAddress = ipAddress;
        IpAddress = ipAddress;
        NetworkName = networkName;
    }

    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException();
    }

    public override void TurnOn()
    {
        Connect();
        IsTurnedOn = true;
    }

    public override string ToCsv()
    {
        return $"{Id},{Name},{IpAddress},{NetworkName}";
    }
    
    public override string ToString() => $"Embedded - ID: {Id}, Name: {Name}, IP: {IpAddress}, Network: {NetworkName}, On: {IsTurnedOn}";
}