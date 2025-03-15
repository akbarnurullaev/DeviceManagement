namespace DeviceManager.SmartWatch;

public class EmptyBatteryException() : Exception("Device can't be used when it's battery is 0%");
