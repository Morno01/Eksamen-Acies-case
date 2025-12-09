using MyProject.Models;

namespace MyProject.Services
{
    /// <summary>
    /// Helper klasse til at håndtere placering af elementer på paller
    /// </summary>
    public class ElementPlaceringHelper
    {
        private readonly PalleOptimeringSettings _settings;

        public ElementPlaceringHelper(PalleOptimeringSettings settings)
        {
            _settings = settings;
        }

        public bool KanPlaceresPaaPalle(ElementMedData elementData, PakkeplanPalle pakkeplanPalle, Palle palle)
        {
            var element = elementData.Element;

            // Tjek palletype hvis krævet
            if (!string.IsNullOrEmpty(element.KraeverPalletype) &&
                element.KraeverPalletype != palle.Palletype)
                return false;

            // Tjek maks elementer pr palle
            if (element.MaksElementerPrPalle.HasValue)
            {
                int antalPaaDennePalle = pakkeplanPalle.Elementer.Count(e => e.ElementId == element.Id);
                if (antalPaaDennePalle >= element.MaksElementerPrPalle.Value)
                    return false;
            }

            // Tjek vægt
            decimal nyVaegt = pakkeplanPalle.SamletVaegt + element.Vaegt;
            if (nyVaegt > palle.MaksVaegt)
                return false;

            // Bestem om element skal roteres
            bool skalRoteres = SkalElementRoteres(element, pakkeplanPalle, palle);

            int elementHoejde = element.Hoejde;
            int elementBredde = element.Bredde;

            if (skalRoteres)
            {
                // Swap dimensioner
                (elementHoejde, elementBredde) = (elementBredde, elementHoejde);
            }

            // Tjek dimensioner
            if (elementHoejde > palle.Laengde + palle.Overmaal)
                return false;

            if (elementBredde > palle.Bredde + palle.Overmaal)
                return false;

            // Tjek højde
            int nyHoejde = pakkeplanPalle.SamletHoejde + element.Hoejde;
            if (nyHoejde > palle.MaksHoejde)
                return false;

            // Tjek stabling regler
            if (!KanStables(element, pakkeplanPalle, palle))
                return false;

            return true;
        }

        public void PlacerElement(ElementMedData elementData, PakkeplanPalle pakkeplanPalle)
        {
            var element = elementData.Element;
            var palle = pakkeplanPalle.Palle;

            // Bestem rotation
            bool erRoteret = SkalElementRoteres(element, pakkeplanPalle, palle);

            // Bestem lag
            int lag = BeregnLag(element, pakkeplanPalle);

            // Bestem plads (1-5)
            int plads = BeregnNaestePlads(pakkeplanPalle, lag);

            // Opret pakkeplan element
            var pakkeplanElement = new PakkeplanElement
            {
                PakkeplanPalleId = pakkeplanPalle.Id,
                PakkeplanPalle = pakkeplanPalle,
                ElementId = element.Id,
                Element = element,
                Lag = lag,
                Plads = plads,
                ErRoteret = erRoteret,
                Sortering = pakkeplanPalle.Elementer.Count + 1
            };

            pakkeplanPalle.Elementer.Add(pakkeplanElement);

            // Opdater palle statistikker
            pakkeplanPalle.SamletVaegt += element.Vaegt;
            pakkeplanPalle.SamletHoejde += element.Hoejde;

            if (lag > pakkeplanPalle.AntalLag)
                pakkeplanPalle.AntalLag = lag;
        }

