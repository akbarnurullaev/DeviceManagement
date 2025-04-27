namespace DeviceManager
{
    /// <summary>
    /// Manages a collection of devices and provides operations on them.
    /// </summary>
    public class DeviceManager
    {
        private const int MaxCount = 15;
        private readonly IDeviceRepository _repository;

        /// <summary>
        /// Gets the list of devices.
        /// </summary>
        public readonly List<Device> Devices;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceManager"/> class.
        /// </summary>
        /// <param name="repository">The device repository used for persistence.</param>
        public DeviceManager(IDeviceRepository repository)
        {
            _repository = repository;
            Devices = _repository.LoadDevices().Take(MaxCount).ToList();
        }

        /// <summary>
        /// Adds a new device.
        /// </summary>
        /// <param name="device">The device to add.</param>
        public void AddDevice(Device device)
        {
            if (Devices.Count >= MaxCount)
            {
                Console.WriteLine("Device list is full. Cannot add more devices.");
                return;
            }

            Devices.Add(device);
            _repository.SaveDevices(Devices);
        }

        /// <summary>
        /// Removes a device by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the device to remove.</param>
        public void RemoveDevice(string id)
        {
            var device = Devices.FirstOrDefault(d => d.Id == id);
            if (device != null)
            {
                Devices.Remove(device);
                _repository.SaveDevices(Devices);
            }
            else
            {
                Console.WriteLine($"Device with ID {id} not found.");
            }
        }

        /// <summary>
        /// Edits an existing device's identifier and/or name.
        /// </summary>
        /// <param name="id">The current device identifier.</param>
        /// <param name="newId">The new device identifier.</param>
        /// <param name="newName">The new device name.</param>
        public void EditDevice(string id, string? newId, string? newName)
        {
            var device = GetDeviceById(id);
            if (device != null)
            {
                device.Name = newName ?? device.Name;
                device.Id = newId ?? device.Id;
                _repository.SaveDevices(Devices);
            }
            else
            {
                Console.WriteLine($"Device with ID {id} not found.");
            }
        }

        /// <summary>
        /// Turns on a device by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the device to turn on.</param>
        public void TurnOnDevice(string id)
        {
            GetDeviceById(id)?.TurnOn();
        }

        /// <summary>
        /// Turns off a device by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the device to turn off.</param>
        public void TurnOffDevice(string id)
        {
            GetDeviceById(id)?.TurnOff();
        }

        /// <summary>
        /// Displays all devices.
        /// </summary>
        public void ShowAllDevices()
        {
            foreach (var device in Devices)
            {
                Console.WriteLine(device);
            }
        }

        /// <summary>
        /// Retrieves a device by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the device.</param>
        /// <returns>The device if found; otherwise, null.</returns>
        public Device? GetDeviceById(string id)
        {
            return Devices.FirstOrDefault(d => d.Id == id);
        }
    }
}