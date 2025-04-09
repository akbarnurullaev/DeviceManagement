using DeviceManager;
using DeviceManager.EmbeddedDevice;
using DeviceManager.PersonalComputer;
using DeviceManager.SmartWatch;
using Web;
using ArgumentException = System.ArgumentException;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

List<Device> devices = new List<Device>
{
    new Smartwatch("SW-01", "Fitness Tracker", 85),
    new PersonalComputer("PC-01", "Nigga boo", "Mac OS"),
};

app.MapGet("/devices", () =>
{
    var result = devices.Select(d => new
    {
        d.Id,
        d.Name,
        Type = d.GetType().Name,
        d.IsTurnedOn
    });
    return Results.Ok(result);
});

app.MapGet("/devices/{id}", (string id) =>
{
    var device = devices.FirstOrDefault(d => d.Id == id);
    return device is null ? Results.NotFound() : Results.Ok(device.ToString());
});

app.MapPost("/devices", (DeviceDto dto) =>
{
    if (devices.Any(d => d.Id == dto.Id))
        return Results.Conflict("Device with this ID already exists.");

    Device device = dto.Type switch
    {
        "PC" => new PersonalComputer(dto.Id, dto.Name, dto.OperatingSystem),
        "Embedded" => new EmbeddedDevice(dto.Id, dto.Name, dto.IpAddress!, dto.NetworkName!),
        "Smartwatch" => new Smartwatch(dto.Id, dto.Name, dto.BatteryPercentage ?? 100),
        _ => throw new ArgumentException("Unknown device type")
    };

    devices.Add(device);
    return Results.Created($"/devices/{device.Id}", device.ToString());
});

app.MapPut("/devices/{id}", (string id, DeviceDto dto) =>
{
    var index = devices.FindIndex(d => d.Id == id);
    if (index == -1) return Results.NotFound();

    Device updated = dto.Type switch
    {
        "PC" => new PersonalComputer(id, dto.Name, dto.OperatingSystem),
        "Embedded" => new EmbeddedDevice(id, dto.Name, dto.IpAddress!, dto.NetworkName!),
        "Smartwatch" => new Smartwatch(id, dto.Name, dto.BatteryPercentage ?? 100),
        _ => throw new ArgumentException("Unknown device type")
    };

    devices[index] = updated;
    return Results.NoContent();
});

app.MapDelete("/devices/{id}", (string id) =>
{
    var device = devices.FirstOrDefault(d => d.Id == id);
    if (device == null) return Results.NotFound();

    devices.Remove(device);
    return Results.NoContent();
});

app.Run();
