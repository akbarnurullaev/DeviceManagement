using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Nodes;
using Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default");

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

const string baseUri = "/api/devices";

app.MapGet(baseUri, async () =>
{
    var devices = new List<DeviceDto>();
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("SELECT Id, Name, IsEnabled FROM Device", conn);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        devices.Add(new DeviceDto
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1),
            Type = "device",
            IsEnabled = reader.GetBoolean(2)
        });
    }
    return Results.Ok(devices);
});

app.MapGet(baseUri + "/{id}", async (string id) =>
{
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    var sql = @"
        SELECT d.Id, d.Name, d.IsEnabled,
               pc.OperationSystem, e.IpAddress, e.NetworkName, sw.BatteryPercentage
        FROM Device d
        LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
        LEFT JOIN Embedded e ON d.Id = e.DeviceId
        LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId
        WHERE d.Id = @Id";
    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Id", id);
    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return Results.NotFound();

    var dto = new DeviceDto
    {
        Id = reader.GetString(0),
        Name = reader.GetString(1),
        Type = reader.IsDBNull(3) ?
               reader.IsDBNull(4) ?
                   reader.IsDBNull(6) ? "device" : "smartwatch"
               : "embedded"
               : "pc",
        OperatingSystem = reader.IsDBNull(3) ? null : reader.GetString(3),
        IpAddress = reader.IsDBNull(4) ? null : reader.GetString(4),
        NetworkName = reader.IsDBNull(5) ? null : reader.GetString(5),
        BatteryPercentage = reader.IsDBNull(6) ? null : reader.GetInt32(6)
    };
    return Results.Ok(dto);
});

