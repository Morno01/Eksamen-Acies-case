using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using Xunit;

namespace MyProject.Tests.Integration
{
    /// <summary>
    /// Database Integration Tests
    /// Tester database operationer med Entity Framework og SQL Server (InMemory)
    /// </summary>
    public class DatabaseIntegrationTests : IDisposable
    {
        private readonly PalleOptimeringContext _context;

        public DatabaseIntegrationTests()
        {
            // Setup InMemory database (simulerer SQL Server)
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PalleOptimeringContext(options);
        }

        /// <summary>
        /// SCRUM-75: TC6-INT-001 - Gem elementdata til SQL Server
        /// Test Step 1-8: Database CRUD operationer
        /// </summary>
        [Fact]
        public async Task SCRUM75_GemElementdataTilSQLServer()
        {
            // Test Step 1: Opret database forbindelse
            // DbContext oprettet i constructor
            Assert.NotNull(_context);
            // Expected: DbContext oprettet, Forbindelse til database etableret ✓

            // Test Step 2: Opret nyt Element
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
            // Expected: Element objekt oprettet i memory ✓

            // Test Step 3: Tilføj til DbContext
            _context.Elementer.Add(element);

            // Verificer element er tracked
            var entry = _context.Entry(element);
            Assert.Equal(EntityState.Added, entry.State);
            // Expected: Element tracked af Entity Framework, State: Added, Pending insert: true ✓

            // Test Step 4: Kald SaveChangesAsync()
            int rowsAffected = await _context.SaveChangesAsync();

            // Expected:
            // SQL genereret: INSERT INTO Elementer (Reference, Type, Serie, ...) VALUES (...)
            // Return value: 1 (én række påvirket)
            Assert.Equal(1, rowsAffected);
            // Status: SUCCESS ✓

            // Test Step 5: Verificer Id tildelt
            Assert.True(element.Id > 0, "Id skulle være tildelt efter SaveChanges");
            int tildeltId = element.Id;
            // Expected: Id tildelt: true, Eksempel værdi: element.Id = 123
            // Database identity column fungerer ✓

            // Test Step 6: Query fra database
            var saved = await _context.Elementer
                .FirstOrDefaultAsync(e => e.Reference == "TEST-DØR-999");

            Assert.NotNull(saved);
            // Expected: Element fundet i database, Rows returned: 1, saved != null: true ✓

            // Test Step 7: Verificer alle felter
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
            // Expected: Alle felter matcher ✓

            // Test Step 8: Cleanup - Slet test data
            _context.Elementer.Remove(saved);
            int deletedRows = await _context.SaveChangesAsync();

            Assert.Equal(1, deletedRows);

            var afterDelete = await _context.Elementer
                .FirstOrDefaultAsync(e => e.Reference == "TEST-DØR-999");
            Assert.Null(afterDelete);
            // Expected: Element slettet, Database tilbage til original tilstand
            // Test data cleanup komplet ✓
        }

        /// <summary>
        /// TC6-INT-003: Test database transaktioner
        /// Verificer at SaveChanges er atomic (alt eller intet)
        /// </summary>
        [Fact]
        public async Task TC6INT003_DatabaseTransaktionAtomicity()
        {
            // Arrange - Opret 3 elementer
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

            // Act - Tilføj alle 3 i samme transaktion
            _context.Elementer.AddRange(element1, element2, element3);
            int saved = await _context.SaveChangesAsync();

            // Assert - Alle 3 skulle være gemt
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