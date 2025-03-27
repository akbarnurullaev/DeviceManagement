namespace DeviceManager.SmartWatch;

/// <summary>
/// Thrown when battery percentage is negative or 0%
/// </summary>   
public class EmptyBatteryException() : Exception("Device can't be used when it's battery is 0%");
