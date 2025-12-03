using MyProject.Models;
using MyProject.Services;
using Xunit;

namespace MyProject.Tests.Services
{
    public class ElementSorteringHelperTests
    {
        private List<Element> GetTestElementer()
        {
            return new List<Element>
            {
                new Element
                {
                    Id = 1,
                    Maerke = "A",
                    Serie = "S1",
                    Hoejde = 2000,
                    Bredde = 800,
                    Dybde = 100,
                    Vaegt = 50m,
                    ErSpecialelement = false
                },
                new Element
                {
                    Id = 2,
                    Maerke = "A",
                    Serie = "S2",
                    Hoejde = 1800,
                    Bredde = 700,
                    Dybde = 100,
                    Vaegt = 45m,
                    ErSpecialelement = true
                },
                new Element
                {
                    Id = 3,
                    Maerke = "B",
                    Serie = "S1",
                    Hoejde = 2200,
                    Bredde = 900,
                    Dybde = 100,
                    Vaegt = 60m,
                    ErSpecialelement = false
                }
            };
        }

        [Fact]
        public void SorterElementer_SortererEfterMaerke()
        {
            // Arrange
            var settings = new PalleOptimeringSettings
            {
                SorteringsPrioritering = "Maerke"
            };
            var helper = new ElementSorteringHelper(settings);
            var elementer = GetTestElementer();

            // Act
            var sorteret = helper.SorterElementer(elementer);

            // Assert
            Assert.Equal("A", sorteret[0].Element.Maerke);
            Assert.Equal("A", sorteret[1].Element.Maerke);
            Assert.Equal("B", sorteret[2].Element.Maerke);
        }

        [Fact]
        public void SorterElementer_SortererEfterSpecialelement()
        {
            // Arrange
            var settings = new PalleOptimeringSettings
            {
                SorteringsPrioritering = "Specialelement"
            };
            var helper = new ElementSorteringHelper(settings);
            var elementer = GetTestElementer();

            // Act
            var sorteret = helper.SorterElementer(elementer);

            // Assert
            Assert.True(sorteret[0].Element.ErSpecialelement);
            Assert.False(sorteret[1].Element.ErSpecialelement);
            Assert.False(sorteret[2].Element.ErSpecialelement);
        }

        [Fact]
        public void SorterElementer_SortererEfterElementstorrelse()
        {
            // Arrange
            var settings = new PalleOptimeringSettings
            {
                SorteringsPrioritering = "Elementstorrelse"
            };
            var helper = new ElementSorteringHelper(settings);
            var elementer = GetTestElementer();

            // Act
            var sorteret = helper.SorterElementer(elementer);

            // Assert
            // Størst først (2200 * 900 = 1980000)
            Assert.Equal(3, sorteret[0].Element.Id);
            // Derefter (2000 * 800 = 1600000)
            Assert.Equal(1, sorteret[1].Element.Id);
            // Mindst (1800 * 700 = 1260000)
            Assert.Equal(2, sorteret[2].Element.Id);
        }

        [Fact]
        public void SorterElementer_SortererEfterVaegt()
        {
            // Arrange
            var settings = new PalleOptimeringSettings
            {
                SorteringsPrioritering = "Vaegt"
            };
            var helper = new ElementSorteringHelper(settings);
            var elementer = GetTestElementer();

            // Act
            var sorteret = helper.SorterElementer(elementer);

            // Assert
            Assert.Equal(60m, sorteret[0].Element.Vaegt);
            Assert.Equal(50m, sorteret[1].Element.Vaegt);
            Assert.Equal(45m, sorteret[2].Element.Vaegt);
        }

        [Fact]
        public void SorterElementer_SortererEfterFlerePrioriteter()
        {
            // Arrange
            var settings = new PalleOptimeringSettings
            {
                SorteringsPrioritering = "Maerke,Serie"
            };
            var helper = new ElementSorteringHelper(settings);
            var elementer = GetTestElementer();

            // Act
            var sorteret = helper.SorterElementer(elementer);

            // Assert
            // Først mærke A, serie S1
            Assert.Equal("A", sorteret[0].Element.Maerke);
            Assert.Equal("S1", sorteret[0].Element.Serie);
            // Så mærke A, serie S2
            Assert.Equal("A", sorteret[1].Element.Maerke);
            Assert.Equal("S2", sorteret[1].Element.Serie);
            // Sidst mærke B
            Assert.Equal("B", sorteret[2].Element.Maerke);
        }
    }
}
