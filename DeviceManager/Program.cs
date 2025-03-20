using DeviceManager.SmartWatch;

string filePath = "db.csv"; 

DeviceManager.DeviceManager manager;
try
{
    manager = new DeviceManager.DeviceManager(filePath);
    Console.WriteLine("Device Manager initialized successfully.");
}
catch (FileNotFoundException e)
{
    Console.WriteLine($"Error: {e.Message}");
    return;
}

Console.WriteLine("\nLoaded Devices:");
manager.ShowAllDevices();

var newSmartwatch = new Smartwatch("SW-2002", "Fitness Tracker", 85);
manager.AddDevice(newSmartwatch);
Console.WriteLine("\nAdded a new device: " + newSmartwatch);
manager.ShowAllDevices();

string deviceId = "SW-2002";

manager.EditDevice(deviceId, null, "PJATK Ultra 2500");
manager.ShowAllDevices();

manager.RemoveDevice(deviceId);

Console.WriteLine("\nUpdated Device List:");
manager.ShowAllDevices();

