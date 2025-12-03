using MyProject.Models;

namespace MyProject.Services
{
    public interface IElementService
    {
        Task<IEnumerable<Element>> GetAlleElementer();
        Task<Element?> GetElement(int id);
        Task<Element> OpretElement(Element element);
        Task<Element> OpdaterElement(Element element);
        Task<bool> SletElement(int id);
        Task<IEnumerable<Element>> OpretFlereElementer(IEnumerable<Element> elementer);
    }
}
