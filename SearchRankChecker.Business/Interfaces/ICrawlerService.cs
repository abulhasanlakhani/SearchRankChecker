using System.Threading.Tasks;

namespace SearchRankChecker.Business.Interfaces
{
    public interface ICrawlerService
    {
        Task<string> GetSearchResults(string searchTerms);
    }
}
