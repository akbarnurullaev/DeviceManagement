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
                    if (tokens.Length < 2 || tokens.Length > 5)
                    {
                        Console.WriteLine($"Skipping invalid line: {line}");
                        continue;
                    }

                    var id = tokens[0]; // ID as a string
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

            File.AppendAllText(_filePath, device.ToCsv() + Environment.NewLine);
            Devices.Add(device);
        }

        public void RemoveDevice(string id) => Devices.RemoveAll(d => d.Id == id);
        
        public void EditDevice(string id, string? newId, string? newName)
        {
            var device = Devices.FirstOrDefault(d => d.Id == id);
            
            if (device != null)
            {
                device.Name = newName ?? device.Name;
                device.Id = newId ?? device.Id;
            };
        }

        public void TurnOnDevice(string id)
        {
            var device = Devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
                Console.WriteLine($"Device with ID {id} not found.");
            else
                device.TurnOn();
        }

        public void TurnOffDevice(string id)
        {
            var device = Devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
                Console.WriteLine($"Device with ID {id} not found.");
            else
                device.TurnOff();
        }

        public void ShowAllDevices() => Devices.ForEach(Console.WriteLine);
    }
}