app.MapPost(baseUri, async (HttpRequest request) =>
{
    string? contentType = request.ContentType?.ToLower();
    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string raw = await reader.ReadToEndAsync();
            var json = JsonNode.Parse(raw);
            if (json == null) return Results.BadRequest();
            var type = json["type"]?.GetValue<string>();
            var valNode = json["typeValue"];
            if (type == null || valNode == null) return Results.BadRequest();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var dto = JsonSerializer.Deserialize<DeviceDto>(valNode.ToString(), opts);
            if (dto == null) return Results.BadRequest();
            
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using (var tx = conn.BeginTransaction())
            {
                var insDevice = new SqlCommand(
                    "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)", conn, tx);
                insDevice.Parameters.AddWithValue("@Id", dto.Id);
                insDevice.Parameters.AddWithValue("@Name", dto.Name);
                insDevice.Parameters.AddWithValue("@IsEnabled", true);
                await insDevice.ExecuteNonQueryAsync();

                switch(type.ToLower())
                {
                    case "embedded":
                        var insEmb = new SqlCommand(
                            "INSERT INTO Embedded (DeviceId, IpAddress, NetworkName) VALUES (@Id,@Ip,@Net)", conn, tx);
                        insEmb.Parameters.AddWithValue("@Id", dto.Id);
                        insEmb.Parameters.AddWithValue("@Ip", dto.IpAddress!);
                        insEmb.Parameters.AddWithValue("@Net", dto.NetworkName!);
                        await insEmb.ExecuteNonQueryAsync();
                        break;
                    case "pc":
                        var insPc = new SqlCommand(
                            "INSERT INTO PersonalComputer (DeviceId, OperationSystem) VALUES (@Id,@OS)", conn, tx);
                        insPc.Parameters.AddWithValue("@Id", dto.Id);
                        insPc.Parameters.AddWithValue("@OS", dto.OperatingSystem!);
                        await insPc.ExecuteNonQueryAsync();
                        break;
                    case "smartwatch":
                        var insSw = new SqlCommand(
                            "INSERT INTO Smartwatch (DeviceId, BatteryPercentage) VALUES (@Id,@Batt)", conn, tx);
                        insSw.Parameters.AddWithValue("@Id", dto.Id);
                        insSw.Parameters.AddWithValue("@Batt", dto.BatteryPercentage!.Value);
                        await insSw.ExecuteNonQueryAsync();
                        break;
                    default:
                        tx.Rollback();
                        return Results.BadRequest($"Unknown device type '{type}'");
                }
                tx.Commit();
            }
            return Results.Created($"{baseUri}/{dto.Id}", dto);
        }
        case "text/plain":
        {
            using var reader = new StreamReader(request.Body);
            var line = await reader.ReadToEndAsync();
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) return Results.BadRequest("Invalid format");
            var type = parts[0].ToLower();
            var idPart = parts[1];
            var namePart = parts[2];

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var insDevice = new SqlCommand(
                "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)", conn, tx);
            insDevice.Parameters.AddWithValue("@Id", idPart);
            insDevice.Parameters.AddWithValue("@Name", namePart);
            insDevice.Parameters.AddWithValue("@IsEnabled", true);
            await insDevice.ExecuteNonQueryAsync();

            switch (type)
            {
                case "embedded":
                    if (parts.Length < 5) { tx.Rollback(); return Results.BadRequest("Invalid format for embedded"); }
                    string ip = parts[3], net = parts[4];
                    var insEmb = new SqlCommand(
                        "INSERT INTO Embedded (DeviceId, IpAddress, NetworkName) VALUES (@Id,@Ip,@Net)", conn, tx);
                    insEmb.Parameters.AddWithValue("@Id", idPart);
                    insEmb.Parameters.AddWithValue("@Ip", ip);
                    insEmb.Parameters.AddWithValue("@Net", net);
                    await insEmb.ExecuteNonQueryAsync();
                    break;
                case "pc":
                    if (parts.Length < 4) { tx.Rollback(); return Results.BadRequest("Invalid format for pc"); }
                    var os = parts[3];
                    var insPc = new SqlCommand(
                        "INSERT INTO PersonalComputer (DeviceId, OperationSystem) VALUES (@Id,@OS)", conn, tx);
                    insPc.Parameters.AddWithValue("@Id", idPart);
                    insPc.Parameters.AddWithValue("@OS", os);
                    await insPc.ExecuteNonQueryAsync();
                    break;
                case "smartwatch":
                    if (parts.Length < 4) { tx.Rollback(); return Results.BadRequest("Invalid format for smartwatch"); }
                    if (!int.TryParse(parts[3], out var batt)) { tx.Rollback(); return Results.BadRequest("Invalid battery percentage"); }
                    var insSw = new SqlCommand(
                        "INSERT INTO Smartwatch (DeviceId, BatteryPercentage) VALUES (@Id,@Batt)", conn, tx);
                    insSw.Parameters.AddWithValue("@Id", idPart);
                    insSw.Parameters.AddWithValue("@Batt", batt);
                    await insSw.ExecuteNonQueryAsync();
                    break;
                default:
                    tx.Rollback();
                    return Results.BadRequest($"Unknown device type '{type}'");
            }

            tx.Commit();
            var dto = new DeviceDto { Type = type, Id = idPart, Name = namePart };
            return Results.Created($"{baseUri}/{idPart}", dto);
        }
        default:
            return Results.Conflict();
    }
})
    .Accepts<DeviceDto>("application/json")
    .Accepts<string>("text/plain");

