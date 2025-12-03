using MyProject.Models;
using MyProject.Services.DTOs;

namespace MyProject.Services
{
    public interface IPalleOptimeringService
    {
        Task<PakkeplanResultat> GenererPakkeplan(PakkeplanRequest request);
        Task<Pakkeplan?> GetPakkeplan(int id);
        Task<IEnumerable<Pakkeplan>> GetAllePakkeplaner();
    }
}
