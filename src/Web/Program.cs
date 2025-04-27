using DeviceManager;
using DeviceManager.EmbeddedDevice;
using DeviceManager.PersonalComputer;
using DeviceManager.SmartWatch;
using Microsoft.Data.SqlClient;
using Web;
using ArgumentException = System.ArgumentException;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/devices", async () =>
{
    var devices = new List<object>();
        
    await using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        
        var command = new SqlCommand("SELECT Id, Name, IsEnabled FROM Device", connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            devices.Add(new
            {
                Id = reader["Id"].ToString(),
                Name = reader["Name"].ToString(),
                IsEnabled = (bool)reader["IsEnabled"]
            });
        }
    }
    return Results.Ok(devices);
});

app.MapGet("/devices/{id}", async (string id) =>
{
    await using var connection = new SqlConnection(connectionString);
    
    connection.Open();

    var command = new SqlCommand(
        @"SELECT *
                  FROM Device d
                  LEFT JOIN Embedded e ON d.Id = e.DeviceId
                  LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
                  LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId
                  WHERE d.Id = @DeviceId;",
        connection
    );

    command.Parameters.AddWithValue("@DeviceId", id);
        
    await using var reader = await command.ExecuteReaderAsync();
        
    if (await reader.ReadAsync())
    {
        var device = new
        {
            Id = reader["Id"].ToString(),
            Name = reader["Name"].ToString(),
            IsEnabled = (bool) reader["IsEnabled"],
            IpAddress = reader["IpAddress"] as string,
            NetworkName = reader["NetworkName"] as string,
            OperationSystem = reader["OperationSystem"] as string,
            BatteryPercentage = reader["BatteryPercentage"] as int?
        };

        return Results.Ok(device);
    }

    return Results.NotFound();
});

app.MapDelete("/devices/{id}", async (string id) =>
{
    await using var connection = new SqlConnection(connectionString);
    
    connection.Open();

    var command = new SqlCommand(
        @"
            DELETE FROM Embedded WHERE DeviceId = @Id;
            DELETE FROM Smartwatch WHERE DeviceId = @Id;
            DELETE FROM PersonalComputer WHERE DeviceId = @Id;
            DELETE FROM Device WHERE Id = @Id;
        ", 
        connection
    );
    
    command.Parameters.AddWithValue("@Id", id);
    command.ExecuteNonQuery();
        
    return Results.NoContent();
});

app.Run();
