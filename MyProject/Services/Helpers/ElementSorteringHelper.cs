using MyProject.Models;

namespace MyProject.Services
{
    /// <summary>
    /// Helper klasse til at sortere elementer efter optimerings kriterier
    /// </summary>
    public class ElementSorteringHelper
    {
        private readonly PalleOptimeringSettings _settings;

        public ElementSorteringHelper(PalleOptimeringSettings settings)
        {
            _settings = settings;
        }

        public List<ElementMedData> SorterElementer(List<Element> elementer)
        {
           
            var elementerMedData = elementer.Select(e => new ElementMedData(e)).ToList();

           
            var prioriteter = _settings.SorteringsPrioritering
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

           
            IOrderedEnumerable<ElementMedData>? sorteret = null;

            foreach (var prioritet in prioriteter)
            {
                sorteret = prioritet.ToLower() switch
                {
                    "type" => sorteret == null
                        ? elementerMedData.OrderBy(e => e.Element.Type ?? string.Empty)
                        : sorteret.ThenBy(e => e.Element.Type ?? string.Empty),

                    "specialelement" => sorteret == null
                        ? elementerMedData.OrderByDescending(e => e.Element.ErSpecialelement)
                        : sorteret.ThenByDescending(e => e.Element.ErSpecialelement),

                    "pallestorrelse" => sorteret == null
                        ? elementerMedData.OrderBy(e => e.MinPalleId)
                        : sorteret.ThenBy(e => e.MinPalleId),

                    "elementstorrelse" => sorteret == null
                        ? elementerMedData.OrderByDescending(e => e.Element.Hoejde * e.Element.Bredde)
                        : sorteret.ThenByDescending(e => e.Element.Hoejde * e.Element.Bredde),

                    "vaegt" => sorteret == null
                        ? elementerMedData.OrderByDescending(e => e.Element.Vaegt)
                        : sorteret.ThenByDescending(e => e.Element.Vaegt),

                    "serie" => sorteret == null
                        ? elementerMedData.OrderBy(e => e.Element.Serie ?? string.Empty)
                        : sorteret.ThenBy(e => e.Element.Serie ?? string.Empty),

                    _ => sorteret
                };
            }

            return sorteret?.ToList() ?? elementerMedData;
        }
    }

    /// <summary>
    /// Wrapper klasse til at holde element og ekstra data under optimering
    /// </summary>
    public class ElementMedData
    {
        public Element Element { get; set; }
        public int? MinPalleId { get; set; }

        public ElementMedData(Element element)
        {
            Element = element;
        }
    }
}
