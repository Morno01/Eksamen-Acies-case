using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Services
{
    public class PalleOptimeringSettingsService : IPalleOptimeringSettingsService
    {
        private readonly PalleOptimeringContext _context;

        public PalleOptimeringSettingsService(PalleOptimeringContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PalleOptimeringSettings>> GetAlleSettings()
        {
            return await _context.Settings.ToListAsync();
        }

        public async Task<PalleOptimeringSettings?> GetSettings(int id)
        {
            return await _context.Settings.FindAsync(id);
        }

        public async Task<PalleOptimeringSettings?> GetAktivSettings()
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.Aktiv);
        }

        public async Task<PalleOptimeringSettings> OpretSettings(PalleOptimeringSettings settings)
        {
            _context.Settings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<PalleOptimeringSettings> OpdaterSettings(PalleOptimeringSettings settings)
        {
            _context.Entry(settings).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<bool> SletSettings(int id)
        {
            var settings = await _context.Settings.FindAsync(id);
            if (settings == null)
                return false;

            _context.Settings.Remove(settings);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
