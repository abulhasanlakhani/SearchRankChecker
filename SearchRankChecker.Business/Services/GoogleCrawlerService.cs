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
        /// <param name="searchTerms"></param>
        /// <returns></returns>
        public async Task<string> GetSearchResults(string searchTerms)
        {
            if (string.IsNullOrEmpty(searchTerms))
                throw new ArgumentException("Search terms must be provided!");

            var query = BuildCrawlQuery(searchTerms);
            
            var httpClient = _httpClientFactory.CreateClient(nameof(HttpClientsEnum.GoogleClient));

            var request = new HttpRequestMessage(HttpMethod.Get, query);

            var cancellationTokenSource = new CancellationTokenSource();

            using var response =
                await httpClient.GetAsync(request.RequestUri, cancellationTokenSource.Token);
            
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
