using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Services
{
    public class ElementPlaceringHelperTests
    {
        private PalleOptimeringSettings GetTestSettings()
        {
            return new PalleOptimeringSettings
            {
                Id = 1,
                Navn = "Test",
                MaksLag = 2,
                TilladVendeOpTilMaksKg = 50m,
                HoejdeBreddefaktor = 0.3m,
                HoejdeBreddefaktorKunForEnkeltElementer = true,
                TilladStablingOpTilMaksHoejdeInklPalle = 1500,
                TilladStablingOpTilMaksElementVaegt = 70m,
                PlacerLaengsteElementerYderst = true
            };
        }

        private Palle GetTestPalle()
        {
            return new Palle
            {
                Id = 1,
                PalleBeskrivelse = "Test Palle",
                Laengde = 2400,
                Bredde = 800,
                Hoejde = 150,
                Palletype = "Trae",
                Vaegt = 25m,
                MaksHoejde = 2800,
                MaksVaegt = 1000m,
                Overmaal = 50,
                Sortering = 1
            };
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedGyldigElement_ReturnererTrue()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 45m,
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
            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            // Assert
            Assert.True(resultat);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForTungtElement_ReturnererFalse()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 1000m, // For tungt!
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
            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            // Assert
            Assert.False(resultat);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForHoejtElement_ReturnererFalse()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 3000, // For højt!
                Vaegt = 45m,
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
            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            // Assert
            Assert.False(resultat);
        }

        [Fact]
        public void PlacerElement_TilfojerElementTilPalle()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 45m,
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            Assert.Single(pakkeplanPalle.Elementer);
            Assert.Equal(palle.Vaegt + element.Element.Vaegt, pakkeplanPalle.SamletVaegt);
            Assert.Equal(palle.Hoejde + element.Element.Hoejde, pakkeplanPalle.SamletHoejde);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForkertPalletype_ReturnererFalse()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 45m,
                RotationsRegel = "Ja",
                KraeverPalletype = "Alu" // Kræver anden palletype
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
            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            // Assert
            Assert.False(resultat);
        }

        #region Rotation og Vendning Tests

        [Fact]
        public void SkalElementRoteres_MedRotationsRegelNej_ReturnererFalse()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 900,
                Dybde = 100,
                Vaegt = 45m,
                RotationsRegel = "Nej" // Må IKKE roteres
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret, "Element med RotationsRegel='Nej' må ikke roteres");
        }

        [Fact]
        public void SkalElementRoteres_MedRotationsRegelSkal_ReturnererTrue()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 900,
                Dybde = 100,
                Vaegt = 45m,
                RotationsRegel = "Skal" // SKAL roteres
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret, "Element med RotationsRegel='Skal' skal altid roteres");
        }

        [Fact]
        public void SkalElementRoteres_TungtElement_RotererIkke()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m; // Maks 50 kg må vendes
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 900,
                Dybde = 100,
                Vaegt = 75m, // Over 50kg grænse
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret, "Tungt element (>50kg) roteres ikke automatisk");
        }

        [Fact]
        public void SkalElementRoteres_TungtElementMenMaksHoejdeOverskrides_Roterer()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            palle.MaksHoejde = 500; // Lav maksimal højde

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 600, // Højere end MaksHoejde
                Bredde = 200,
                Dybde = 100,
                Vaegt = 75m, // Over 50kg grænse
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret, "Tungt element roteres alligevel når makshøjde overskrides");
        }

        [Fact]
        public void SkalElementRoteres_LetElementMedStandardOrientering_RotererIkke()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000, // Højere end bredde
                Bredde = 900,
                Dybde = 100,
                Vaegt = 30m, // Let element
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret, "Element hvor Hoejde > Bredde placeres uden rotation");
        }

        [Fact]
        public void SkalElementRoteres_LetElementBredereEndHoejt_Roterer()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 900,  // Lavere end bredde
                Bredde = 2000, // Bredere end højde
                Dybde = 100,
                Vaegt = 30m,
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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret, "Element hvor Bredde > Hoejde roteres til korteste side");
        }

        [Fact]
        public void SkalElementRoteres_HoejdeBreddefaktor_RotererSmaltElement()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 100m;
            settings.HoejdeBreddefaktor = 0.3m; // Faktor < 0.3 roteres
            settings.HoejdeBreddefaktorKunForEnkeltElementer = false; // Gælder altid
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 3000, // Meget langt
                Bredde = 600,  // Smalt
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Ja"
            });
            // Faktor = 600/3000 = 0.2 < 0.3 → Skal roteres

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
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret, "Smalt element (faktor 0.2 < 0.3) skal lægges ned");
        }

        [Fact]
        public void SkalElementRoteres_HoejdeBreddefaktorKunForsteElement_IkkeRotererAndretElement()
        {
            // Arrange
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 100m;
            settings.HoejdeBreddefaktor = 0.3m;
            settings.HoejdeBreddefaktorKunForEnkeltElementer = true; // Kun første element
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            // Først element - normalt
            var element1 = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 800,
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Ja"
            });

            // Andet element - smalt (ville normalt roteres)
            var element2 = new ElementMedData(new Element
            {
                Id = 2,
                Hoejde = 3000, // Meget langt
                Bredde = 600,  // Smalt
                Dybde = 100,
                Vaegt = 40m,
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
            helper.PlacerElement(element1, pakkeplanPalle);
            helper.PlacerElement(element2, pakkeplanPalle);

            // Assert
            var placeret2 = pakkeplanPalle.Elementer.Last();
            Assert.False(placeret2.ErRoteret,
                "Andet element roteres ikke når HoejdeBreddefaktorKunForEnkeltElementer=true");
        }

        [Fact]
        public void PlacerElement_BeregnerKorrektPallehoedjeMedRotation()
        {
            // Arrange
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2100, // Dette er højden der tæller når oprejst
                Bredde = 900,
                Dybde = 100,   // Dette er tykkelsen (ikke relevant for højde)
                Vaegt = 45m,
                RotationsRegel = "Ja"
            });

            var pakkeplanPalle = new PakkeplanPalle
            {
                Id = 1,
                PalleId = palle.Id,
                Palle = palle,
                SamletHoejde = palle.Hoejde, // 150 mm
                SamletVaegt = palle.Vaegt,
                AntalLag = 1
            };

            // Act
            helper.PlacerElement(element, pakkeplanPalle);

            // Assert
            int forventetHoejde = palle.Hoejde + element.Element.Hoejde; // 150 + 2100 = 2250
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
            Assert.Equal("Element højde skal lægges til pallehøjde (ikke Dybde)",
                         forventetHoejde.ToString(), pakkeplanPalle.SamletHoejde.ToString());
        }

        #endregion
    }
}
