using DeviceManager.SmartWatch;

namespace DeviceManager
{
    public class DeviceManager
    {
        private const int MaxCount = 15;
        private readonly string _filePath;
        private readonly List<Device> _devices = new();

        public DeviceManager(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");
            
            _filePath = filePath;

            foreach (var line in File.ReadLines(filePath))
            {
                var tokens = line.Split(',');

                if (tokens.Length < 4)
                    throw new FormatException($"Invalid line format: {line}");

                if (!int.TryParse(tokens[1], out int id))
                    throw new FormatException($"Invalid ID in line: {line}");
                
                var type = tokens[0];
                var name = tokens[2];
                Device? device = type switch
                {
                    "SW" when tokens.Length == 4 && int.TryParse(tokens[3], out int battery) => new Smartwatch(id, name, battery),
                    "P" when tokens.Length == 4 => new PersonalComputer.PersonalComputer(id, name, tokens[3]),
                    "ED" when tokens.Length == 5 => new EmbeddedDevice.EmbeddedDevice(id, name, tokens[3], tokens[4]),
                    _ => throw new FormatException($"Invalid device type or parameters: {line}")
                };

                AddDevice(device);
            };
        }

        private void AddDevice(Device device)
        {
            if (_devices.Count >= MaxCount)
            {
                Console.WriteLine("Device list is full. Cannot add more devices.");
                return;
            }

            File.AppendAllText(_filePath, device.ToCSV() + Environment.NewLine);
            _devices.Add(device);
        }

        public void RemoveDevice(int id) => _devices.RemoveAll(d => d.Id == id);

        public void TurnOnDevice(int id)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
                Console.WriteLine($"Device with ID {id} not found.");
            else
                device.TurnOn();
        }

        public void TurnOffDevice(int id)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);
            if (device is null)
                Console.WriteLine($"Device with ID {id} not found.");
            else
                device.TurnOff();
        }

        public void ShowAllDevices() => _devices.ForEach(Console.WriteLine);
    }
}