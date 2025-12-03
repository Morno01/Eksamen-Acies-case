using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Services
{
    public class PalleService : IPalleService
    {
        private readonly PalleOptimeringContext _context;

        public PalleService(PalleOptimeringContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Palle>> GetAlleAktivePaller()
        {
            return await _context.Paller
                .Where(p => p.Aktiv)
                .OrderBy(p => p.Sortering)
                .ToListAsync();
        }

        public async Task<IEnumerable<Palle>> GetAllePaller()
        {
            return await _context.Paller
                .OrderBy(p => p.Sortering)
                .ToListAsync();
        }

        public async Task<Palle?> GetPalle(int id)
        {
            return await _context.Paller.FindAsync(id);
        }

        public async Task<Palle> OpretPalle(Palle palle)
        {
            _context.Paller.Add(palle);
            await _context.SaveChangesAsync();
            return palle;
        }

        public async Task<Palle> OpdaterPalle(Palle palle)
        {
            _context.Entry(palle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return palle;
        }

        public async Task<bool> SletPalle(int id)
        {
            var palle = await _context.Paller.FindAsync(id);
            if (palle == null)
                return false;

            _context.Paller.Remove(palle);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
