using System;
using System.Threading;
using System.Threading.Tasks;

namespace SearchRankChecker.Business.Interfaces
{
    public interface ICrawlerService
    {
        Task<string> GetSearchResults(Uri urlToSearch, string searchTerms);
    }
}
