namespace DeviceManager;

/// <summary>
/// Represents a generic device.
/// </summary>
public abstract class Device
{
    /// <summary>
    /// Gets or sets the device identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the device name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the device is turned on.
    /// </summary>
    public bool IsTurnedOn { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Device"/> class.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="name">The device name.</param>
    protected Device(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Turns on the device.
    /// </summary>
    public virtual void TurnOn()
    {
        IsTurnedOn = true;
    }

    /// <summary>
    /// Turns off the device.
    /// </summary>
    public virtual void TurnOff()
    {
        IsTurnedOn = false;
    }

    /// <summary>
    /// Converts the device to its CSV representation.
    /// </summary>
    /// <returns>A CSV string representing the device.</returns>
    public abstract string ToCsv();
}
