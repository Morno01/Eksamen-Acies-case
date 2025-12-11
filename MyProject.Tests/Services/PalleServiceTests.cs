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
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            var resultat = await service.GetAlleAktivePaller();

            Assert.Single(resultat);
            Assert.All(resultat, p => Assert.True(p.Aktiv));
        }

        [Fact]
        public async Task GetAllePaller_ReturnererAllePaller()
        {
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            var resultat = await service.GetAllePaller();

            Assert.Equal(2, resultat.Count());
        }

        [Fact]
        public async Task GetPalle_MedGyldigId_ReturnererPalle()
        {
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            var resultat = await service.GetPalle(1);

            Assert.NotNull(resultat);
            Assert.Equal("Test Palle 1", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task GetPalle_MedUgyldigId_ReturnererNull()
        {
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            var resultat = await service.GetPalle(999);

            Assert.Null(resultat);
        }

        [Fact]
        public async Task OpretPalle_TilfojerNyPalle()
        {
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

            var resultat = await service.OpretPalle(nyPalle);

            Assert.NotNull(resultat);
            Assert.True(resultat.Id > 0);
            Assert.Equal("Ny Test Palle", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task OpdaterPalle_OpdatererEksisterendePalle()
        {
            var context = GetInMemoryContext();
            var service = new PalleService(context);
            var palle = await service.GetPalle(1);
            palle!.PalleBeskrivelse = "Opdateret Beskrivelse";

            var resultat = await service.OpdaterPalle(palle);

            Assert.Equal("Opdateret Beskrivelse", resultat.PalleBeskrivelse);
        }

        [Fact]
        public async Task SletPalle_FjernerPalle()
        {
            var context = GetInMemoryContext();
            var service = new PalleService(context);

            var resultat = await service.SletPalle(1);
            var slettetPalle = await service.GetPalle(1);

            Assert.True(resultat);
            Assert.Null(slettetPalle);
        }
    }
}
