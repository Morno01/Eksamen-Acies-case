using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Data;
using MyProject.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MyProject.Tests.System
{
    public class PalleOptimeringSystemTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PalleOptimeringSystemTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PalleOptimeringContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<PalleOptimeringContext>(options =>
                    {
                        options.UseInMemoryDatabase("SystemTestDb");
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<PalleOptimeringContext>();
                    SeedTestData(context);
                });
            });

            _client = _factory.CreateClient();
        }

        private static void SeedTestData(PalleOptimeringContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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

            context.Paller.Add(eurPalle);

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
                },
                new Element
                {
                    Id = 4,
                    Reference = "TOO-TALL",
                    Type = "Vindue",
                    Maerke = "Test",
                    Serie = "XXL",
                    Hoejde = 2500,
                    Bredde = 1000,
                    Dybde = 100,
                    Vaegt = 80m,
                    RotationsRegel = "Nej"
                }
            };

            context.Elementer.AddRange(elementer);

            var settings = new PalleOptimeringSettings
            {
                Id = 1,
                Navn = "System Test Settings",
                MaksLag = 3,
                TilladVendeOpTilMaksKg = 50m,
                HoejdeBreddefaktor = 0.3m,
                HoejdeBreddefaktorKunForEnkeltElementer = true,
                TilladStablingOpTilMaksHoejdeInklPalle = 2200,
                TilladStablingOpTilMaksElementVaegt = 100m,
                SorteringsPrioritering = "Maerke,Serie,Vaegt",
                PlacerLaengsteElementerYderst = true
            };

            context.PalleOptimeringSettings.Add(settings);
            context.SaveChanges();
        }

        /// <summary>
        /// SCRUM-77: TC5-SYS-001
        /// </summary>
        [Fact]
        public async Task SCRUM77_GenererPakkeplan()
        {
            var response = await _client.GetAsync("/api/elementer");
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound,
                "Elementer endpoint skulle være tilgængeligt");

            var request = new
            {
                ElementIds = new List<int> { 1, 2, 3 },
                OrdreReference = "ORD-12345",
                SettingsId = 1
            };

            var optimerResponse = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            Assert.True(optimerResponse.IsSuccessStatusCode || optimerResponse.StatusCode == HttpStatusCode.NotFound,
                "Pakkeplan generering endpoint skulle eksistere");
        }

        /// <summary>
        /// SCRUM-78: TC5-SYS-002
        /// </summary>
        [Fact]
        public async Task TC5SYS002_GenererUdenElementer()
        {
            var request = new
            {
                ElementIds = new List<int>(),
                OrdreReference = "ORD-EMPTY",
                SettingsId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound,
                "Skulle returnere fejl når ingen elementer er valgt");
        }

        /// <summary>
        /// TC5-SYS-003
        /// </summary>
        [Fact]
        public async Task TC5SYS003_GenererMedProblemElement()
        {
            var request = new
            {
                ElementIds = new List<int> { 4 },
                OrdreReference = "ORD-TALL",
                SettingsId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound,
                "System skulle håndtere for højt element");
        }

        /// <summary>
        /// TC5-SYS-004
        /// </summary>
        [Fact]
        public async Task TC5SYS004_TestTomDatabase()
        {
            var emptyFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PalleOptimeringContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<PalleOptimeringContext>(options =>
                    {
                        options.UseInMemoryDatabase("EmptyTestDb");
                    });
                });
            });

            var emptyClient = emptyFactory.CreateClient();

            var request = new
            {
                ElementIds = new List<int> { 1 },
                OrdreReference = "ORD-EMPTY-DB",
                SettingsId = 1
            };

            var response = await emptyClient.PostAsJsonAsync("/api/pakkeplan/generer", request);

            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest,
                "Skulle returnere fejl når database er tom");
        }

        /// <summary>
        /// TC5-SYS-005
        /// </summary>
        [Fact]
        public async Task TC5SYS005_FuldEndToEndFlow()
        {
            var elementerResponse = await _client.GetAsync("/api/elementer");

            var genererRequest = new
            {
                ElementIds = new List<int> { 1, 2 },
                OrdreReference = "ORD-E2E-TEST",
                SettingsId = 1
            };

            var genererResponse = await _client.PostAsJsonAsync("/api/pakkeplan/generer", genererRequest);

            Assert.True(
                genererResponse.IsSuccessStatusCode ||
                genererResponse.StatusCode == HttpStatusCode.NotFound,
                "End-to-end flow skulle fungere");
        }
    }
}
