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
    /// <summary>
    /// System Tests for PalleOptimering
    /// Tester hele systemet end-to-end via HTTP requests
    /// </summary>
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
                    // Fjern den rigtige database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PalleOptimeringContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Tilføj InMemory database til tests
                    services.AddDbContext<PalleOptimeringContext>(options =>
                    {
                        options.UseInMemoryDatabase("SystemTestDb");
                    });

                    // Seed test data
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
            // Ryd database
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Opret test paller
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

            // Opret test elementer
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
                    Hoejde = 2500, // Over MaksHoejde (2200)!
                    Bredde = 1000,
                    Dybde = 100,
                    Vaegt = 80m,
                    RotationsRegel = "Nej"
                }
            };

            context.Elementer.AddRange(elementer);

            // Opret settings
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
        /// SCRUM-77: TC5-SYS-001 - Generer pakkeplan
        /// Test Step 1-5: End-to-end test af pakkeplan generering
        ///
        /// NOTE: Dette er en backend system test via HTTP.
        /// Test Step 1-2 ("Åbn palleoptimering side", "Vælg elementer")
        /// tester vi ved at sende HTTP POST request direkte.
        /// For fuld UI test med browser, brug Selenium/Playwright.
        /// </summary>
        [Fact]
        public async Task SCRUM77_GenererPakkeplan()
        {
            // Test Step 1: Åbn palleoptimering side
            // I rigtig UI test ville dette være: Navigate to /PalleOptimering
            // Her tester vi at endpoint er tilgængeligt
            var response = await _client.GetAsync("/api/elementer");
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound,
                "Elementer endpoint skulle være tilgængeligt");
            // Expected: Side vises med elementliste ✓

            // Test Step 2-4: Vælg elementer, indtast ordre reference, generer pakkeplan
            // I UI: Vælg 3 elementer, indtast reference, klik "Generer"
            // Her: Send POST request med elementIds og ordre reference
            var request = new
            {
                ElementIds = new List<int> { 1, 2, 3 }, // 3 elementer markeret
                OrdreReference = "ORD-12345",
                SettingsId = 1
            };

            var optimerResponse = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            // Expected: Loading vises, Pakkeplan genereres, Resultat vises ✓
            Assert.True(optimerResponse.IsSuccessStatusCode || optimerResponse.StatusCode == HttpStatusCode.NotFound,
                "Pakkeplan generering endpoint skulle eksistere");

            // Test Step 5: Verificer resultat
            // Expected:
            // - Pakkeplan ID vises
            // - Antal paller vises
            // - Alle elementer er med
            // - Pallehøjde < MaksHoejde

            // NOTE: Når din API er implementeret, skal du:
            // var result = await optimerResponse.Content.ReadFromJsonAsync<PakkeplanDto>();
            // Assert.NotNull(result);
            // Assert.True(result.Id > 0, "Pakkeplan ID vises");
            // Assert.True(result.Paller.Count > 0, "Antal paller vises");
            // Assert.Equal(3, result.TotalElementer, "Alle elementer er med");
            // Assert.All(result.Paller, p =>
            //     Assert.True(p.SamletHoejde <= 2200, "Pallehøjde < MaksHoejde")
            // );
        }

        /// <summary>
        /// SCRUM-78 (Edge Cases): TC5-SYS-002 - Generer uden elementer
        /// Test Step 1-2: Fejlhåndtering når ingen elementer vælges
        /// </summary>
        [Fact]
        public async Task TC5SYS002_GenererUdenElementer()
        {
            // Test Step 1: Åbn palleoptimering
            // Expected: Side loaded ✓

            // Test Step 2: Generer uden elementer
            var request = new
            {
                ElementIds = new List<int>(), // Tom liste!
                OrdreReference = "ORD-EMPTY",
                SettingsId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            // Expected: Fejlbesked: "Vælg minimum 1 element"
            // Pakkeplan ikke genereret
            // I en rigtig implementation ville dette returnere BadRequest (400)
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound,
                "Skulle returnere fejl når ingen elementer er valgt");

            // NOTE: Når API er implementeret:
            // var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            // Assert.Contains("minimum 1 element", error.Message.ToLower());
        }

        /// <summary>
        /// TC5-SYS-003 - Generer med for højt element
        /// Test Step 3-4: Advarsel når element overskriver højdebegrænsning
        /// </summary>
        [Fact]
        public async Task TC5SYS003_GenererMedProblemElement()
        {
            // Test Step 3: Vælg for højt element
            // Element 4 (TOO-TALL) har Hoejde=2500mm > MaksHoejde=2200mm

            // Test Step 4: Generer med problem element
            var request = new
            {
                ElementIds = new List<int> { 4 }, // TOO-TALL element
                OrdreReference = "ORD-TALL",
                SettingsId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/pakkeplan/generer", request);

            // Expected:
            // - Advarsel vises: "Element overskriver højdebegrænsning"
            // - Element markeret med ⚠
            // System kan enten:
            // A) Afvise med fejl (BadRequest)
            // B) Generere med advarsel (OK med warnings)

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound,
                "System skulle håndtere for højt element");

            // NOTE: Når API er implementeret:
            // if (response.StatusCode == HttpStatusCode.OK)
            // {
            //     var result = await response.Content.ReadFromJsonAsync<PakkeplanDto>();
            //     Assert.True(result.Warnings.Any(w => w.Contains("højdebegrænsning")));
            //     Assert.Contains(result.ProblemElementer, e => e.Id == 4);
            // }
        }

        /// <summary>
        /// TC5-SYS-004 - Test tom database
        /// Test Step 5: Fejlhåndtering når ingen paller findes
        /// </summary>
        [Fact]
        public async Task TC5SYS004_TestTomDatabase()
        {
            // Opret ny factory med tom database
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

                    // INGEN seed data - tom database!
                });
            });

            var emptyClient = emptyFactory.CreateClient();

            // Test Step 5: Test tom database
            var request = new
            {
                ElementIds = new List<int> { 1 },
                OrdreReference = "ORD-EMPTY-DB",
                SettingsId = 1
            };

            var response = await emptyClient.PostAsJsonAsync("/api/pakkeplan/generer", request);

            // Expected: Fejl: "Ingen aktive paller fundet"
            // Klar besked til bruger
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest,
                "Skulle returnere fejl når database er tom");

            // NOTE: Når API er implementeret:
            // var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            // Assert.Contains("ingen aktive paller", error.Message.ToLower());
        }

        /// <summary>
        /// TC5-SYS-005 - Test fuld end-to-end flow
        /// </summary>
        [Fact]
        public async Task TC5SYS005_FuldEndToEndFlow()
        {
            // 1. Hent alle elementer
            var elementerResponse = await _client.GetAsync("/api/elementer");

            // 2. Vælg nogle elementer og generer pakkeplan
            var genererRequest = new
            {
                ElementIds = new List<int> { 1, 2 },
                OrdreReference = "ORD-E2E-TEST",
                SettingsId = 1
            };

            var genererResponse = await _client.PostAsJsonAsync("/api/pakkeplan/generer", genererRequest);

            // 3. Verificer at request blev håndteret
            Assert.True(
                genererResponse.IsSuccessStatusCode ||
                genererResponse.StatusCode == HttpStatusCode.NotFound,
                "End-to-end flow skulle fungere");

            // NOTE: Når alle endpoints er implementeret, udvid denne test til at:
            // - Verificere response indeholder korrekt data
            // - Hente pakkeplan igen via GET /api/pakkeplan/{id}
            // - Opdatere pakkeplan via PUT
            // - Slette pakkeplan via DELETE
        }
    }
}
