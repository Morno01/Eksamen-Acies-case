using MyProject.Models;

namespace MyProject.Services
{
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

            if (!string.IsNullOrEmpty(element.KraeverPalletype) &&
                element.KraeverPalletype != palle.Palletype)
                return false;

            if (element.MaksElementerPrPalle.HasValue)
            {
                int antalPaaDennePalle = pakkeplanPalle.Elementer.Count(e => e.ElementId == element.Id);
                if (antalPaaDennePalle >= element.MaksElementerPrPalle.Value)
                    return false;
            }

            decimal nyVaegt = pakkeplanPalle.SamletVaegt + element.Vaegt;
            if (nyVaegt > palle.MaksVaegt)
                return false;

            bool skalRoteres = SkalElementRoteres(element, pakkeplanPalle, palle);

            int elementHoejde = element.Hoejde;
            int elementBredde = element.Bredde;

            if (skalRoteres)
            {
                (elementHoejde, elementBredde) = (elementBredde, elementHoejde);
            }

            if (elementHoejde > palle.Laengde + palle.Overmaal)
                return false;

            if (elementBredde > palle.Bredde + palle.Overmaal)
                return false;

            int nyHoejde = pakkeplanPalle.SamletHoejde + element.Dybde;
            if (nyHoejde > palle.MaksHoejde)
                return false;

            if (!KanStables(element, pakkeplanPalle, palle))
                return false;

            return true;
        }

        public void PlacerElement(ElementMedData elementData, PakkeplanPalle pakkeplanPalle)
        {
            var element = elementData.Element;
            var palle = pakkeplanPalle.Palle;

            bool erRoteret = SkalElementRoteres(element, pakkeplanPalle, palle);

            int lag = BeregnLag(element, pakkeplanPalle);

            int plads = BeregnNaestePlads(pakkeplanPalle, lag);

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

            pakkeplanPalle.SamletVaegt += element.Vaegt;
            pakkeplanPalle.SamletHoejde += element.Dybde;

            if (lag > pakkeplanPalle.AntalLag)
                pakkeplanPalle.AntalLag = lag;
        }

        private bool SkalElementRoteres(Element element, PakkeplanPalle pakkeplanPalle, Palle palle)
        {
            if (element.RotationsRegel == "Nej")
                return false;

            if (element.RotationsRegel == "Skal")
                return true;


            if (element.Vaegt > _settings.TilladVendeOpTilMaksKg)
            {
                int hoejdeUdenRotation = pakkeplanPalle.SamletHoejde + element.Dybde;
                if (hoejdeUdenRotation > palle.MaksHoejde)
                    return true; // Roter alligevel
                return false;
            }

            decimal faktor1 = (decimal)element.Hoejde / element.Bredde;
            decimal faktor2 = (decimal)element.Bredde / element.Hoejde;
            decimal mindsteFaktor = Math.Min(faktor1, faktor2);

            if (mindsteFaktor < _settings.HoejdeBreddefaktor)
            {
                if (_settings.HoejdeBreddefaktorKunForEnkeltElementer)
                {
                    if (pakkeplanPalle.Elementer.Count == 0)
                        return true; // Læg ned
                }
                else
                {
                    return true; // Læg altid ned
                }
            }

            if (element.Bredde > element.Hoejde)
                return true;

            if (pakkeplanPalle.Elementer.Any())
            {
                var langsteElement = pakkeplanPalle.Elementer
                    .Select(e => e.ErRoteret ? e.Element.Bredde : e.Element.Hoejde)
                    .Max();

                if (element.Bredde <= langsteElement && element.Bredde <= palle.Laengde + palle.Overmaal)
                    return true;
            }

            return false;
        }

        private bool KanStables(Element element, PakkeplanPalle pakkeplanPalle, Palle palle)
        {
            if (!pakkeplanPalle.Elementer.Any())
                return true;

            int aktuelLag = BeregnLag(element, pakkeplanPalle);

            if (aktuelLag > _settings.MaksLag)
                return false;

            int nyHoejde = pakkeplanPalle.SamletHoejde + element.Dybde;
            if (_settings.TilladStablingOpTilMaksHoejdeInklPalle.HasValue &&
                nyHoejde > _settings.TilladStablingOpTilMaksHoejdeInklPalle.Value)
                return false;

            if (aktuelLag > 1 && _settings.TilladStablingOpTilMaksElementVaegt.HasValue)
            {
                if (element.Vaegt > _settings.TilladStablingOpTilMaksElementVaegt.Value)
                    return false;
            }

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

            int aktuelLag = pakkeplanPalle.AntalLag;

            var elementerIAktuelLag = pakkeplanPalle.Elementer
                .Where(e => e.Lag == aktuelLag)
                .ToList();

            if (elementerIAktuelLag.Count < 5)
                return aktuelLag;

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
