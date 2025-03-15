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
}