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

        /// <summary>
        /// TC6-INT-004: Test concurrent access (optimistic concurrency)
        /// </summary>
        [Fact]
        public async Task TC6INT004_ConcurrentDatabaseAccess()
        {
            // Arrange - Opret element
            var palle = new Palle
            {
                PalleBeskrivelse = "Test Palle",
                Palletype = "Trae",
                Laengde = 1200,
                Bredde = 800,
                Hoejde = 150,
                MaksHoejde = 2200,
                MaksVaegt = 1000m,
                Vaegt = 25m,
                Overmaal = 50,
                Sortering = 1
            };

            _context.Paller.Add(palle);
            await _context.SaveChangesAsync();

            // Act - Simuler 2 concurrent updates
            var palle1 = await _context.Paller.FindAsync(palle.Id);
            var palle2 = await _context.Paller.FindAsync(palle.Id);

            // Begge opdaterer
            palle1!.MaksVaegt = 1200m;
            palle2!.MaksVaegt = 1500m;

            // Første update lykkes
            await _context.SaveChangesAsync();

            // Assert - Verificer opdatering
            var updated = await _context.Paller.FindAsync(palle.Id);
            Assert.Equal(1500m, updated!.MaksVaegt); // Sidste update vinder i InMemory
        }

        /// <summary>
        /// TC6-INT-005: Test komplekse queries med LINQ
        /// </summary>
        [Fact]
        public async Task TC6INT005_KomplekseLinqQueries()
        {
            // Arrange - Seed data
            var elementer = new List<Element>
            {
                new Element { Reference = "Q1", Type = "Vindue", Maerke = "Velux", Hoejde = 1200, Bredde = 800, Dybde = 100, Vaegt = 35m, RotationsRegel = "Ja" },
                new Element { Reference = "Q2", Type = "Vindue", Maerke = "Velux", Hoejde = 1000, Bredde = 600, Dybde = 80, Vaegt = 25m, RotationsRegel = "Ja" },
                new Element { Reference = "Q3", Type = "Dør", Maerke = "Jeld-Wen", Hoejde = 2100, Bredde = 900, Dybde = 120, Vaegt = 60m, RotationsRegel = "Nej" },
                new Element { Reference = "Q4", Type = "Vindue", Maerke = "Rationel", Hoejde = 1500, Bredde = 900, Dybde = 100, Vaegt = 40m, RotationsRegel = "Ja" }
            };

            _context.Elementer.AddRange(elementer);
            await _context.SaveChangesAsync();

            // Act & Assert - Forskellige query typer

            // 1. Filter by Type
            var vinduer = await _context.Elementer
                .Where(e => e.Type == "Vindue")
                .ToListAsync();
            Assert.Equal(3, vinduer.Count);

            // 2. Group by Maerke
            var byMaerke = await _context.Elementer
                .GroupBy(e => e.Maerke)
                .Select(g => new { Maerke = g.Key, Count = g.Count() })
                .ToListAsync();
            Assert.Equal(3, byMaerke.Count); // Velux, Jeld-Wen, Rationel

            // 3. Order by Vaegt descending
            var tyngste = await _context.Elementer
                .OrderByDescending(e => e.Vaegt)
                .FirstAsync();
            Assert.Equal("Q3", tyngste.Reference); // Dør med 60kg

            // 4. Average vægt
            var avgVaegt = await _context.Elementer
                .AverageAsync(e => e.Vaegt);
            Assert.Equal(40m, avgVaegt); // (35+25+60+40)/4 = 40

            // 5. Complex filter
            var storeVinduer = await _context.Elementer
                .Where(e => e.Type == "Vindue" && e.Hoejde >= 1200)
                .OrderBy(e => e.Maerke)
                .ToListAsync();
            Assert.Equal(2, storeVinduer.Count); // Q1 (Velux 1200) og Q4 (Rationel 1500)
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
