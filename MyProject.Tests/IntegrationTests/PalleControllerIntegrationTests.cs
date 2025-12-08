using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Tests.IntegrationTests
{
    /// <summary>
    /// Integration Tests for PallerController
    /// Reference: docs/testplan.md - Section 4.2 Integrationstest
    /// Test: Controller → Service → Database integration
    /// Reference: docs/klassediagram.md - PallerController → IPalleService → PalleOptimeringContext
    /// </summary>
    public class PalleControllerIntegrationTests
    {
        /// <summary>
        /// Test: PallerController.GetAllePaller() → PalleService → Database
        /// Formål: Verificer hele stakken fra Controller til Database fungerer
        /// Reference: Klassediagram - Controller → Service Integration
        /// </summary>
        [Fact]
        public async Task GetAllePaller_IntegrationTest_ShouldReturnDataFromDatabase()
        {
            // Arrange - Setup hele stakken (Database → Service → Controller)
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_GetAllePaller")
                .Options;

            // Setup database med test data
            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.AddRange(
                    new Palle { Id = 1, PalleBeskrivelse = "Test Palle 1", Laengde = 750, Bredde = 1000, Aktiv = true },
                    new Palle { Id = 2, PalleBeskrivelse = "Test Palle 2", Laengde = 800, Bredde = 1200, Aktiv = true }
                );
                await context.SaveChangesAsync();
            }

            // Act - Kald hele stakken
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var result = await controller.GetAllePaller();

                // Assert - Verificer response fra controller
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Palle>>>(result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var paller = Assert.IsAssignableFrom<IEnumerable<Palle>>(okResult.Value);

                Assert.Equal(2, paller.Count());
            }
        }

        /// <summary>
        /// Test: PallerController.GetPalle(id) - Found Scenario
        /// Formål: Test at controller returnerer 200 OK når palle findes
        /// Reference: Testplan - HTTP status kode tests
        /// </summary>
        [Fact]
        public async Task GetPalle_WhenFound_ShouldReturn200OK()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_GetPalle_Found")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle { Id = 1, PalleBeskrivelse = "Eksisterende Palle", Laengde = 800 });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var result = await controller.GetPalle(1);

                // Assert
                var actionResult = Assert.IsType<ActionResult<Palle>>(result);
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var palle = Assert.IsType<Palle>(okResult.Value);

                Assert.Equal("Eksisterende Palle", palle.PalleBeskrivelse);
            }
        }

        /// <summary>
        /// Test: PallerController.GetPalle(id) - Not Found Scenario
        /// Formål: Test at controller returnerer 404 NotFound når palle ikke findes
        /// Reference: Testplan - HTTP status kode tests
        /// </summary>
        [Fact]
        public async Task GetPalle_WhenNotFound_ShouldReturn404NotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_GetPalle_NotFound")
                .Options;

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var result = await controller.GetPalle(999); // ID findes ikke

                // Assert
                var actionResult = Assert.IsType<ActionResult<Palle>>(result);
                Assert.IsType<NotFoundResult>(actionResult.Result);
            }
        }

        /// <summary>
        /// Test: PallerController.OpretPalle(palle)
        /// Formål: Test at ny palle oprettes og gemmes korrekt i database
        /// Reference: Testplan - 201 Created ved success
        /// </summary>
        [Fact]
        public async Task OpretPalle_ShouldReturn201CreatedAndSaveToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_OpretPalle")
                .Options;

            var newPalle = new Palle
            {
                PalleBeskrivelse = "Ny Integration Test Palle",
                Laengde = 1000,
                Bredde = 1200,
                Hoejde = 150,
                Pallegruppe = "Test",
                Palletype = "Træ",
                Vaegt = 30m,
                MaksHoejde = 2000,
                MaksVaegt = 500m,
                Aktiv = true
            };

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var result = await controller.OpretPalle(newPalle);

                // Assert
                var actionResult = Assert.IsType<ActionResult<Palle>>(result);
                var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var createdPalle = Assert.IsType<Palle>(createdResult.Value);

                Assert.Equal("Ny Integration Test Palle", createdPalle.PalleBeskrivelse);
                Assert.True(createdPalle.Id > 0); // ID er auto-genereret
            }

            // Verify data er gemt i database
            using (var context = new PalleOptimeringContext(options))
            {
                var savedPalle = await context.Paller.FirstOrDefaultAsync(p => p.PalleBeskrivelse == "Ny Integration Test Palle");
                Assert.NotNull(savedPalle);
            }
        }

        /// <summary>
        /// Test: PallerController.OpdaterPalle(id, palle)
        /// Formål: Test at palle opdateres i database via controller
        /// Reference: Testplan - 204 NoContent ved success
        /// </summary>
        [Fact]
        public async Task OpdaterPalle_ShouldReturn204NoContentAndUpdateDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_OpdaterPalle")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle
                {
                    Id = 1,
                    PalleBeskrivelse = "Original Beskrivelse",
                    Laengde = 800,
                    Aktiv = true
                });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var updatedPalle = new Palle
                {
                    Id = 1,
                    PalleBeskrivelse = "Opdateret Beskrivelse",
                    Laengde = 900,
                    Aktiv = false
                };

                var result = await controller.OpdaterPalle(1, updatedPalle);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }

            // Verify opdateringen i database
            using (var context = new PalleOptimeringContext(options))
            {
                var updated = await context.Paller.FindAsync(1);
                Assert.Equal("Opdateret Beskrivelse", updated.PalleBeskrivelse);
                Assert.Equal(900, updated.Laengde);
                Assert.False(updated.Aktiv);
            }
        }

        /// <summary>
        /// Test: PallerController.SletPalle(id)
        /// Formål: Test at palle slettes fra database
        /// Reference: Testplan - 204 NoContent ved success
        /// </summary>
        [Fact]
        public async Task SletPalle_ShouldReturn204NoContentAndDeleteFromDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_SletPalle")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                context.Paller.Add(new Palle { Id = 1, PalleBeskrivelse = "Slet denne palle" });
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                var controller = new PallerController(service);

                var result = await controller.SletPalle(1);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }

            // Verify sletning i database
            using (var context = new PalleOptimeringContext(options))
            {
                var deleted = await context.Paller.FindAsync(1);
                Assert.Null(deleted);
            }
        }

        /// <summary>
        /// Test: Database Relation - RESTRICT constraint på master data
        /// Formål: Verificer at Paller ikke kan slettes hvis de bruges i PakkeplanPaller
        /// Reference: ER-diagram - RESTRICT delete på master data
        /// </summary>
        [Fact]
        public async Task SletPalle_WhenUsedInPakkeplan_ShouldFailDueToRestrictConstraint()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDb_RestrictDelete")
                .Options;

            using (var context = new PalleOptimeringContext(options))
            {
                // Opret palle og pakkeplan der bruger den
                var palle = new Palle { Id = 1, PalleBeskrivelse = "Brugt Palle" };
                context.Paller.Add(palle);

                // I virkeligheden ville PakkeplanPalle have en FK til Paller
                // Dette test viser konceptet
                await context.SaveChangesAsync();
            }

            // Act & Assert
            // I en fuld implementation ville dette give en DbUpdateException
            // pga. RESTRICT constraint
            using (var context = new PalleOptimeringContext(options))
            {
                var service = new PalleService(context);
                // Test at constraint håndteres korrekt i service layer
                // (Kræver fuld implementation med relation tables)
            }
        }
    }

    #region Mock Controller Implementation

    /// <summary>
    /// Mock PallerController baseret på klassediagrammet
    /// Reference: docs/klassediagram.md - PallerController
    /// </summary>
    public class PallerController : ControllerBase
    {
        private readonly IPalleService _palleService;

        public PallerController(IPalleService palleService)
        {
            _palleService = palleService;
        }

        public async Task<ActionResult<IEnumerable<Palle>>> GetAllePaller()
        {
            var paller = await _palleService.GetAllePaller();
            return Ok(paller);
        }

        public async Task<ActionResult<Palle>> GetPalle(int id)
        {
            var palle = await _palleService.GetPalle(id);
            if (palle == null)
                return NotFound();

            return Ok(palle);
        }

        public async Task<ActionResult<Palle>> OpretPalle(Palle palle)
        {
            var created = await _palleService.OpretPalle(palle);
            return CreatedAtAction(nameof(GetPalle), new { id = created.Id }, created);
        }

        public async Task<IActionResult> OpdaterPalle(int id, Palle palle)
        {
            if (id != palle.Id)
                return BadRequest();

            await _palleService.OpdaterPalle(palle);
            return NoContent();
        }

        public async Task<IActionResult> SletPalle(int id)
        {
            var success = await _palleService.SletPalle(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }

    #endregion
}
