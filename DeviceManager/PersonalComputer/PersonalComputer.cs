namespace DeviceManager.PersonalComputer;

public class PersonalComputer(string id, string name, string? operatingSystem) : Device(id, name)
{
    public string? OperatingSystem { get; set; } = operatingSystem;

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
        {
            throw new EmptySystemException();
        }
        
        base.TurnOn();
    }
    
    public override string ToCsv()
    {
        return $"{Id},{Name}{(string.IsNullOrWhiteSpace(OperatingSystem) ? "" : $",{OperatingSystem}")}";
    }
    
    public override string ToString() => $"PC - ID: {Id}, Name: {Name}, OS: {OperatingSystem}, On: {IsTurnedOn}";
}