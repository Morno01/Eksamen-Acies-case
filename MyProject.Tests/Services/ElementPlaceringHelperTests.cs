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
            Assert.Equal(palle.Hoejde + element.Element.Dybde, pakkeplanPalle.SamletHoejde);
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
    }
}
