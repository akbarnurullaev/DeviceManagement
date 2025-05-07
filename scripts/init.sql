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
    IsEnabled BIT NOT NULL,
    RowVersion ROWVERSION NOT NULL
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

-- Stored Procedures
IF OBJECT_ID('usp_InsertDevice', 'P') IS NOT NULL
    DROP PROCEDURE usp_InsertDevice;
GO
CREATE PROCEDURE usp_InsertDevice
    @Id VARCHAR(50),
    @Name VARCHAR(250),
    @IsEnabled BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Device (Id, Name, IsEnabled)
    VALUES (@Id, @Name, @IsEnabled);
END
GO

IF OBJECT_ID('usp_InsertEmbedded', 'P') IS NOT NULL
    DROP PROCEDURE usp_InsertEmbedded;
GO
CREATE PROCEDURE usp_InsertEmbedded
    @DeviceId VARCHAR(50),
    @IpAddress VARCHAR(250),
    @NetworkName VARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Embedded (DeviceId, IpAddress, NetworkName)
    VALUES (@DeviceId, @IpAddress, @NetworkName);
END
GO

IF OBJECT_ID('usp_InsertPersonalComputer', 'P') IS NOT NULL
    DROP PROCEDURE usp_InsertPersonalComputer;
GO
CREATE PROCEDURE usp_InsertPersonalComputer
    @DeviceId VARCHAR(50),
    @OperationSystem VARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO PersonalComputer (DeviceId, OperationSystem)
    VALUES (@DeviceId, @OperationSystem);
END
GO

IF OBJECT_ID('usp_InsertSmartwatch', 'P') IS NOT NULL
    DROP PROCEDURE usp_InsertSmartwatch;
GO
CREATE PROCEDURE usp_InsertSmartwatch
    @DeviceId VARCHAR(50),
    @BatteryPercentage INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Smartwatch (DeviceId, BatteryPercentage)
    VALUES (@DeviceId, @BatteryPercentage);
END
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