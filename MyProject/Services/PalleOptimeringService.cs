using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using MyProject.Services.DTOs;

namespace MyProject.Services
{
    public class PalleOptimeringService : IPalleOptimeringService
    {
        private readonly PalleOptimeringContext _context;
        private readonly IPalleOptimeringSettingsService _settingsService;
        private readonly IPalleService _palleService;

        public PalleOptimeringService(
            PalleOptimeringContext context,
            IPalleOptimeringSettingsService settingsService,
            IPalleService palleService)
        {
            _context = context;
            _settingsService = settingsService;
            _palleService = palleService;
        }

        public async Task<PakkeplanResultat> GenererPakkeplan(PakkeplanRequest request)
        {
            var resultat = new PakkeplanResultat();

            var settings = request.SettingsId.HasValue
                ? await _settingsService.GetSettings(request.SettingsId.Value)
                : await _settingsService.GetAktivSettings();

            if (settings == null)
            {
                resultat.Status = "Error";
                resultat.Meddelelser.Add("Ingen aktive settings fundet");
                return resultat;
            }

            var elementer = await _context.Elementer
                .Where(e => request.ElementIds.Contains(e.Id))
                .ToListAsync();

            if (!elementer.Any())
            {
                resultat.Status = "Error";
                resultat.Meddelelser.Add("Ingen elementer fundet");
                return resultat;
            }

            var paller = (await _palleService.GetAlleAktivePaller()).ToList();

            if (!paller.Any())
            {
                resultat.Status = "Error";
                resultat.Meddelelser.Add("Ingen aktive paller fundet");
                return resultat;
            }

            var pakkeplan = new Pakkeplan
            {
                OrdreReference = request.OrdreReference,
                SettingsId = settings.Id,
                Oprettet = DateTime.Now,
                AntalElementer = elementer.Count
            };

            _context.Pakkeplaner.Add(pakkeplan);
            await _context.SaveChangesAsync();

            var optimeringContext = new OptimeringContext
            {
                Elementer = elementer,
                Paller = paller,
                Settings = settings,
                Pakkeplan = pakkeplan
            };

            var pakkeplanPaller = KorOptimeringAlgoritme(optimeringContext);

            pakkeplan.AntalPaller = pakkeplanPaller.Count;
            pakkeplan.Paller = pakkeplanPaller;
            await _context.SaveChangesAsync();

            resultat.PakkeplanId = pakkeplan.Id;
            resultat.AntalPaller = pakkeplan.AntalPaller;
            resultat.AntalElementer = pakkeplan.AntalElementer;
            resultat.Paller = pakkeplanPaller.Select(pp => new PalleResultat
            {
                PalleNummer = pp.PalleNummer,
                PalleBeskrivelse = pp.Palle.PalleBeskrivelse,
                AntalLag = pp.AntalLag,
                SamletVaegt = pp.SamletVaegt,
                SamletHoejde = pp.SamletHoejde,
                Elementer = pp.Elementer.Select(pe => new ElementPlacering
                {
                    ElementId = pe.ElementId,
                    Reference = pe.Element.Reference,
                    Lag = pe.Lag,
                    Plads = pe.Plads,
                    ErRoteret = pe.ErRoteret,
                    Hoejde = pe.Element.Hoejde,
                    Bredde = pe.Element.Bredde,
                    Vaegt = pe.Element.Vaegt
                }).OrderBy(e => e.Lag).ThenBy(e => e.Plads).ToList()
            }).ToList();

            resultat.Meddelelser.Add($"Pakkeplan genereret med {resultat.AntalPaller} paller");

            return resultat;
        }

        private List<PakkeplanPalle> KorOptimeringAlgoritme(OptimeringContext context)
        {
            var pakkeplanPaller = new List<PakkeplanPalle>();
            var sorteringHelper = new ElementSorteringHelper(context.Settings);
            var placeringHelper = new ElementPlaceringHelper(context.Settings);

            var elementerMedData = context.Elementer.Select(e => new ElementMedData(e)
            {
                MinPalleId = FindMindstePalle(e, context.Paller)
            }).ToList();

            var sorterteElementer = sorteringHelper.SorterElementer(context.Elementer);

            foreach (var sorteret in sorterteElementer)
            {
                var match = elementerMedData.First(e => e.Element.Id == sorteret.Element.Id);
                sorteret.MinPalleId = match.MinPalleId;
            }

            int palleNummer = 0;

            foreach (var elementData in sorterteElementer)
            {
                bool placeret = false;

                foreach (var pp in pakkeplanPaller)
                {
                    if (placeringHelper.KanPlaceresPaaPalle(elementData, pp, context.Paller.First(p => p.Id == pp.PalleId)))
                    {
                        placeringHelper.PlacerElement(elementData, pp);
                        placeret = true;
                        break;
                    }
                }

                if (!placeret)
                {
                    palleNummer++;
                    var palle = context.Paller.First(p => p.Id == elementData.MinPalleId);

                    var aktivPalle = new PakkeplanPalle
                    {
                        PakkeplanId = context.Pakkeplan.Id,
                        Pakkeplan = context.Pakkeplan,
                        PalleNummer = palleNummer,
                        PalleId = palle.Id,
                        Palle = palle,
                        AntalLag = 1,
                        SamletHoejde = palle.Hoejde,
                        SamletVaegt = palle.Vaegt
                    };

                    pakkeplanPaller.Add(aktivPalle);
                    placeringHelper.PlacerElement(elementData, aktivPalle);
                }
            }

            return pakkeplanPaller;
        }

        private int? FindMindstePalle(Element element, List<Palle> paller)
        {
            foreach (var palle in paller.OrderBy(p => p.Sortering))
            {
                bool passerPaaLaengde = element.Hoejde <= palle.Laengde + palle.Overmaal;
                bool passerPaaBredde = element.Bredde <= palle.Bredde + palle.Overmaal;
                bool passerRoteret = element.Bredde <= palle.Laengde + palle.Overmaal &&
                                    element.Hoejde <= palle.Bredde + palle.Overmaal;

                if (!string.IsNullOrEmpty(element.KraeverPalletype) &&
                    element.KraeverPalletype != palle.Palletype)
                    continue;

                int totalHoejde = palle.Hoejde + element.Dybde;
                if (totalHoejde > palle.MaksHoejde)
                    continue;

                if (passerPaaLaengde && passerPaaBredde || passerRoteret)
                    return palle.Id;
            }

            return paller.OrderByDescending(p => p.Sortering).First().Id;
        }

        public async Task<Pakkeplan?> GetPakkeplan(int id)
        {
            return await _context.Pakkeplaner
                .Include(p => p.Settings)
                .Include(p => p.Paller)
                    .ThenInclude(pp => pp.Palle)
                .Include(p => p.Paller)
                    .ThenInclude(pp => pp.Elementer)
                        .ThenInclude(pe => pe.Element)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pakkeplan>> GetAllePakkeplaner()
        {
            return await _context.Pakkeplaner
                .Include(p => p.Settings)
                .OrderByDescending(p => p.Oprettet)
                .ToListAsync();
        }
    }

    internal class OptimeringContext
    {
        public List<Element> Elementer { get; set; } = new();
        public List<Palle> Paller { get; set; } = new();
        public PalleOptimeringSettings Settings { get; set; } = null!;
        public Pakkeplan Pakkeplan { get; set; } = null!;
    }
}
