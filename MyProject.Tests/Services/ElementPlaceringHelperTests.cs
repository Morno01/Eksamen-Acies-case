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

            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            Assert.True(resultat);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForTungtElement_ReturnererFalse()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 100,
                Vaegt = 1000m,
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

            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            Assert.False(resultat);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForHoejtElement_ReturnererFalse()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 700,
                Dybde = 3000,
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

            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            Assert.False(resultat);
        }

        [Fact]
        public void PlacerElement_TilfojerElementTilPalle()
        {
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

            helper.PlacerElement(element, pakkeplanPalle);

            Assert.Single(pakkeplanPalle.Elementer);
            Assert.Equal(palle.Vaegt + element.Element.Vaegt, pakkeplanPalle.SamletVaegt);
            Assert.Equal(palle.Hoejde + element.Element.Hoejde, pakkeplanPalle.SamletHoejde);
        }

        [Fact]
        public void KanPlaceresPaaPalle_MedForkertPalletype_ReturnererFalse()
        {
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
                KraeverPalletype = "Alu"
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

            var resultat = helper.KanPlaceresPaaPalle(element, pakkeplanPalle, palle);

            Assert.False(resultat);
        }

        [Fact]
        public void SkalElementRoteres_MedRotationsRegelNej_ReturnererFalse()
        {
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
                RotationsRegel = "Nej"
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_MedRotationsRegelSkal_ReturnererTrue()
        {
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
                RotationsRegel = "Skal"
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_TungtElement_RotererIkke()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 900,
                Dybde = 100,
                Vaegt = 75m,
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_TungtElementMenMaksHoejdeOverskrides_Roterer()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            palle.MaksHoejde = 500;

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 600,
                Bredde = 200,
                Dybde = 100,
                Vaegt = 75m,
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_LetElementMedStandardOrientering_RotererIkke()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 900,
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.False(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_LetElementBredereEndHoejt_Roterer()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 50m;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();
            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 900,
                Bredde = 2000,
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_HoejdeBreddefaktor_RotererSmaltElement()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 100m;
            settings.HoejdeBreddefaktor = 0.3m;
            settings.HoejdeBreddefaktorKunForEnkeltElementer = false;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 3000,
                Bredde = 600,
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

            helper.PlacerElement(element, pakkeplanPalle);

            var placeret = pakkeplanPalle.Elementer.First();
            Assert.True(placeret.ErRoteret);
        }

        [Fact]
        public void SkalElementRoteres_HoejdeBreddefaktorKunForsteElement_IkkeRotererAndretElement()
        {
            var settings = GetTestSettings();
            settings.TilladVendeOpTilMaksKg = 100m;
            settings.HoejdeBreddefaktor = 0.3m;
            settings.HoejdeBreddefaktorKunForEnkeltElementer = true;
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            var element1 = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2000,
                Bredde = 800,
                Dybde = 100,
                Vaegt = 40m,
                RotationsRegel = "Ja"
            });

            var element2 = new ElementMedData(new Element
            {
                Id = 2,
                Hoejde = 3000,
                Bredde = 600,
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

            helper.PlacerElement(element1, pakkeplanPalle);
            helper.PlacerElement(element2, pakkeplanPalle);

            var placeret2 = pakkeplanPalle.Elementer.Last();
            Assert.False(placeret2.ErRoteret);
        }

        [Fact]
        public void PlacerElement_BeregnerKorrektPallehoedjeMedRotation()
        {
            var settings = GetTestSettings();
            var helper = new ElementPlaceringHelper(settings);
            var palle = GetTestPalle();

            var element = new ElementMedData(new Element
            {
                Id = 1,
                Hoejde = 2100,
                Bredde = 900,
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

            helper.PlacerElement(element, pakkeplanPalle);

            int forventetHoejde = palle.Hoejde + element.Element.Hoejde;
            Assert.Equal(forventetHoejde, pakkeplanPalle.SamletHoejde);
        }
    }
}
