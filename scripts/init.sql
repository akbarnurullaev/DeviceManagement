IF DB_ID('DeviceDb') IS NOT NULL
    BEGIN
        ALTER DATABASE DeviceDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE DeviceDb;
    END;
GO

CREATE DATABASE DeviceDb;
GO

USE DeviceDb;
GO

-- Models
CREATE TABLE Device (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(250) NOT NULL,
    IsEnabled BIT NOT NULL
);
GO

CREATE TABLE Embedded (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IpAddress VARCHAR(250) NOT NULL,
    NetworkName VARCHAR(250) NOT NULL,
    DeviceId VARCHAR(50) NOT NULL UNIQUE,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO

CREATE TABLE PersonalComputer (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OperationSystem VARCHAR(250),
    DeviceId VARCHAR(50) NOT NULL UNIQUE,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO

CREATE TABLE Smartwatch (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BatteryPercentage INT NOT NULL,
    DeviceId VARCHAR(50) NOT NULL UNIQUE,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);
GO

-- Seeding
INSERT INTO Device (Id, Name, IsEnabled) VALUES
    ('SW-01', 'FitPro Watch', 1),
    ('PC-01', 'My Laptop', 1),
    ('EM-01', 'IoT Sensor', 1);
GO

INSERT INTO Smartwatch (BatteryPercentage, DeviceId) VALUES
    (87, 'SW-01');
GO

INSERT INTO PersonalComputer (OperationSystem, DeviceId) VALUES
    ('Windows 11', 'PC-01');
GO

INSERT INTO Embedded (IpAddress, NetworkName, DeviceId) VALUES
    ('192.168.1.10', 'SensorNet', 'EM-01');
GO