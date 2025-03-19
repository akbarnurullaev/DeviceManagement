using DeviceManager.SmartWatch;

namespace Tests
{
    public class DeviceManagerTests : IDisposable
    {
        private const string TestFilePath = "test_devices.csv";

        public DeviceManagerTests()
        {
            if (File.Exists(TestFilePath))
                File.Delete(TestFilePath);
        }

        private void CreateTestFile(IEnumerable<string> lines)
        {
            File.WriteAllLines(TestFilePath, lines);
        }

        [Fact]
        public void DeviceManager_ShouldThrowFileNotFound_WhenFileDoesNotExist()
        {
            Assert.Throws<FileNotFoundException>(() => new DeviceManager.DeviceManager("non_existent_file.csv"));
        }

        [Fact]
        public void DeviceManager_ShouldLoadDevicesFromFile()
        {
            CreateTestFile(new[]
            {
                "SW-1,SmartWatch1,80%",
                "P-2,Laptop1,Windows",
                "ED-3,Embedded1,192.168.1.1,HomeNetwork"
            });

            var manager = new DeviceManager.DeviceManager(TestFilePath);

            Assert.Equal(3, manager.Devices.Count);
        }

        [Fact]
        public void AddDevice_ShouldIncreaseDeviceCount()
        {
            CreateTestFile(Array.Empty<string>());
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            var newDevice = new Smartwatch("SW-4", "NewWatch", 50);
            manager.AddDevice(newDevice);

            Assert.Single(manager.Devices);
        }

        [Fact]
        public void RemoveDevice_ShouldDecreaseDeviceCount()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            manager.RemoveDevice("SW-1");

            Assert.Empty(manager.Devices);
        }

        [Fact]
        public void EditDevice_ShouldUpdateProperties()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            manager.EditDevice("SW-1", newId: "SW-5", newName: "UpdatedWatch");

            var device = manager.Devices.First();
            Assert.Equal("SW-5", device.Id);
            Assert.Equal("UpdatedWatch", device.Name);
        }

        [Fact]
        public void TurnOnDevice_ShouldNotThrow_WhenDeviceExists()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            var exception = Record.Exception(() => manager.TurnOnDevice("SW-1"));
            Assert.Null(exception);
        }

        [Fact]
        public void TurnOnDevice_ShouldNotThrow_WhenDeviceDoesNotExist()
        {
            CreateTestFile(Array.Empty<string>());
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            var exception = Record.Exception(() => manager.TurnOnDevice("SW-99"));
            Assert.Null(exception);
        }

        [Fact]
        public void ShouldNotExceedMaxDeviceLimit()
        {
            var deviceEntries = Enumerable.Range(1, 15).Select(i => $"SW-{i},Device{i},75%");
            CreateTestFile(deviceEntries);
            var manager = new DeviceManager.DeviceManager(TestFilePath);

            var newDevice = new Smartwatch("SW-16", "OverflowWatch", 50);
            manager.AddDevice(newDevice);

            Assert.Equal(15, manager.Devices.Count);
        }

        public void Dispose()
        {
            if (File.Exists(TestFilePath))
                File.Delete(TestFilePath);
        }
    }
}