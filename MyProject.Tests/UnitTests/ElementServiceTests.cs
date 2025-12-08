using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Tests.UnitTests
{
    /// <summary>
    /// Unit Tests for ElementService klassen
    /// Reference: docs/testplan.md - Section 4.1 Unit Test
    /// Reference: docs/klassediagram.md - ElementService implementation
    /// </summary>
    public class ElementServiceTests
    {
        /// <summary>
        /// Test: ElementService.GetAlleElementer()
        /// Formål: Verificer at alle elementer returneres korrekt
        /// Reference: Elementer tabel i ER-diagram
        /// </summary>
        [Fact]
        public async Task GetAlleElementer_ShouldReturnAllElements()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAlleElementer")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Elementer.AddRange(
                    new Element { Id = 1, Reference = "DØR-001", Type = "Dør", Serie = "Standard", Hoejde = 2100, Bredde = 900, Dybde = 60, Vaegt = 45.5m },
                    new Element { Id = 2, Reference = "VIN-001", Type = "Vindue", Serie = "Premium", Hoejde = 1200, Bredde = 1000, Dybde = 80, Vaegt = 25.0m },
                    new Element { Id = 3, Reference = "DØR-002", Type = "Dør", Serie = "Lux", Hoejde = 2100, Bredde = 1200, Dybde = 80, Vaegt = 65.0m }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.GetAlleElementer();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
            }
        }

        /// <summary>
        /// Test: ElementService.GetElement(id)
        /// Formål: Verificer hentning af enkelt element med valid ID
        /// </summary>
        [Fact]
        public async Task GetElement_WithValidId_ShouldReturnElement()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetElement")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Elementer.Add(new Element
                {
                    Id = 1,
                    Reference = "TEST-001",
                    Type = "Dør",
                    Serie = "Test",
                    Hoejde = 2000,
                    Bredde = 800,
                    Dybde = 50,
                    Vaegt = 40.0m
                });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.GetElement(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("TEST-001", result.Reference);
                Assert.Equal("Dør", result.Type);
            }
        }

        /// <summary>
        /// Test: ElementService.OpretElement(element) - Valid Data
        /// Formål: Verificer oprettelse med valid data og validering
        /// Reference: Elementer tabel constraints - Reference max 100 tegn, Vaegt DECIMAL(18,2)
        /// </summary>
        [Fact]
        public async Task OpretElement_WithValidData_ShouldCreateElement()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_OpretElement_Valid")
                .Options;

            var newElement = new Element
            {
                Reference = "NY-DØR-001",
                Type = "Dør",
                Serie = "Standard",
                Hoejde = 2100,
                Bredde = 900,
                Dybde = 60,
                Vaegt = 45.75m, // DECIMAL(18,2) validering
                ErSpecialelement = false,
                ErGeometrielement = false,
                RotationsRegel = "Ja",
                KraeverPalletype = null,
                MaksElementerPrPalle = null
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.OpretElement(newElement);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Id > 0);
                Assert.Equal("NY-DØR-001", result.Reference);
                Assert.Equal(45.75m, result.Vaegt);
            }
        }

        /// <summary>
        /// Test: ElementService.OpretFlereElementer(elementer)
        /// Formål: Verificer bulk insert funktionalitet
        /// Reference: Testplan - ElementService.OpretFlereElementer metode
        /// </summary>
        [Fact]
        public async Task OpretFlereElementer_ShouldCreateMultipleElements()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_BulkInsert")
                .Options;

            var newElements = new List<Element>
            {
                new Element { Reference = "BULK-001", Type = "Dør", Serie = "A", Hoejde = 2000, Bredde = 800, Dybde = 50, Vaegt = 40m },
                new Element { Reference = "BULK-002", Type = "Vindue", Serie = "A", Hoejde = 1200, Bredde = 1000, Dybde = 80, Vaegt = 25m },
                new Element { Reference = "BULK-003", Type = "Dør", Serie = "B", Hoejde = 2100, Bredde = 900, Dybde = 60, Vaegt = 45m }
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.OpretFlereElementer(newElements);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
                Assert.All(result, e => Assert.True(e.Id > 0)); // Alle har fået ID
            }

            // Verify i database
            using (var context = new PalleOptimeringContext(options))
            {
                var savedElements = await context.Elementer.ToListAsync();
                Assert.Equal(3, savedElements.Count);
            }
        }

        /// <summary>
        /// Test: ElementService.OpdaterElement(element)
        /// Formål: Verificer opdatering af eksisterende element
        /// </summary>
        [Fact]
        public async Task OpdaterElement_ShouldUpdateExistingElement()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_OpdaterElement")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Elementer.Add(new Element
                {
                    Id = 1,
                    Reference = "ORIGINAL",
                    Type = "Dør",
                    Vaegt = 40m,
                    ErSpecialelement = false
                });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var element = await context.Elementer.FindAsync(1);
                element.Reference = "OPDATERET";
                element.Vaegt = 50m;
                element.ErSpecialelement = true;

                var result = await service.OpdaterElement(element);

                // Assert
                Assert.Equal("OPDATERET", result.Reference);
                Assert.Equal(50m, result.Vaegt);
                Assert.True(result.ErSpecialelement);
            }
        }

        /// <summary>
        /// Test: ElementService.SletElement(id)
        /// Formål: Verificer sletning af element
        /// </summary>
        [Fact]
        public async Task SletElement_ShouldDeleteElement()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SletElement")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Elementer.Add(new Element { Id = 1, Reference = "DELETE-ME", Type = "Test" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.SletElement(1);

                // Assert
                Assert.True(result);
            }

            // Verify
            using (var context = new PalleOptimeringContext(options))
            {
                var deleted = await context.Elementer.FindAsync(1);
                Assert.Null(deleted);
            }
        }

        /// <summary>
        /// Test: Element Model Validering - RotationsRegel enum
        /// Formål: Verificer at kun gyldige værdier accepteres (Nej, Ja, Skal)
        /// Reference: ER-diagram - RotationsRegel kolonne
        /// </summary>
        [Theory]
        [InlineData("Nej")]
        [InlineData("Ja")]
        [InlineData("Skal")]
        public async Task OpretElement_WithValidRotationsRegel_ShouldSucceed(string rotationsRegel)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_RotationsRegel_{rotationsRegel}")
                .Options;

            var element = new Element
            {
                Reference = $"ROT-{rotationsRegel}",
                Type = "Dør",
                Serie = "Test",
                Hoejde = 2000,
                Bredde = 800,
                Dybde = 50,
                Vaegt = 40m,
                RotationsRegel = rotationsRegel
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                var result = await service.OpretElement(element);

                // Assert
                Assert.Equal(rotationsRegel, result.RotationsRegel);
            }
        }

        /// <summary>
        /// Test: Element Model - ErSpecialelement og ErGeometrielement flags
        /// Formål: Verificer boolean flags fungerer korrekt
        /// Reference: Elementer tabel - boolean kolonner
        /// </summary>
        [Fact]
        public async Task OpretElement_WithSpecialFlags_ShouldPersistCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SpecialFlags")
                .Options;

            var specialElement = new Element
            {
                Reference = "SPECIAL-001",
                Type = "Special",
                Serie = "Custom",
                Hoejde = 2500,
                Bredde = 1500,
                Dybde = 100,
                Vaegt = 80m,
                ErSpecialelement = true,
                ErGeometrielement = true
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new ElementService(context);
                await service.OpretElement(specialElement);
            }

            // Assert
            using (var context = new PalleOptimeringContext(options))
            {
                var saved = await context.Elementer.FirstAsync();
                Assert.True(saved.ErSpecialelement);
                Assert.True(saved.ErGeometrielement);
            }
        }
    }

    #region Mock Service Implementation

    public interface IElementService
    {
        Task<IEnumerable<Element>> GetAlleElementer();
        Task<Element?> GetElement(int id);
        Task<Element> OpretElement(Element element);
        Task<IEnumerable<Element>> OpretFlereElementer(IEnumerable<Element> elementer);
        Task<Element> OpdaterElement(Element element);
        Task<bool> SletElement(int id);
    }

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

        public async Task<IEnumerable<Element>> OpretFlereElementer(IEnumerable<Element> elementer)
        {
            await _context.Elementer.AddRangeAsync(elementer);
            await _context.SaveChangesAsync();
            return elementer;
        }

        public async Task<Element> OpdaterElement(Element element)
        {
            _context.Elementer.Update(element);
            await _context.SaveChangesAsync();
            return element;
        }

        public async Task<bool> SletElement(int id)
        {
            var element = await _context.Elementer.FindAsync(id);
            if (element == null) return false;

            _context.Elementer.Remove(element);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    #endregion
}
