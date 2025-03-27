namespace DeviceManager.PersonalComputer;

/// <summary>
/// Thrown when OS is not provided
/// </summary>   
public class EmptySystemException() : Exception("Device can't be used with no OS");