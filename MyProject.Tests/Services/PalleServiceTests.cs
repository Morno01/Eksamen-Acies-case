using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Services
{
    public class PalleServiceTests
    {
        private PalleOptimeringContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PalleOptimeringContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new PalleOptimeringContext(options);

            // Seed test data
            context.Paller.AddRange(
                new Palle
                {
                    Id = 1,
                    PalleBeskrivelse = "Test Palle 1",
                    Laengde = 2400,
                    Bredde = 750,
                    Hoejde = 150,
                    Palletype = "Trae",
                    Vaegt = 25m,
                    MaksHoejde = 2800,
                    MaksVaegt = 1000m,
                    Aktiv = true,
                    Sortering = 1
                },
                new Palle
                {
                    Id = 2,
                    PalleBeskrivelse = "Test Palle 2",
                    Laengde = 2400,
                    Bredde = 800,
                    Hoejde = 150,
                    Palletype = "Trae",
                    Vaegt = 27m,
                    MaksHoejde = 2800,
                    MaksVaegt = 1200m,
                    Aktiv = false,
                    Sortering = 2
                }
            );

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAlleAktivePaller_ReturnererKunAktivePaller()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            // Act
            var resultat = await service.GetAlleAktivePaller();

            // Assert
            Assert.Single(resultat);
            Assert.All(resultat, p => Assert.True(p.Aktiv));
        }

        [Fact]
        public async Task GetAllePaller_ReturnererAllePaller()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            // Act
            var resultat = await service.GetAllePaller();

            // Assert
            Assert.Equal(2, resultat.Count());
        }

        [Fact]
        public async Task GetPalle_MedGyldigId_ReturnererPalle()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            // Act
            var resultat = await service.GetPalle(1);

            // Assert
            Assert.NotNull(resultat);
            Assert.Equal("Test Palle 1", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task GetPalle_MedUgyldigId_ReturnererNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            // Act
            var resultat = await service.GetPalle(999);

            // Assert
            Assert.Null(resultat);
        }

        [Fact]
        public async Task OpretPalle_TilfojerNyPalle()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);
            var nyPalle = new Palle
            {
                PalleBeskrivelse = "Ny Test Palle",
                Laengde = 2400,
                Bredde = 1000,
                Hoejde = 150,
                Palletype = "Alu",
                Vaegt = 30m,
                MaksHoejde = 2800,
                MaksVaegt = 1500m,
                Aktiv = true,
                Sortering = 3
            };

            // Act
            var resultat = await service.OpretPalle(nyPalle);

            // Assert
            Assert.NotNull(resultat);
            Assert.True(resultat.Id > 0);
            Assert.Equal("Ny Test Palle", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task OpdaterPalle_OpdatererEksisterendePalle()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);
            var palle = await service.GetPalle(1);
            palle!.PalleBeskrivelse = "Opdateret Beskrivelse";

            // Act
            var resultat = await service.OpdaterPalle(palle);

            // Assert
            Assert.Equal("Opdateret Beskrivelse", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task SletPalle_FjernerPalle()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            // Act
            var resultat = await service.SletPalle(1);
            var slettetPalle = await service.GetPalle(1);

            // Assert
            Assert.True(resultat);
            Assert.Null(slettetPalle);
        }
    }
}
