/// <summary>
/// Defines the logic for handling the battery life
/// </summary>   
public interface IPowerNotifier
{
    /// <summary>
    /// Notifies about low battery percentage
    /// </summary>   
    public void NotifyAboutLowBattery();
}