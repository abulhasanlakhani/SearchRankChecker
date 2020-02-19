using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Models;

namespace SearchRankChecker.Business.Services
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CrawlerService> _logger;
        private readonly IOptions<AppSettings> _settings;
        public IConfiguration Configuration { get; }

        public CrawlerService(IHttpClientFactory httpClientFactory, ILogger<CrawlerService> logger, IConfiguration configuration, IOptions<AppSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings;
            Configuration = configuration;
        }

        /// <summary>
        /// Uses HttpClient to crawl Google Search and bring the results as string
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <exception cref="WebException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public async Task<string> GetSearchResults(string searchTerms)
        {
            if (string.IsNullOrEmpty(searchTerms))
                throw new ArgumentException("Search terms must be provided!");

            var query = BuildCrawlQuery(searchTerms);
            var httpClient = _httpClientFactory.CreateClient("SearchClient");

            var request = new HttpRequestMessage(HttpMethod.Get, query);

            try
            {
                using var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e) when (e is WebException || e is HttpRequestException || e is SocketException)
            {
                _logger.LogError(e, "Exception occured while processing the http request!");
                throw;
            }
        }

        /// <summary>
        /// Build the query to be sent to google search
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <returns>Search query with encoded search terms</returns>
        private string BuildCrawlQuery(string searchTerms)
        {
            var encodedSearchTerm = HttpUtility.UrlEncode(searchTerms);

            var searchQuery = $"search?q={encodedSearchTerm}";
            
            var selectedHttpClient = GetSelectedHttpClient();
            
            var maxSearchResultsConfig = Configuration[$"HttpClientSettings:{selectedHttpClient}:MaxSearchResults"];

            if (!string.IsNullOrEmpty(maxSearchResultsConfig))
            {
                searchQuery += $"&num={maxSearchResultsConfig}";
            }
            
            return searchQuery;
        }

        private string GetSelectedHttpClient()
        {
            var selectedHttpClient = _settings.Value.SelectedHttpClient;

            if (string.IsNullOrEmpty(selectedHttpClient))
                throw new ArgumentException("Default HttpClient should be set in the config");

            return selectedHttpClient;
        }
    }
}
