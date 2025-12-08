using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Tests.UnitTests
{
    /// <summary>
    /// Unit Tests for PalleService klassen
    /// Reference: docs/testplan.md - Section 4.1 Unit Test
    /// Reference: docs/klassediagram.md - PalleService implementation
    /// </summary>
    public class PalleServiceTests
    {
        /// <summary>
        /// Test: PalleService.GetAllePaller()
        /// Formål: Verificer at alle paller returneres korrekt fra databasen
        /// Reference: Paller tabel i ER-diagram (docs/er-diagram.md)
        /// </summary>
        [Fact]
        public async Task GetAllePaller_ShouldReturnAllPaller()
        {
            // Arrange - Opretter in-memory database med test data
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAllePaller")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                // Tilføj test data - 3 paller som i seed data
                context.Paller.AddRange(
                    new Palle { Id = 1, PalleBeskrivelse = "Træpalle 75'er", Laengde = 750, Bredde = 1000, Aktiv = true },
                    new Palle { Id = 2, PalleBeskrivelse = "Træpalle 80'er", Laengde = 800, Bredde = 1200, Aktiv = true },
                    new Palle { Id = 3, PalleBeskrivelse = "Træpalle 100'er", Laengde = 1000, Bredde = 1200, Aktiv = false }
                );
                await context.SaveChangesAsync();
            }

            // Act - Kald service metoden
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.GetAllePaller();

                // Assert - Verificer resultatet
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
            }
        }

        /// <summary>
        /// Test: PalleService.GetAlleAktivePaller()
        /// Formål: Verificer at kun aktive paller returneres (Aktiv = true)
        /// Reference: Paller.Aktiv kolonne i ER-diagram
        /// </summary>
        [Fact]
        public async Task GetAlleAktivePaller_ShouldReturnOnlyActivePaller()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAktivePaller")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.AddRange(
                    new Palle { Id = 1, PalleBeskrivelse = "Aktiv palle 1", Aktiv = true },
                    new Palle { Id = 2, PalleBeskrivelse = "Aktiv palle 2", Aktiv = true },
                    new Palle { Id = 3, PalleBeskrivelse = "Inaktiv palle", Aktiv = false }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.GetAlleAktivePaller();

                // Assert - Kun 2 aktive paller skal returneres
                Assert.Equal(2, result.Count());
                Assert.All(result, p => Assert.True(p.Aktiv));
            }
        }

        /// <summary>
        /// Test: PalleService.GetPalle(id) - Valid ID
        /// Formål: Verificer at enkelt palle kan hentes med valid ID
        /// </summary>
        [Fact]
        public async Task GetPalle_WithValidId_ShouldReturnPalle()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetPalle_Valid")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle { Id = 1, PalleBeskrivelse = "Test Palle", Laengde = 800 });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.GetPalle(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Test Palle", result.PalleBeskrivelse);
                Assert.Equal(800, result.Laengde);
            }
        }

        /// <summary>
        /// Test: PalleService.GetPalle(id) - Invalid ID
        /// Formål: Verificer at null returneres når ID ikke findes
        /// </summary>
        [Fact]
        public async Task GetPalle_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetPalle_Invalid")
                .Options;

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.GetPalle(999); // ID findes ikke

                // Assert
                Assert.Null(result);
            }
        }

        /// <summary>
        /// Test: PalleService.OpretPalle(palle) - Valid Data
        /// Formål: Verificer at ny palle kan oprettes med valid data
        /// Reference: Paller tabel constraints (NOT NULL, decimal precision)
        /// </summary>
        [Fact]
        public async Task OpretPalle_WithValidData_ShouldCreatePalle()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_OpretPalle_Valid")
                .Options;

            var newPalle = new Palle
            {
                PalleBeskrivelse = "Ny Test Palle",
                Laengde = 1000,
                Bredde = 1200,
                Hoejde = 150,
                Pallegruppe = "Standard",
                Palletype = "Træ",
                Vaegt = 25.5m, // DECIMAL(18,2)
                MaksHoejde = 2000,
                MaksVaegt = 500m,
                Overmaal = 50,
                LuftMellemElementer = 10,
                Aktiv = true,
                Sortering = 1
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.OpretPalle(newPalle);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Id > 0); // Auto-generated ID
                Assert.Equal("Ny Test Palle", result.PalleBeskrivelse);
            }

            // Verify data er gemt i database
            using (var context = new PalleOptimeringContext(options))
            {
                var savedPalle = await context.Paller.FindAsync(newPalle.Id);
                Assert.NotNull(savedPalle);
            }
        }

        /// <summary>
        /// Test: PalleService.OpdaterPalle(palle)
        /// Formål: Verificer at eksisterende palle kan opdateres
        /// </summary>
        [Fact]
        public async Task OpdaterPalle_ShouldUpdateExistingPalle()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_OpdaterPalle")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle
                {
                    Id = 1,
                    PalleBeskrivelse = "Original",
                    Laengde = 800,
                    Aktiv = true
                });
                await context.SaveChangesAsync();
            }

            // Act - Opdater beskrivelse og aktiv status
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var palle = await context.Paller.FindAsync(1);
                palle.PalleBeskrivelse = "Opdateret";
                palle.Aktiv = false;

                var result = await service.OpdaterPalle(palle);

                // Assert
                Assert.Equal("Opdateret", result.PalleBeskrivelse);
                Assert.False(result.Aktiv);
            }
        }

        /// <summary>
        /// Test: PalleService.SletPalle(id)
        /// Formål: Verificer at palle kan slettes
        /// </summary>
        [Fact]
        public async Task SletPalle_ShouldDeletePalle()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SletPalle")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle { Id = 1, PalleBeskrivelse = "Slet mig" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var result = await service.SletPalle(1);

                // Assert
                Assert.True(result);
            }

            // Verify palle er slettet
            using (var context = new PalleOptimeringContext(options))
            {
                var deletedPalle = await context.Paller.FindAsync(1);
                Assert.Null(deletedPalle);
            }
        }
    }

    #region Mock Classes (Erstatter rigtige klasser indtil de er implementeret)

    // Disse mock klasser repræsenterer den faktiske kode fra klassediagrammet
    // De skal erstattes med de rigtige klasser når projektet er fuldt implementeret

    public class PalleOptimeringContext : DbContext
    {
        public PalleOptimeringContext(DbContextOptions<PalleOptimeringContext> options) : base(options) { }
        public DbSet<Palle> Paller { get; set; }
        public DbSet<Element> Elementer { get; set; }
    }

    public class Palle
    {
        public int Id { get; set; }
        public string PalleBeskrivelse { get; set; } = "";
        public int Laengde { get; set; }
        public int Bredde { get; set; }
        public int Hoejde { get; set; }
        public string Pallegruppe { get; set; } = "";
        public string Palletype { get; set; } = "";
        public decimal Vaegt { get; set; }
        public int MaksHoejde { get; set; }
        public decimal MaksVaegt { get; set; }
        public int Overmaal { get; set; }
        public int LuftMellemElementer { get; set; }
        public bool Aktiv { get; set; }
        public int Sortering { get; set; }
    }

    public class Element
    {
        public int Id { get; set; }
        public string Reference { get; set; } = "";
        public string Type { get; set; } = "";
        public string Serie { get; set; } = "";
        public int Hoejde { get; set; }
        public int Bredde { get; set; }
        public int Dybde { get; set; }
        public decimal Vaegt { get; set; }
        public bool ErSpecialelement { get; set; }
        public bool ErGeometrielement { get; set; }
        public string RotationsRegel { get; set; } = "Nej";
        public string? KraeverPalletype { get; set; }
        public int? MaksElementerPrPalle { get; set; }
    }

    public interface IPalleService
    {
        Task<IEnumerable<Palle>> GetAllePaller();
        Task<IEnumerable<Palle>> GetAlleAktivePaller();
        Task<Palle?> GetPalle(int id);
        Task<Palle> OpretPalle(Palle palle);
        Task<Palle> OpdaterPalle(Palle palle);
        Task<bool> SletPalle(int id);
    }

    public class PalleService : IPalleService
    {
        private readonly PalleOptimeringContext _context;

        public PalleService(PalleOptimeringContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Palle>> GetAllePaller()
        {
            return await _context.Paller.ToListAsync();
        }

        public async Task<IEnumerable<Palle>> GetAlleAktivePaller()
        {
            return await _context.Paller.Where(p => p.Aktiv).ToListAsync();
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
            _context.Paller.Update(palle);
            await _context.SaveChangesAsync();
            return palle;
        }

        public async Task<bool> SletPalle(int id)
        {
            var palle = await _context.Paller.FindAsync(id);
            if (palle == null) return false;

            _context.Paller.Remove(palle);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    #endregion
}
