using System;
using System.Threading.Tasks;
using SearchRankChecker.Business.Interfaces;

namespace SearchRankChecker.Business.Services
{
    public class TestCrawlerService : ITestCrawlerService
    {
        public async Task<string> GetSearchResults(Uri urlToSearch)
        {
            return await new Task<string>(() => "Hello World");
        }
    }
}
