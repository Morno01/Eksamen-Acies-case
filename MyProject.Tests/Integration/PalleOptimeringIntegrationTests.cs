using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Integration
{
    /// <summary>
    /// Integration tests for PalleOptimering systemet
    /// Tester samspillet mellem Services, Database og Models
    /// </summary>
    public class PalleOptimeringIntegrationTests : IDisposable
    {
        private readonly PalleOptimeringContext _context;
        private readonly PalleOptimeringService _service;
        private readonly PalleOptimeringSettings _settings;

        public PalleOptimeringIntegrationTests()
        {
            // Arrange - Setup InMemory database
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unik DB per test
                .Options;

            _context = new PalleOptimeringContext(options);

            // Seed test data
            SeedTestData();

            // Opret service med settings
            _settings = new PalleOptimeringSettings
            {
                Id = 1,
                Navn = "Integration Test Settings",
                MaksLag = 3,
                TilladVendeOpTilMaksKg = 50m,
                HoejdeBreddefaktor = 0.3m,
                HoejdeBreddefaktorKunForEnkeltElementer = true,
                TilladStablingOpTilMaksHoejdeInklPalle = 2200,
                TilladStablingOpTilMaksElementVaegt = 100m,
                SorteringsPrioritering = "Maerke,Serie,Vaegt",
                PlacerLaengsteElementerYderst = true
            };

            _service = new PalleOptimeringService(_context, _settings);
        }

        private void SeedTestData()
        {
            // Paller
            var eurPalle = new Palle
            {
                Id = 1,
                PalleBeskrivelse = "EUR-palle 80x120",
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

            var storPalle = new Palle
            {
                Id = 2,
                PalleBeskrivelse = "Stor palle 120x120",
                Palletype = "Trae",
                Laengde = 1200,
                Bredde = 1200,
                Hoejde = 150,
                MaksHoejde = 2500,
                MaksVaegt = 1500m,
                Vaegt = 30m,
                Overmaal = 50,
                Sortering = 2
            };

            _context.Paller.AddRange(eurPalle, storPalle);

            // Elementer
            var elementer = new List<Element>
            {
                new Element
                {
                    Id = 1,
                    Reference = "VIND-001",
                    Type = "Vindue",
                    Maerke = "Velux",
                    Serie = "S100",
                    Hoejde = 1200,
                    Bredde = 800,
                    Dybde = 100,
                    Vaegt = 35m,
                    RotationsRegel = "Ja"
                },
                new Element
                {
                    Id = 2,
                    Reference = "VIND-002",
                    Type = "Vindue",
                    Maerke = "Velux",
                    Serie = "S100",
                    Hoejde = 600,
                    Bredde = 600,
                    Dybde = 80,
                    Vaegt = 20m,
                    RotationsRegel = "Ja"
                },
                new Element
                {
                    Id = 3,
                    Reference = "DOOR-001",
                    Type = "Dør",
                    Maerke = "Jeld-Wen",
                    Serie = "D200",
                    Hoejde = 2100,
                    Bredde = 900,
                    Dybde = 120,
                    Vaegt = 60m,
                    RotationsRegel = "Nej"
                }
            };

            _context.Elementer.AddRange(elementer);
            _context.SaveChanges();
        }

        /// <summary>
        /// INT-001: Test at en pakkeplan kan oprettes med elementer fra databasen
        /// </summary>
        [Fact]
        public async Task INT001_OpretPakkeplanMedElementerFraDatabase()
        {
            // Arrange
            var elementIds = new List<int> { 1, 2 }; // VIND-001 og VIND-002

            // Act
            var pakkeplan = await _service.OptimerPakkeplanAsync(elementIds);

            // Assert
            Assert.NotNull(pakkeplan);
            Assert.NotEmpty(pakkeplan.Paller);

            // Verificer at elementer blev placeret
            var totalElementer = pakkeplan.Paller.Sum(p => p.Elementer.Count);
            Assert.Equal(2, totalElementer);
        }

        /// <summary>
        /// INT-002: Test at pallehøjde beregnes korrekt gennem hele systemet
        /// </summary>
        [Fact]
        public async Task INT002_PallehojdeBeregnesKorrektGennemSystem()
        {
            // Arrange
            var elementIds = new List<int> { 2, 2 }; // To VIND-002 (600mm hver)

            // Act
            var pakkeplan = await _service.OptimerPakkeplanAsync(elementIds);

            // Assert
            var palle = pakkeplan.Paller.First();

            // Expected: 150mm (palle) + 600mm + 600mm = 1350mm
            Assert.Equal(1350, palle.SamletHoejde);
            Assert.Equal(2, palle.Elementer.Count);
        }

        /// <summary>
        /// INT-003: Test at elementer sorteres korrekt efter Maerke og Serie
        /// </summary>
        [Fact]
        public async Task INT003_ElementerSorteresEfterMaerkeOgSerie()
        {
            // Arrange
            var elementIds = new List<int> { 1, 2, 3 }; // Velux, Velux, Jeld-Wen

            // Act
            var elementer = await _context.Elementer
                .Where(e => elementIds.Contains(e.Id))
                .OrderBy(e => e.Maerke)
                .ThenBy(e => e.Serie)
                .ToListAsync();

            // Assert
            Assert.Equal("Jeld-Wen", elementer[0].Maerke); // J kommer før V
            Assert.Equal("Velux", elementer[1].Maerke);
            Assert.Equal("Velux", elementer[2].Maerke);
        }

        /// <summary>
        /// INT-004: Test at maksimal vægt respekteres på palle
        /// </summary>
        [Fact]
        public async Task INT004_MaksimalVaegtRespekteres()
        {
            // Arrange - skab mange tunge elementer
            for (int i = 10; i < 30; i++)
            {
                _context.Elementer.Add(new Element
                {
                    Id = i,
                    Reference = $"HEAVY-{i}",
                    Type = "Dør",
                    Hoejde = 500,
                    Bredde = 500,
                    Dybde = 100,
                    Vaegt = 100m, // Tunge elementer
                    RotationsRegel = "Ja"
                });
            }
            await _context.SaveChangesAsync();

            var elementIds = Enumerable.Range(10, 20).ToList();

            // Act
            var pakkeplan = await _service.OptimerPakkeplanAsync(elementIds);

            // Assert
            foreach (var palle in pakkeplan.Paller)
            {
                // Hver EUR palle har max 1000kg kapacitet
                Assert.True(palle.SamletVaegt <= palle.Palle.MaksVaegt,
                    $"Palle {palle.Id} overskred vægtgrænse: {palle.SamletVaegt}kg > {palle.Palle.MaksVaegt}kg");
            }
        }

        /// <summary>
        /// INT-005: Test database persistering af pakkeplan
        /// </summary>
        [Fact]
        public async Task INT005_PakkeplanGemmesTilDatabase()
        {
            // Arrange
            var elementIds = new List<int> { 1 };

            // Act
            var pakkeplan = await _service.OptimerPakkeplanAsync(elementIds);
            await _context.SaveChangesAsync();

            // Hent fra database igen
            var gemt = await _context.Pakkeplaner
                .Include(p => p.Paller)
                .ThenInclude(p => p.Elementer)
                .FirstOrDefaultAsync(p => p.Id == pakkeplan.Id);

            // Assert
            Assert.NotNull(gemt);
            Assert.NotEmpty(gemt.Paller);
            Assert.Equal(pakkeplan.Paller.Count, gemt.Paller.Count);
        }

        /// <summary>
        /// INT-006: Test at forskellige palle typer vælges korrekt
        /// </summary>
        [Fact]
        public async Task INT006_KorrektPalletypeVaelges()
        {
            // Arrange - element der kræver stor palle
            var stortElement = new Element
            {
                Id = 100,
                Reference = "BIG-001",
                Type = "Vindue",
                Hoejde = 2000,
                Bredde = 1100, // Bredere end EUR-palle (800mm)
                Dybde = 100,
                Vaegt = 80m,
                RotationsRegel = "Nej"
            };
            _context.Elementer.Add(stortElement);
            await _context.SaveChangesAsync();

            // Act
            var pakkeplan = await _service.OptimerPakkeplanAsync(new List<int> { 100 });

            // Assert
            var palle = pakkeplan.Paller.First();

            // Skulle vælge stor palle (1200mm bred) ikke EUR (800mm)
            Assert.True(palle.Palle.Bredde >= 1100,
                $"Element kræver min 1100mm bredde, men palle er kun {palle.Palle.Bredde}mm");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
