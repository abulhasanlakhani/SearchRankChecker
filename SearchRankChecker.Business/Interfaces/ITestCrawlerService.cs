using System;
using System.Threading.Tasks;

namespace SearchRankChecker.Business.Interfaces
{
    public interface ITestCrawlerService
    {
        Task<string> GetSearchResults(Uri urlToSearch);
    }
}
