namespace DeviceManager;

public abstract class Device(string id, string name)
{
    public string Id = id;
    public string Name = name;
    public bool   IsTurnedOn = false;

    public virtual void TurnOn()
    {
        IsTurnedOn = true;
    }

    public virtual void TurnOff()
    {
        IsTurnedOn = false;
    }

    public abstract string ToCsv();
}