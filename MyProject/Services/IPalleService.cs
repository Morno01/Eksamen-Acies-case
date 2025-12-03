using MyProject.Models;

namespace MyProject.Services
{
    public interface IPalleService
    {
        Task<IEnumerable<Palle>> GetAlleAktivePaller();
        Task<IEnumerable<Palle>> GetAllePaller();
        Task<Palle?> GetPalle(int id);
        Task<Palle> OpretPalle(Palle palle);
        Task<Palle> OpdaterPalle(Palle palle);
        Task<bool> SletPalle(int id);
    }
}
