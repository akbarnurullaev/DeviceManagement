using Web;

namespace Repositories;

public interface IDeviceRepository
{
    Task<IEnumerable<DeviceDto>> GetAllAsync();
    Task<DeviceDto?> GetByIdAsync(string id);
    Task CreateAsync(DeviceDto dto);
    Task<bool> UpdateAsync(DeviceDto dto);
    Task DeleteAsync(string id);
}