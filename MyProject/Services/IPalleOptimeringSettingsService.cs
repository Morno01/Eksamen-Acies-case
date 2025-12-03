using MyProject.Models;

namespace MyProject.Services
{
    public interface IPalleOptimeringSettingsService
    {
        Task<IEnumerable<PalleOptimeringSettings>> GetAlleSettings();
        Task<PalleOptimeringSettings?> GetSettings(int id);
        Task<PalleOptimeringSettings?> GetAktivSettings();
        Task<PalleOptimeringSettings> OpretSettings(PalleOptimeringSettings settings);
        Task<PalleOptimeringSettings> OpdaterSettings(PalleOptimeringSettings settings);
        Task<bool> SletSettings(int id);
    }
}
