using DeviceManager.SmartWatch;

namespace DeviceManager
{
    public class DeviceManager
    {
        private const int MaxCount = 15;
        private readonly string _filePath;
        public readonly List<Device> Devices = new();

        public DeviceManager(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
    
            _filePath = filePath;

            foreach (var line in File.ReadLines(filePath))
            {
                try
                {
                    var tokens = line.Split(',');
                    if (tokens.Length is < 2 or > 5)
                    {
                        Console.WriteLine($"Skipping invalid line: {line}");
                        continue;
                    }

                    var id = tokens[0];
                    var type = id.Split('-')[0];

                    if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(type))
                    {
                        Console.WriteLine($"Skipping invalid ID in line: {line}");
                        continue;
                    }

                    var name = tokens[1];
                    Device? device = type switch
                    {
                        "SW" when tokens.Length == 3 && int.TryParse(tokens[2].TrimEnd('%'), out int battery) 
                            => new Smartwatch(id, name, battery),

                        "P" when tokens.Length == 3 
                            => new PersonalComputer.PersonalComputer(id, name, operatingSystem: tokens[2]),

                        "P" when tokens.Length == 2 
                            => new PersonalComputer.PersonalComputer(id, name, null),

                        "ED" when tokens.Length == 4 
                            => new EmbeddedDevice.EmbeddedDevice(id, name, ipAddress: tokens[2], networkName: tokens[3]),

                        _ => null
                    };

                    if (device != null && Devices.Count < MaxCount)
                        Devices.Add(device);
                    else
                        Console.WriteLine($"Skipping invalid or duplicate device: {line}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error processing line: {line}, Error: {e.Message}");
                }
            }
        }

        public void AddDevice(Device device)
        {
            if (Devices.Count >= MaxCount)
            {
                Console.WriteLine("Device list is full. Cannot add more devices.");
                return;
            }

            Devices.Add(device);
            SaveDevicesToFile();
        }

        public void RemoveDevice(string id)
        {
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                if (Devices[i].Id == id)
                {
                    Devices.RemoveAt(i);
                    SaveDevicesToFile();
                    return;
                }
            }
            
            Console.WriteLine($"Device with ID {id} not found.");
        }
        
        public void EditDevice(string id, string? newId, string? newName)
        {
            object boxedDevice = GetDeviceById(id);

            if (boxedDevice != null)
            {
                Device unboxedDevice = (Device) boxedDevice;

                unboxedDevice.Name = newName ?? unboxedDevice.Name;
                unboxedDevice.Id = newId ?? unboxedDevice.Id;

                SaveDevicesToFile();
            }
            else
            {
                Console.WriteLine($"Device with ID {id} not found.");
            }
        }

        public void TurnOnDevice(string id)
        {
            GetDeviceById(id)?.TurnOn();
        }

        public void TurnOffDevice(string id)
        {
            GetDeviceById(id)?.TurnOff();
        }

        public void ShowAllDevices()
        {
            foreach (var device in Devices)
            {
                Console.WriteLine(device);
            }
        }
        
        public Device? GetDeviceById(string id)
        {
            foreach (var device in Devices)
            {
                if (device.Id == id)
                {
                    return device;
                }
            }
            return null;
        }

        private void SaveDevicesToFile()
        {
            var lines = new List<string>();

            foreach (var device in Devices)
            {
                lines.Add(device.ToCsv());
            }

            File.WriteAllLines(_filePath, lines);
        }
    }
}