        private bool SkalElementRoteres(Element element, PakkeplanPalle pakkeplanPalle, Palle palle)
        {
            // Tjek rotationsregel på element
            if (element.RotationsRegel == "Nej")
                return false;

            if (element.RotationsRegel == "Skal")
                return true;

            // Rotationsregel er "Ja" - bestem automatisk

            // Tjek vægt regel
            if (element.Vaegt > _settings.TilladVendeOpTilMaksKg)
            {
                // Tung - roter ikke med mindre makshøjde overskrides
                int hoejdeUdenRotation = pakkeplanPalle.SamletHoejde + element.Hoejde;
                if (hoejdeUdenRotation > palle.MaksHoejde)
                    return true; // Roter alligevel
                return false;
            }

            // Tjek højde/bredde faktor
            decimal faktor1 = (decimal)element.Hoejde / element.Bredde;
            decimal faktor2 = (decimal)element.Bredde / element.Hoejde;
            decimal mindsteFaktor = Math.Min(faktor1, faktor2);

            if (mindsteFaktor < _settings.HoejdeBreddefaktor)
            {
                // Element er meget langt på ét led
                if (_settings.HoejdeBreddefaktorKunForEnkeltElementer)
                {
                    // Kun hvis det er første element
                    if (pakkeplanPalle.Elementer.Count == 0)
                        return true; // Læg ned
                }
                else
                {
                    return true; // Læg altid ned
                }
            }

            // Standard: placer på korteste side (roter hvis bredde > højde)
            if (element.Bredde > element.Hoejde)
                return true;

            // Tjek om rotation ville hjælpe stabling
            if (pakkeplanPalle.Elementer.Any())
            {
                var langsteElement = pakkeplanPalle.Elementer
                    .Select(e => e.ErRoteret ? e.Element.Bredde : e.Element.Hoejde)
                    .Max();

                // Roter hvis det ikke bliver længere end længste element
                if (element.Bredde <= langsteElement && element.Bredde <= palle.Laengde + palle.Overmaal)
                    return true;
            }

            return false;
        }

        private bool KanStables(Element element, PakkeplanPalle pakkeplanPalle, Palle palle)
        {
            // Ingen stabling hvis der ikke er elementer på pallen
            if (!pakkeplanPalle.Elementer.Any())
                return true;

            int aktuelLag = BeregnLag(element, pakkeplanPalle);

            // Tjek maks lag
            if (aktuelLag > _settings.MaksLag)
                return false;

            // Tjek samlet højde efter stabling
            int nyHoejde = pakkeplanPalle.SamletHoejde + element.Hoejde;
            if (_settings.TilladStablingOpTilMaksHoejdeInklPalle.HasValue &&
                nyHoejde > _settings.TilladStablingOpTilMaksHoejdeInklPalle.Value)
                return false;

            // Tjek element vægt for stabling
            if (aktuelLag > 1 && _settings.TilladStablingOpTilMaksElementVaegt.HasValue)
            {
                if (element.Vaegt > _settings.TilladStablingOpTilMaksElementVaegt.Value)
                    return false;
            }

            // Tjek om der er geometri-elementer i laget under
            if (aktuelLag > 1)
            {
                var elementerILagUnder = pakkeplanPalle.Elementer
                    .Where(e => e.Lag == aktuelLag - 1)
                    .Select(e => e.Element);

                if (elementerILagUnder.Any(e => e.ErGeometrielement))
                    return false; // Må ikke stable ovenpå geometri-elementer
            }

            return true;
        }

        private int BeregnLag(Element element, PakkeplanPalle pakkeplanPalle)
        {
            if (!pakkeplanPalle.Elementer.Any())
                return 1;

            // Find hvilket lag vi er på baseret på højde
            int aktuelLag = pakkeplanPalle.AntalLag;

            // Tjek om vi kan være i nuværende lag eller skal starte nyt
            var elementerIAktuelLag = pakkeplanPalle.Elementer
                .Where(e => e.Lag == aktuelLag)
                .ToList();

            // Hvis der er plads i nuværende lag (max 5 elementer)
            if (elementerIAktuelLag.Count < 5)
                return aktuelLag;

            // Ellers start nyt lag
            return aktuelLag + 1;
        }

        private int BeregnNaestePlads(PakkeplanPalle pakkeplanPalle, int lag)
        {
            var elementerILag = pakkeplanPalle.Elementer
                .Where(e => e.Lag == lag)
                .OrderBy(e => e.Plads)
                .ToList();

            if (!elementerILag.Any())
                return 1;

            // Standard rækkefølge: 1,5,2,4,3 (hvis PlacerLaengsteElementerYderst)
            // Ellers: 1,2,3,4,5
            if (_settings.PlacerLaengsteElementerYderst)
            {
                int[] raekkefoelge = { 1, 5, 2, 4, 3 };
                foreach (var plads in raekkefoelge)
                {
                    if (!elementerILag.Any(e => e.Plads == plads))
                        return plads;
                }
            }
            else
            {
                for (int plads = 1; plads <= 5; plads++)
                {
                    if (!elementerILag.Any(e => e.Plads == plads))
                        return plads;
                }
            }

            return elementerILag.Count + 1;
        }
    }
}
