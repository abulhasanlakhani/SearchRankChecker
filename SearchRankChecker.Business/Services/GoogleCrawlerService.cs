using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Enums;

namespace SearchRankChecker.Business.Services
{
    public class GoogleCrawlerService : ICrawlerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleCrawlerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Uses HttpClient to crawl Google Search and bring the results as string
        /// </summary>
        /// <param name="urlToSearch"></param>
        /// <param name="searchTerms"></param>
        /// <returns></returns>
        public async Task<string> GetSearchResults(Uri urlToSearch, string searchTerms)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var query = BuildCrawlQuery(searchTerms);
            
            var httpClient = _httpClientFactory.CreateClient(nameof(HttpClientsEnum.GoogleClient));

            var request = new HttpRequestMessage(HttpMethod.Get, query);

            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Build the query to be sent to google search
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <returns>Search query with encoded search terms</returns>
        private string BuildCrawlQuery(string searchTerms)
        {
            var encodedSearchTerm = HttpUtility.UrlEncode(searchTerms);
            
            // Add "&num=100" to get 100 search results;
            return $"search?q={encodedSearchTerm}";
        }
    }
}
