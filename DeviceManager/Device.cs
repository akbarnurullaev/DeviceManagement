namespace DeviceManager;

public abstract class Device
{
    public int    Id;
    public string Name;
    public bool   IsTurnedOn;

    public virtual void TurnOn()
    {
        IsTurnedOn = true;
    }

    public virtual void TurnOff()
    {
        IsTurnedOn = false;
    }
}