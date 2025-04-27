namespace DeviceManager.EmbeddedDevice;

/// <summary>
/// Thrown when an incorrect IP provided for device's connection attempt
/// </summary>   
public class ConnectionException() : Exception("Connection can not be established due to malformed fields.");