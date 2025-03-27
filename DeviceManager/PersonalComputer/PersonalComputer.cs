namespace DeviceManager.PersonalComputer;

/// <summary>
/// Represents a personal computer device.
/// </summary>
public class PersonalComputer : Device
{
    /// <summary>
    /// Gets or sets the operating system.
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalComputer"/> class.
    /// </summary>
    /// <param name="id">The device identifier.</param>
    /// <param name="name">The device name.</param>
    /// <param name="operatingSystem">The operating system.</param>
    public PersonalComputer(string id, string name, string? operatingSystem)
        : base(id, name)
    {
        OperatingSystem = operatingSystem;
    }

    /// <summary>
    /// Turns on the personal computer.
    /// </summary>
    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
        {
            throw new EmptySystemException();
        }

        base.TurnOn();
    }

    /// <summary>
    /// Converts the personal computer to its CSV representation.
    /// </summary>
    /// <returns>A CSV string representing the personal computer.</returns>
    public override string ToCsv()
    {
        return $"{Id},{Name}{(string.IsNullOrWhiteSpace(OperatingSystem) ? "" : $",{OperatingSystem}")}";
    }

    /// <summary>
    /// Returns a string that represents the personal computer.
    /// </summary>
    /// <returns>A string representation of the personal computer.</returns>
    public override string ToString() =>
        $"PC - ID: {Id}, Name: {Name}, OS: {OperatingSystem}, On: {IsTurnedOn}";
}