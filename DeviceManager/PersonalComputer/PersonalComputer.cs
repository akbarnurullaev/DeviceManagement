namespace DeviceManager.PersonalComputer;

public class PersonalComputer(int id, string name, string operatingSystem) : Device(id, name)
{
    public string OperatingSystem { get; set; } = operatingSystem;

    public override void TurnOn()
    {
        if (string.IsNullOrWhiteSpace(OperatingSystem))
        {
            throw new EmptySystemException();
        }
        
        base.TurnOn();
    }
    
    public override string ToCSV()
    {
        return $"P,{Id},{Name},{OperatingSystem}";
    }
    
    public override string ToString() => $"PC - ID: {Id}, Name: {Name}, OS: {OperatingSystem}, On: {IsTurnedOn}";
}