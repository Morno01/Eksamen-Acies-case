using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using Xunit;

namespace MyProject.Tests.Integration
{
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly PalleOptimeringContext _context;

        public DatabaseIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PalleOptimeringContext(options);
        }

        [Fact]
        public async Task SCRUM75_GemElementdataTilSQLServer()
        {
            Assert.NotNull(_context);

            var element = new Element
            {
                Reference = "TEST-DØR-999",
                Type = "Dør",
                Maerke = "Test Brand",
                Serie = "Test Serie",
                Hoejde = 2100,
                Bredde = 900,
                Dybde = 100,
                Vaegt = 45.5m,
                RotationsRegel = "Ja",
                ErSpecialelement = false,
                ErGeometrielement = false
            };

            _context.Elementer.Add(element);

            var entry = _context.Entry(element);
            Assert.Equal(EntityState.Added, entry.State);

            int rowsAffected = await _context.SaveChangesAsync();

            Assert.Equal(1, rowsAffected);

            Assert.True(element.Id > 0, "Id skulle være tildelt efter SaveChanges");
            int tildeltId = element.Id;

            var saved = await _context.Elementer
                .FirstOrDefaultAsync(e => e.Reference == "TEST-DØR-999");

            Assert.NotNull(saved);

            Assert.Equal("TEST-DØR-999", saved.Reference);
            Assert.Equal("Dør", saved.Type);
            Assert.Equal("Test Brand", saved.Maerke);
            Assert.Equal("Test Serie", saved.Serie);
            Assert.Equal(2100, saved.Hoejde);
            Assert.Equal(900, saved.Bredde);
            Assert.Equal(100, saved.Dybde);
            Assert.Equal(45.5m, saved.Vaegt);
            Assert.Equal("Ja", saved.RotationsRegel);
            Assert.False(saved.ErSpecialelement);
            Assert.False(saved.ErGeometrielement);
            Assert.Equal(tildeltId, saved.Id);

            _context.Elementer.Remove(saved);
            int deletedRows = await _context.SaveChangesAsync();

            Assert.Equal(1, deletedRows);

            var afterDelete = await _context.Elementer
                .FirstOrDefaultAsync(e => e.Reference == "TEST-DØR-999");
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task TC6INT003_DatabaseTransaktionAtomicity()
        {
            var element1 = new Element
            {
                Reference = "TRANS-001",
                Type = "Vindue",
                Hoejde = 1200,
                Bredde = 800,
                Dybde = 100,
                Vaegt = 30m,
                RotationsRegel = "Ja"
            };

            var element2 = new Element
            {
                Reference = "TRANS-002",
                Type = "Vindue",
                Hoejde = 1200,
                Bredde = 800,
                Dybde = 100,
                Vaegt = 30m,
                RotationsRegel = "Ja"
            };

            var element3 = new Element
            {
                Reference = "TRANS-003",
                Type = "Dør",
                Hoejde = 2100,
                Bredde = 900,
                Dybde = 120,
                Vaegt = 50m,
                RotationsRegel = "Nej"
            };

            _context.Elementer.AddRange(element1, element2, element3);
            int saved = await _context.SaveChangesAsync();

            Assert.Equal(3, saved);

            var count = await _context.Elementer
                .CountAsync(e => e.Reference!.StartsWith("TRANS-"));
            Assert.Equal(3, count);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}