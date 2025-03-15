namespace DeviceManager.SmartWatch;

public class Smartwatch : Device, IPowerNotifier
{
    private int _batteryPercentage;

    public int BatteryPercentage
    {
        get => _batteryPercentage;
        set
        {
            if (value is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException("BatteryPercentage");
            }

            _batteryPercentage = value;

            if (_batteryPercentage < 20) NotifyAboutLowBattery();
        }
    }
    
    public Smartwatch(int id, string name, int batteryPercentage) : base(id, name)
    {
        BatteryPercentage = batteryPercentage;
    }

    public override void TurnOn()
    {
        if (BatteryPercentage == 0)
        {
            throw new EmptyBatteryException();
        }
        
        BatteryPercentage = Math.Max(0, BatteryPercentage - 10);
        
        base.TurnOn();
    }
    
    public void NotifyAboutLowBattery()
    {
        Console.WriteLine("Battery percentage is less than 20%");
    }
    
    public override string ToCSV()
    {
        return $"SW,{Id},{Name},{BatteryPercentage}";
    }
    
    public override string ToString() => $"Smartwatch - ID: {Id}, Name: {Name}, Battery: {BatteryPercentage}%, On: {IsTurnedOn}";
}