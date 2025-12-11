using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Services
{
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

        [Fact]
        public void SCRUM58_BeregningAfPallehoejde_MedEnkeltVindue()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();

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

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde, // Initial: kun pallehøjde
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            Assert.Equal(150, pakkeplanPalle.SamletHoejde);

            helper.PlacerElement(vindue, pakkeplanPalle);

            int forventetHoejde = 150 + 1200;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
            Assert.Equal(1350, pakkeplanPalle.SamletHoejde);

            Assert.Single(pakkeplanPalle.Elementer);
            Assert.Equal("VIND-001", pakkeplanPalle.Elementer.First().Element.Reference);
        }

        [Fact]
        public void TC1UT002_StablingAfToElementer()
        {
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

            helper.PlacerElement(vindue1, pakkeplanPalle);
            var hoejdeEfterForsteElement = pakkeplanPalle.SamletHoejde;

            helper.PlacerElement(vindue2, pakkeplanPalle);
            var hoejdeEfterAndetElement = pakkeplanPalle.SamletHoejde;

            Assert.Equal(750, hoejdeEfterForsteElement);

            Assert.Equal(1350, hoejdeEfterAndetElement);

            Assert.Equal(2, pakkeplanPalle.Elementer.Count);
        }

        [Fact]
        public void TC1UT003_ElementOverskrederMaksimalHoejde()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();

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

            bool kanPlaceres = helper.KanPlaceresPaaPalle(hoejDor, pakkeplanPalle, palle);

            Assert.False(kanPlaceres);

            Assert.Empty(pakkeplanPalle.Elementer);
            Assert.Equal(150, pakkeplanPalle.SamletHoejde); // Uændret
        }

        [Fact]
        public void TC1UT004_TomPalleInitialisering()
        {
            var palle = GetEURPalle();

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            Assert.Equal(150, pakkeplanPalle.SamletHoejde); // Kun pallehøjde
            Assert.Equal(25m, pakkeplanPalle.SamletVaegt);  // Kun pallevægt
            Assert.Equal(1, pakkeplanPalle.AntalLag);
            Assert.Empty(pakkeplanPalle.Elementer);         // Ingen elementer
        }

        [Fact]
        public void TC1UT005_MaksimalKapacitetBeregning()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle();

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

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

            helper.PlacerElement(element1, pakkeplanPalle);
            int hoejdeEfterElement1 = pakkeplanPalle.SamletHoejde;

            helper.PlacerElement(element2, pakkeplanPalle);
            int hoejdeEfterElement2 = pakkeplanPalle.SamletHoejde;

            Assert.Equal(1150, hoejdeEfterElement1);

            Assert.Equal(2150, hoejdeEfterElement2);

            Assert.Equal(2, pakkeplanPalle.Elementer.Count);

            Assert.True(pakkeplanPalle.SamletHoejde <= palle.MaksHoejde,
                $"SamletHoejde ({pakkeplanPalle.SamletHoejde}mm) <= MaksHoejde ({palle.MaksHoejde}mm)");
        }
        [Fact]
        public void SCRUM74_TungtElementRoteresIkke()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m; // Maks 50kg må vendes
            var helper = new ElementPlaceringHelper(settings);

            var palle = GetEURPalle();
            palle.MaksHoejde = 2800; // Nok plads til element uden rotation

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

            helper.PlacerElement(tungtElement, pakkeplanPalle);

            var placeretElement = pakkeplanPalle.Elementer.First();
            Assert.False(placeretElement.ErRoteret, "Tungt element (75kg > 50kg) skal IKKE roteres");

            int forventetHoejde = 150 + 2000;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
            Assert.Equal(2150, pakkeplanPalle.SamletHoejde);

            Assert.True(pakkeplanPalle.SamletHoejde <= palle.MaksHoejde,
                "Højde check: 2150mm < 2800mm (OK uden rotation)");
        }

        [Fact]
        public void TEST_FlereElementerPerLagBasereretPaaBredde()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetEURPalle(); // Længde = 1200mm, Bredde = 800mm, Overmaal = 50mm

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde,
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            var door1 = new ElementMedData(new Element
            {
                Id = 1,
                Reference = "DØR-001",
                Type = "Dør",
                Serie = "Premium",
                Hoejde = 2100,
                Bredde = 400, // Smal dør
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Nej" // Må ikke roteres
            });

            var door2 = new ElementMedData(new Element
            {
                Id = 2,
                Reference = "DØR-002",
                Type = "Dør",
                Serie = "Premium",
                Hoejde = 2100,
                Bredde = 400,
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Nej"
            });

            var door3 = new ElementMedData(new Element
            {
                Id = 3,
                Reference = "DØR-003",
                Type = "Dør",
                Serie = "Premium",
                Hoejde = 2100,
                Bredde = 400,
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Nej"
            });

            helper.PlacerElement(door1, pakkeplanPalle);
            helper.PlacerElement(door2, pakkeplanPalle);
            helper.PlacerElement(door3, pakkeplanPalle);

            Assert.Equal(3, pakkeplanPalle.Elementer.Count);
            Assert.All(pakkeplanPalle.Elementer, e => Assert.Equal(1, e.Lag));

            var brugtBredde = pakkeplanPalle.Elementer
                .Where(e => e.Lag == 1)
                .Sum(e => e.ErRoteret ? e.Element.Hoejde : e.Element.Bredde);

            Assert.Equal(1200, brugtBredde); // 3 × 400 = 1200
            Assert.True(brugtBredde <= palle.Laengde + palle.Overmaal,
                $"Brugt bredde {brugtBredde}mm skal passe i tilgængelig bredde {palle.Laengde + palle.Overmaal}mm");

            int forventetHoejde = 150 + 2100;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);

        }

        [Fact]
        public void TEST_DenneFejlerMedVilje()
        {
            int forventet = 100;
            int faktisk = 200;

            Assert.Equal(forventet, faktisk); // ❌ FEJLER: 100 != 200

        }
    }
}