app.MapPut(baseUri + "/{id}", async (string id, HttpRequest request) =>
{
    string? contentType = request.ContentType?.ToLower();
    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string raw = await reader.ReadToEndAsync();
            var json = JsonNode.Parse(raw);
            if (json == null) return Results.BadRequest();
            var type = json["type"]?.GetValue<string>();
            var valNode = json["typeValue"];
            if (type == null || valNode == null) return Results.BadRequest();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<DeviceDto>(valNode.ToString(), opts);
            if (dto == null) return Results.BadRequest();
            if (dto.Id != id) return Results.BadRequest("ID mismatch");

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var updDevice = new SqlCommand(
                "UPDATE Device SET Name=@Name, IsEnabled=@IsEnabled WHERE Id=@Id", conn, tx);
            updDevice.Parameters.AddWithValue("@Name", dto.Name);
            updDevice.Parameters.AddWithValue("@IsEnabled", dto.IsEnabled);
            updDevice.Parameters.AddWithValue("@Id", dto.Id);
            await updDevice.ExecuteNonQueryAsync();

            switch (type.ToLower())
            {
                case "embedded":
                    var updEmb = new SqlCommand(
                        "UPDATE Embedded SET IpAddress=@Ip, NetworkName=@Net WHERE DeviceId=@Id",
                        conn, tx);
                    updEmb.Parameters.AddWithValue("@Ip", dto.IpAddress!);
                    updEmb.Parameters.AddWithValue("@Net", dto.NetworkName!);
                    updEmb.Parameters.AddWithValue("@Id", dto.Id);
                    await updEmb.ExecuteNonQueryAsync();
                    break;
                case "pc":
                    var updPc = new SqlCommand(
                        "UPDATE PersonalComputer SET OperationSystem=@OS WHERE DeviceId=@Id",
                        conn, tx);
                    updPc.Parameters.AddWithValue("@OS", dto.OperatingSystem!);
                    updPc.Parameters.AddWithValue("@Id", dto.Id);
                    await updPc.ExecuteNonQueryAsync();
                    break;
                case "smartwatch":
                    var updSw = new SqlCommand(
                        "UPDATE Smartwatch SET BatteryPercentage=@Batt WHERE DeviceId=@Id",
                        conn, tx);
                    updSw.Parameters.AddWithValue("@Batt", dto.BatteryPercentage!.Value);
                    updSw.Parameters.AddWithValue("@Id", dto.Id);
                    await updSw.ExecuteNonQueryAsync();
                    break;
                default:
                    tx.Rollback();
                    return Results.BadRequest($"Unknown device type '{type}'");
            }

            tx.Commit();
            return Results.NoContent();
        }
        case "text/plain":
        {
            using var reader = new StreamReader(request.Body);
            var line = await reader.ReadToEndAsync();
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) return Results.BadRequest("Invalid format");
            var type = parts[0].ToLower();
            var idPart = parts[1];
            var namePart = parts[2];

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            var updDevice = new SqlCommand(
                "UPDATE Device SET Name=@Name WHERE Id=@Id", conn, tx);
            updDevice.Parameters.AddWithValue("@Name", namePart);
            updDevice.Parameters.AddWithValue("@Id", idPart);
            await updDevice.ExecuteNonQueryAsync();

            switch (type)
            {
                case "embedded":
                    if (parts.Length < 5) { tx.Rollback(); return Results.BadRequest("Invalid format for embedded"); }
                    string ip = parts[3], net = parts[4];
                    var updEmb = new SqlCommand(
                        "UPDATE Embedded SET IpAddress=@Ip, NetworkName=@Net WHERE DeviceId=@Id", conn, tx);
                    updEmb.Parameters.AddWithValue("@Ip", ip);
                    updEmb.Parameters.AddWithValue("@Net", net);
                    updEmb.Parameters.AddWithValue("@Id", idPart);
                    await updEmb.ExecuteNonQueryAsync();
                    break;
                case "pc":
                    if (parts.Length < 4) { tx.Rollback(); return Results.BadRequest("Invalid format for pc"); }
                    var os = parts[3];
                    var updPc = new SqlCommand(
                        "UPDATE PersonalComputer SET OperationSystem=@OS WHERE DeviceId=@Id", conn, tx);
                    updPc.Parameters.AddWithValue("@OS", os);
                    updPc.Parameters.AddWithValue("@Id", idPart);
                    await updPc.ExecuteNonQueryAsync();
                    break;
                case "smartwatch":
                    if (parts.Length < 4) { tx.Rollback(); return Results.BadRequest("Invalid format for smartwatch"); }
                    if (!int.TryParse(parts[3], out var batt)) { tx.Rollback(); return Results.BadRequest("Invalid battery percentage"); }
                    var updSw = new SqlCommand(
                        "UPDATE Smartwatch SET BatteryPercentage=@Batt WHERE DeviceId=@Id", conn, tx);
                    updSw.Parameters.AddWithValue("@Batt", batt);
                    updSw.Parameters.AddWithValue("@Id", idPart);
                    await updSw.ExecuteNonQueryAsync();
                    break;
                default:
                    tx.Rollback();
                    return Results.BadRequest($"Unknown device type '{type}'");
            }

            tx.Commit();
            return Results.NoContent();
        }
        default:
            return Results.BadRequest("Unsupported content type");
    }
})
    .Accepts<DeviceDto>("application/json")
    .Accepts<string>("text/plain");

app.MapDelete(baseUri + "/{id}", async (string id) =>
{
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    using var cmd = new SqlCommand(
        "DELETE FROM Embedded WHERE DeviceId=@Id; DELETE FROM Smartwatch WHERE DeviceId=@Id; DELETE FROM PersonalComputer WHERE DeviceId=@Id; DELETE FROM Device WHERE Id=@Id;",
        conn);
    cmd.Parameters.AddWithValue("@Id", id);
    await cmd.ExecuteNonQueryAsync();
    return Results.NoContent();
});

app.Run();
