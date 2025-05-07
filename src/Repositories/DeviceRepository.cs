using System.Data;
using Microsoft.Data.SqlClient;
using Web;

namespace Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly string _connectionString;

    public DeviceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<IEnumerable<DeviceDto>> GetAllAsync()
    {
        var devices = new List<DeviceDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var sql = @"
            SELECT d.Id, d.Name, d.IsEnabled,
                   pc.OperationSystem, e.IpAddress, e.NetworkName, sw.BatteryPercentage,
                   d.RowVersion
            FROM Device d
            LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
            LEFT JOIN Embedded e ON d.Id = e.DeviceId
            LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId";
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            devices.Add(new DeviceDto
            {
                Id               = reader.GetString(0),
                Name             = reader.GetString(1),
                IsEnabled        = reader.GetBoolean(2),
                Type             = reader.IsDBNull(3) ? (reader.IsDBNull(4) ? (reader.IsDBNull(6) ? "device" : "smartwatch") : "embedded") : "pc",
                OperatingSystem  = reader.IsDBNull(3) ? null : reader.GetString(3),
                IpAddress        = reader.IsDBNull(4) ? null : reader.GetString(4),
                NetworkName      = reader.IsDBNull(5) ? null : reader.GetString(5),
                BatteryPercentage= reader.IsDBNull(6) ? null : reader.GetInt32(6),
                RowVersion       = (byte[])reader["RowVersion"]
            });
        }
        return devices;
    }

    public async Task<DeviceDto?> GetByIdAsync(string id)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var sql = @"
            SELECT d.Id, d.Name, d.IsEnabled,
                   pc.OperationSystem, e.IpAddress, e.NetworkName, sw.BatteryPercentage,
                   d.RowVersion
            FROM Device d
            LEFT JOIN PersonalComputer pc ON d.Id = pc.DeviceId
            LEFT JOIN Embedded e ON d.Id = e.DeviceId
            LEFT JOIN Smartwatch sw ON d.Id = sw.DeviceId
            WHERE d.Id = @Id";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new DeviceDto
        {
            Id               = reader.GetString(0),
            Name             = reader.GetString(1),
            IsEnabled        = reader.GetBoolean(2),
            Type             = reader.IsDBNull(3) ? (reader.IsDBNull(4) ? (reader.IsDBNull(6) ? "device" : "smartwatch") : "embedded") : "pc",
            OperatingSystem  = reader.IsDBNull(3) ? null : reader.GetString(3),
            IpAddress        = reader.IsDBNull(4) ? null : reader.GetString(4),
            NetworkName      = reader.IsDBNull(5) ? null : reader.GetString(5),
            BatteryPercentage= reader.IsDBNull(6) ? null : reader.GetInt32(6),
            RowVersion       = (byte[])reader["RowVersion"]
        };
    }

    public async Task CreateAsync(DeviceDto dto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();
        try
        {
            await using (var cmd = new SqlCommand("usp_InsertDevice", conn, tx) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@Id", dto.Id);
                cmd.Parameters.AddWithValue("@Name", dto.Name);
                cmd.Parameters.AddWithValue("@IsEnabled", dto.IsEnabled);
                await cmd.ExecuteNonQueryAsync();
            }
            switch (dto.Type.ToLower())
            {
                case "embedded":
                    await using (var cmd = new SqlCommand("usp_InsertEmbedded", conn, tx) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", dto.Id);
                        cmd.Parameters.AddWithValue("@IpAddress", dto.IpAddress!);
                        cmd.Parameters.AddWithValue("@NetworkName", dto.NetworkName!);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
                case "pc":
                    await using (var cmd = new SqlCommand("usp_InsertPersonalComputer", conn, tx) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", dto.Id);
                        cmd.Parameters.AddWithValue("@OperationSystem", dto.OperatingSystem!);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
                case "smartwatch":
                    await using (var cmd = new SqlCommand("usp_InsertSmartwatch", conn, tx) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", dto.Id);
                        cmd.Parameters.AddWithValue("@BatteryPercentage", dto.BatteryPercentage!.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
            }
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(DeviceDto dto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();
        try
        {
            await using (var cmd = new SqlCommand(@"
                UPDATE Device
                SET Name = @Name, IsEnabled = @IsEnabled
                WHERE Id = @Id AND RowVersion = @RowVersion;
                SELECT @@ROWCOUNT;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Name", dto.Name);
                cmd.Parameters.AddWithValue("@IsEnabled", dto.IsEnabled);
                cmd.Parameters.AddWithValue("@Id", dto.Id);
                cmd.Parameters.Add("@RowVersion", SqlDbType.Timestamp).Value = dto.RowVersion;
                var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (affected == 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }
            }
            switch (dto.Type.ToLower())
            {
                case "embedded":
                    await using (var cmd = new SqlCommand(
                        "UPDATE Embedded SET IpAddress = @Ip, NetworkName = @Net WHERE DeviceId = @Id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Ip", dto.IpAddress!);
                        cmd.Parameters.AddWithValue("@Net", dto.NetworkName!);
                        cmd.Parameters.AddWithValue("@Id", dto.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
                case "pc":
                    await using (var cmd = new SqlCommand(
                        "UPDATE PersonalComputer SET OperationSystem = @OS WHERE DeviceId = @Id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@OS", dto.OperatingSystem!);
                        cmd.Parameters.AddWithValue("@Id", dto.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
                case "smartwatch":
                    await using (var cmd = new SqlCommand(
                        "UPDATE Smartwatch SET BatteryPercentage = @Batt WHERE DeviceId = @Id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@Batt", dto.BatteryPercentage!.Value);
                        cmd.Parameters.AddWithValue("@Id", dto.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    break;
            }
            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();
        try
        {
            await using (var cmd = new SqlCommand(@"
                DELETE FROM Embedded WHERE DeviceId = @Id;
                DELETE FROM Smartwatch WHERE DeviceId = @Id;
                DELETE FROM PersonalComputer WHERE DeviceId = @Id;
                DELETE FROM Device WHERE Id = @Id;", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}