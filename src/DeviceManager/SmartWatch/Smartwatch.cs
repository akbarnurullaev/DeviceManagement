namespace DeviceManager.SmartWatch;

/// <summary>
/// Represents a smartwatch device.
/// </summary>
public class Smartwatch : Device, IPowerNotifier
{
    private int _batteryPercentage;

    /// <summary>
    /// Gets or sets the battery percentage.
    /// </summary>
    public int BatteryPercentage
    {
        get => _batteryPercentage;
        set
        {
            if (value is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(BatteryPercentage));
            }

            _batteryPercentage = value;

            if (_batteryPercentage < 20)
                NotifyAboutLowBattery();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Smartwatch"/> class.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="name">The device name.</param>
    /// <param name="batteryPercentage">The initial battery percentage.</param>
    public Smartwatch(string id, string name, int batteryPercentage)
        : base(id, name)
    {
        BatteryPercentage = batteryPercentage;
    }

    /// <summary>
    /// Turns on the smartwatch.
    /// </summary>
    public override void TurnOn()
    {
        if (BatteryPercentage < 11)
        {
            throw new EmptyBatteryException();
        }

        BatteryPercentage = Math.Max(0, BatteryPercentage - 10);
        base.TurnOn();
    }

    /// <summary>
    /// Notifies that the battery is low.
    /// </summary>
    public void NotifyAboutLowBattery()
    {
        Console.WriteLine("Battery percentage is less than 20%");
    }

    /// <summary>
    /// Converts the smartwatch to its CSV representation.
    /// </summary>
    /// <returns>A CSV string representing the smartwatch.</returns>
    public override string ToCsv()
    {
        return $"{Id},{Name},{BatteryPercentage}%";
    }

    /// <summary>
    /// Returns a string that represents the smartwatch.
    /// </summary>
    /// <returns>A string representation of the smartwatch.</returns>
    public override string ToString() =>
        $"Smartwatch - ID: {Id}, Name: {Name}, Battery: {BatteryPercentage}%, On: {IsTurnedOn}";
}