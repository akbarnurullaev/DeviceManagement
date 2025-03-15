using DeviceManager.SmartWatch;
using NUnit.Framework;

namespace DeviceManager.Tests
{
    [TestFixture]
    public class DeviceManagerTests
    {
        private const string TestFilePath = "test_devices.csv";

        private void CreateTestFile(IEnumerable<string> lines)
        {
            File.WriteAllLines(TestFilePath, lines);
        }

        [Test]
        public void DeviceManager_ShouldThrowFileNotFound_WhenFileDoesNotExist()
        {
            Assert.Throws<FileNotFoundException>(() => new DeviceManager("non_existent_file.csv"));
        }

        [Test]
        public void DeviceManager_ShouldLoadDevicesFromFile()
        {
            CreateTestFile(new[] 
            {
                "SW-1,SmartWatch1,80%",
                "P-2,Laptop1,Windows",
                "ED-3,Embedded1,192.168.1.1,HomeNetwork"
            });

            var manager = new DeviceManager(TestFilePath);

            Assert.That(manager.Devices.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddDevice_ShouldIncreaseDeviceCount()
        {
            CreateTestFile(new string[0]); // Empty file
            var manager = new DeviceManager(TestFilePath);

            var newDevice = new Smartwatch("SW-4", "NewWatch", 50);
            manager.AddDevice(newDevice);

            Assert.That(manager.Devices.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveDevice_ShouldDecreaseDeviceCount()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager(TestFilePath);

            manager.RemoveDevice("SW-1");
            
            Assert.That(manager.Devices.Count, Is.EqualTo(0));
        }

        [Test]
        public void EditDevice_ShouldUpdateProperties()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager(TestFilePath);

            manager.EditDevice("SW-1", newId: "SW-5", newName: "UpdatedWatch");

            var devices = manager.Devices;
            Assert.That(devices[0].Id, Is.EqualTo("SW-5"));
            Assert.That(devices[0].Name, Is.EqualTo("UpdatedWatch"));
        }

        [Test]
        public void TurnOnDevice_ShouldNotThrow_WhenDeviceExists()
        {
            CreateTestFile(new[] { "SW-1,SmartWatch1,80%" });
            var manager = new DeviceManager(TestFilePath);

            Assert.DoesNotThrow(() => manager.TurnOnDevice("SW-1"));
        }

        [Test]
        public void TurnOnDevice_ShouldLogMessage_WhenDeviceDoesNotExist()
        {
            CreateTestFile(new string[0]);
            var manager = new DeviceManager(TestFilePath);

            Assert.DoesNotThrow(() => manager.TurnOnDevice("SW-99"));
        }

        [Test]
        public void ShouldNotExceedMaxDeviceLimit()
        {
            var deviceEntries = Enumerable.Range(1, 15).Select(i => $"SW-{i},Device{i},75%");
            CreateTestFile(deviceEntries);
            var manager = new DeviceManager(TestFilePath);

            var newDevice = new Smartwatch("SW-16", "OverflowWatch", 50);
            manager.AddDevice(newDevice);

            Assert.That(manager.Devices.Count, Is.EqualTo(15));
        }
    }
}