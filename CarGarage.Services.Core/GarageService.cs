using CarGarage.Data;
using CarGarage.ViewModels.Garage;
using Microsoft.EntityFrameworkCore;

public class GarageService : IGarageService
{
    private readonly ApplicationDbContext _context;
    public GarageService(ApplicationDbContext context) => _context = context;

    public async Task<bool> HasGarageAsync(string userId)
        => await _context.Garages.AnyAsync(g => g.OwnerId == userId);

    public async Task CreateGarageAsync(GarageViewModel model, string userId)
    {
        var garage = new Garage
        {
            Name = model.Name,
            Bulstat = model.Bulstat,
            City = model.City,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber, // Добавено
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            OwnerId = userId
        };
        await _context.Garages.AddAsync(garage);
        await _context.SaveChangesAsync();
    }

    public async Task<GarageViewModel?> GetGarageDetailsAsync(string userId)
    {
        return await _context.Garages
            .Where(g => g.OwnerId == userId)
            .Select(g => new GarageViewModel
            {
                OwnerName = g.OwnerName,
                Name = g.Name,
                Bulstat = g.Bulstat,
                City = g.City,
                Address = g.Address,
                PhoneNumber = g.PhoneNumber,
                Latitude = g.Latitude,
                Longitude = g.Longitude
            })
            .FirstOrDefaultAsync();
    }

    public async Task UpdateGarageAsync(GarageViewModel model, string userId)
    {
        var garage = await _context.Garages.FirstOrDefaultAsync(g => g.OwnerId == userId);

        if (garage != null)
        {
            garage.Name = model.Name;
            garage.Bulstat = model.Bulstat;
            garage.City = model.City;
            garage.Address = model.Address;
            garage.PhoneNumber = model.PhoneNumber;
            garage.Latitude = model.Latitude;
            garage.Longitude = model.Longitude;
            garage.OwnerName = model.OwnerName;

            await _context.SaveChangesAsync();
        }
    }
}