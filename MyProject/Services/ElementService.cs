using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;

namespace MyProject.Services
{
    public class ElementService : IElementService
    {
        private readonly PalleOptimeringContext _context;

        public ElementService(PalleOptimeringContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Element>> GetAlleElementer()
        {
            return await _context.Elementer.ToListAsync();
        }

        public async Task<Element?> GetElement(int id)
        {
            return await _context.Elementer.FindAsync(id);
        }

        public async Task<Element> OpretElement(Element element)
        {
            _context.Elementer.Add(element);
            await _context.SaveChangesAsync();
            return element;
        }

        public async Task<Element> OpdaterElement(Element element)
        {
            _context.Entry(element).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return element;
        }

        public async Task<bool> SletElement(int id)
        {
            var element = await _context.Elementer.FindAsync(id);
            if (element == null)
                return false;

            _context.Elementer.Remove(element);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Element>> OpretFlereElementer(IEnumerable<Element> elementer)
        {
            _context.Elementer.AddRange(elementer);
            await _context.SaveChangesAsync();
            return elementer;
        }
    }
}
