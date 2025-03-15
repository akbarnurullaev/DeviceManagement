namespace DeviceManager;

public abstract class Device(int id, string name)
{
    public int    Id = id;
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
}