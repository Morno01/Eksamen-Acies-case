using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Services
{
    /// <summary>
    /// Test cases for TC1-UT - Beregning af pallehøjde
    /// Baseret på SCRUM-58 og relaterede test cases fra Jira
    /// </summary>
    public class PallehojdeBeregningTests
    {
        private PalleOptimeringSettings GetTestSettings()
        {
            return new PalleOptimeringSettings
            {
                Id = 1,
                Navn = "Test Settings",
                MaksLag = 2,
                TilladVendeOpTilMaksKg = 50m,
                HoejdeBreddefaktor = 0.3m,
                HoejdeBreddefaktorKunForEnkeltElementer = true,
                TilladStablingOpTilMaksHoejdeInklPalle = 2200,
                TilladStablingOpTilMaksElementVaegt = 70m,
                PlacerLaengsteElementerYderst = true
            };
        }

        private Palle GetEURPalle()
        {
            return new Palle
            {
                Id = 1,
                PalleBeskrivelse = "EUR-palle 80x120",
                Laengde = 1200,
                Bredde = 800,
                Hoejde = 150, // 15cm = 150mm
                Palletype = "Trae",
                Vaegt = 25m,
                MaksHoejde = 2200,
                MaksVaegt = 1000m,
                Overmaal = 50,
                Sortering = 1
            };
        }

        /// <summary>
        /// SCRUM-58: TC1-UT - Beregning af pallehøjde
        /// Test Step 1-4: Verificer pallehøjde med enkelt element
        /// </summary>
        [Fact]
        public void SCRUM58_BeregningAfPallehoejde_MedEnkeltVindue()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();

            // Test Data: VIND-001
            var vindue = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "VIND-001",
                Type = "Vindue",
                Hoejde = 1200, // mm
                Bredde = 1200, // mm
                Dybde = 100,   // mm
                Vaegt = 35m,   // kg
                RotationsRegel = "Ja"
            });

            // Test Step 1: Initialiser PakkeplanPalle med Palle.Id = 1
            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde, // Initial: kun pallehøjde
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Test Step 2: Verificer initial SamletHoejde = 150 mm
            Assert.Equal(150, pakkeplanPalle.SamletHoejde);

            // Test Step 3: Kald PlacerElement(VIND-001, pakkeplanPalle)
            helper.PlacerElement(vindue, pakkeplanPalle);

            // Test Step 4: Verificer SamletHoejde efter placering
            // Expected Result: 150mm (palle) + 1200mm (vindue) = 1350mm
            int forventetHoejde = 150 + 1200;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
            Assert.Equal(1350, pakkeplanPalle.SamletHoejde);

            // Verificer element er tilføjet
            Assert.Single(pakkeplanPalle.Elementer);
            Assert.Equal("VIND-001", pakkeplanPalle.Elementer.First().Element.Reference);
        }

        /// <summary>
        /// TC1-UT-002: Stabling af to elementer
        /// Baseret på SCRUM-71 (antaget)
        /// </summary>
        [Fact]
        public void TC1UT002_StablingAfToElementer()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();

            var vindue1 = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "VIND-001",
                Hoejde = 600, // mm - lavt vindue så det kan stables
                Bredde = 800,
                Dybde = 80,
                Vaegt = 20m,
                RotationsRegel = "Ja"
            });

            var vindue2 = new ElementMedData(new Element
            {
                Id = 2,
                Reference = "VIND-002",
                Hoejde = 600, // mm
                Bredde = 800,
                Dybde = 80,
                Vaegt = 20m,
                RotationsRegel = "Ja"
            });

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Act
            helper.PlacerElement(vindue1, pakkeplanPalle);
            var hoejdeEfterForsteElement = pakkeplanPalle.SamletHoejde;

            helper.PlacerElement(vindue2, pakkeplanPalle);
            var hoejdeEfterAndetElement = pakkeplanPalle.SamletHoejde;

            // Assert
            // Efter første element: 150 + 600 = 750mm
            Assert.Equal(750, hoejdeEfterForsteElement);

            // Efter andet element: 750 + 600 = 1350mm
            Assert.Equal(1350, hoejdeEfterAndetElement);

            // Verificer begge elementer er placeret
            Assert.Equal(2, pakkeplanPalle.Elementer.Count);
        }

        /// <summary>
        /// TC1-UT-003: Element overskrider maksimal højde
        /// Baseret på SCRUM-72 (antaget)
        /// </summary>
        [Fact]
        public void TC1UT003_ElementOverskrederMaksimalHoejde()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();
            // MaksHoejde = 2200mm

            // Element der er for højt
            var hoejDor = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "DØR-SPECIAL-001",
                Hoejde = 2500, // mm - HØJERE end MaksHoejde
                Bredde = 900,
                Dybde = 100,
                Vaegt = 60m,
                RotationsRegel = "Ja"
            });

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde, // 150mm
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Act
            bool kanPlaceres = helper.KanPlaceresPaaPalle(hoejDor, pakkeplanPalle, palle);

            // Assert
            // 150 + 2500 = 2650mm > 2200mm (MaksHoejde)
            Assert.False(kanPlaceres);

            // Verificer at element IKKE blev placeret
            Assert.Empty(pakkeplanPalle.Elementer);
            Assert.Equal(150, pakkeplanPalle.SamletHoejde); // Uændret
        }

        /// <summary>
        /// TC1-UT-004: Tom palle initialisering
        /// Baseret på SCRUM-73 (antaget)
        /// </summary>
        [Fact]
        public void TC1UT004_TomPalleInitialisering()
        {
            // Arrange
            var palle = GetEURPalle();

            // Act
            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Assert
            Assert.Equal(150, pakkeplanPalle.SamletHoejde); // Kun pallehøjde
            Assert.Equal(25m, pakkeplanPalle.SamletVaegt);  // Kun pallevægt
            Assert.Equal(1, pakkeplanPalle.AntalLag);
            Assert.Empty(pakkeplanPalle.Elementer);         // Ingen elementer
        }

        /// <summary>
        /// TC1-UT-005: Maksimal kapacitet beregning
        /// Test hvor mange elementer kan stables inden MaksHoejde nås
        /// </summary>
        [Fact]
        public void TC1UT005_MaksimalKapacitetBeregning()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();
            // Palle: Hoejde=150mm, MaksHoejde=2200mm
            // Tilgængelig højde: 2200 - 150 = 2050mm

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Elementer med 1000mm højde hver
            int elementHoejde = 1000;

            var element1 = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = elementHoejde,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 30m,
                RotationsRegel = "Ja"
            });

            var element2 = new ElementMedData(new Element
            {
                Id = 2,
                Hoejde = elementHoejde,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 30m,
                RotationsRegel = "Ja"
            });

            var element3 = new ElementMedData(new Element
            {
                Id = 3,
                Hoejde = elementHoejde,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 30m,
                RotationsRegel = "Ja"
            });

            // Act & Assert
            // Element 1: 150 + 1000 = 1150mm ✓
            bool kanPlacereElement1 = helper.KanPlaceresPaaPalle(element1, pakkeplanPalle, palle);
            Assert.True(kanPlacereElement1);
            helper.PlacerElement(element1, pakkeplanPalle);
            Assert.Equal(1150, pakkeplanPalle.SamletHoejde);

            // Element 2: 1150 + 1000 = 2150mm ✓ (< 2200)
            bool kanPlacereElement2 = helper.KanPlaceresPaaPalle(element2, pakkeplanPalle, palle);
            Assert.True(kanPlacereElement2);
            helper.PlacerElement(element2, pakkeplanPalle);
            Assert.Equal(2150, pakkeplanPalle.SamletHoejde);

            // Element 3: 2150 + 1000 = 3150mm ✗ (> 2200)
            bool kanPlacereElement3 = helper.KanPlaceresPaaPalle(element3, pakkeplanPalle, palle);
            Assert.False(kanPlacereElement3);

            // Maksimum: 2 elementer kan placeres
            Assert.Equal(2, pakkeplanPalle.Elementer.Count);
        }

        /// <summary>
        /// SCRUM-72: TC1-UT-003 - Stabling overskriver maksimal højde
        /// Test Step 1-3: Element allerede placeret, forsøg at placere et til
        /// </summary>
        [Fact]
        public void SCRUM72_StablingOverskreiderMaksimalHoejde()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();
            // MaksHoejde = 2200mm

            // Test Data: VIND-001 (Hoejde=1200mm)
            var vindue1 = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "VIND-001",
                Type = "Vindue",
                Hoejde = 1200, // mm
                Bredde = 1200, // mm
                Dybde = 100,   // mm
                Vaegt = 35m,   // kg
                RotationsRegel = "Ja"
            });

            var vindue2 = new ElementMedData(new Element
            {
                Id = 2,
                Reference = "VIND-001",
                Type = "Vindue",
                Hoejde = 1200, // mm
                Bredde = 1200, // mm
                Dybde = 100,   // mm
                Vaegt = 35m,   // kg
                RotationsRegel = "Ja"
            });

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Test Step 1: Pre-condition - placer første VIND-001
            helper.PlacerElement(vindue1, pakkeplanPalle);

            // Verificer nuværende SamletHoejde = 1350 mm
            Assert.Equal(1350, pakkeplanPalle.SamletHoejde);

            // Test Step 2-3: Forsøg at placere andet VIND-001
            // 1350 + 1200 = 2550mm > 2200mm (MaksHoejde)
            bool kanPlacereAndetElement = helper.KanPlaceresPaaPalle(vindue2, pakkeplanPalle, palle);

            // Expected Result: Andet element afvises
            Assert.False(kanPlacereAndetElement, "Andet VIND-001 element skal afvises da 1350+1200=2550 > 2200");

            // Verificer at kun første element er placeret
            Assert.Single(pakkeplanPalle.Elementer);
            Assert.Equal(1350, pakkeplanPalle.SamletHoejde); // Uændret
        }

        /// <summary>
        /// SCRUM-74: TC2-UT - Tungt element roteres ikke
        /// Test Step 1-5: Verificer at tunge elementer ikke roteres automatisk
        /// </summary>
        [Fact]
        public void SCRUM74_TungtElementRoteresIkke()
        {
            // Test Step 1: Konfigurer settings: TilladVendeOpTilMaksKg = 50kg
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m; // Maks 50kg må vendes
            var helper = new ElementPlaceringHelper(settings);

            var palle = GetEURPalle();
            palle.MaksHoejde = 2800; // Nok plads til element uden rotation

            // Test Step 2: Opret element med vægt = 75kg (over grænsen)
            var tungtElement = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "TNG-001",
                Type = "Tungt Element",
                Hoejde = 2000, // mm
                Bredde = 900,  // mm
                Dybde = 100,   // mm
                Vaegt = 75m,   // kg - OVER 50kg grænse
                RotationsRegel = "Ja" // Må roteres, men gør det ikke pga vægt
            });

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde, // 150mm
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Test Step 3: Placer element på palle
            helper.PlacerElement(tungtElement, pakkeplanPalle);

            // Test Step 4: Verificer rotation IKKE foretages
            var placeretElement = pakkeplanPalle.Elementer.First();
            Assert.False(placeretElement.ErRoteret, "Tungt element (75kg > 50kg) skal IKKE roteres");

            // Test Step 5: Bekræft SamletHoejde = 150 + 2000 = 2150mm
            int forventetHoejde = 150 + 2000;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
            Assert.Equal(2150, pakkeplanPalle.SamletHoejde);

            // Verificer at højde check passerer: 2150mm < 2800mm
            Assert.True(pakkeplanPalle.SamletHoejde <= palle.MaksHoejde,
                "Højde check: 2150mm < 2800mm (OK uden rotation)");
        }
    }
}
