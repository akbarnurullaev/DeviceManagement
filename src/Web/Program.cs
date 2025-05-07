using Repositories;
using Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

const string baseUri = "/api/devices";

app.MapGet(baseUri, async (IDeviceRepository repo) =>
    Results.Ok(await repo.GetAllAsync()));

app.MapGet(baseUri + "/{id}", async (IDeviceRepository repo, string id) =>
{
    var device = await repo.GetByIdAsync(id);
    return device is null ? Results.NotFound() : Results.Ok(device);
});

app.MapPost(baseUri, async (IDeviceRepository repo, DeviceDto dto) =>
{
    await repo.CreateAsync(dto);
    return Results.Created($"{baseUri}/{dto.Id}", dto);
})
.Accepts<DeviceDto>("application/json");

app.MapPut(baseUri + "/{id}", async (IDeviceRepository repo, string id, DeviceDto dto) =>
{
    if (id != dto.Id) return Results.BadRequest("ID mismatch");
    var updated = await repo.UpdateAsync(dto);
    return updated ? Results.NoContent() : Results.StatusCode(StatusCodes.Status412PreconditionFailed);
})
.Accepts<DeviceDto>("application/json");

app.MapDelete(baseUri + "/{id}", async (IDeviceRepository repo, string id) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();
