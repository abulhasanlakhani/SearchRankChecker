using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Domain.Models;

namespace SearchRankChecker.Business.Services
{
    public class RankCalculatorService : IRankCalculator
    {
        private readonly ILogger<RankCalculatorService> _logger;
        public IConfiguration Configuration { get; }
        private readonly IOptions<AppSettings> _settings;

        public RankCalculatorService(IConfiguration configuration, ILogger<RankCalculatorService> logger, IOptions<AppSettings> settings)
        {
            Configuration = configuration;
            _logger = logger;
            _settings = settings;
        }
        /// <summary> 
        /// Examines the search result and retrieves the position. 
        /// </summary>
        /// <param name="searchResult">Results received from request to http client</param>
        /// <param name="url">Uri to search within the search results</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetUrlRanksFromSearchResults(string searchResult, Uri url)
        {
            var selectedHttpClient = _settings.Value.SelectedHttpClient;

            if (string.IsNullOrEmpty(selectedHttpClient))
                throw new ArgumentException("Default HttpClient should be set in the config");

            var lookup = Configuration[$"HttpClientSettings:{selectedHttpClient}:LookupRegex"];

            if (string.IsNullOrEmpty(lookup))
            {
                _logger.LogError("Lookup regex not found in the config file");
                throw new ArgumentException("Lookup up regex not found!");
            }
            
            var rankList = new List<string>();
            var ranks = new StringBuilder();

            var matches = Regex.Matches(searchResult, lookup);
            
            var urlToMatch = url.Host;

            var directoryPathConfig = Configuration["SearchDefaults:DirectoryPath"];

            if (!string.IsNullOrEmpty(directoryPathConfig))
            {
                urlToMatch += directoryPathConfig;
            }

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Groups[2].Value;
                
                if (match.Contains(urlToMatch))
                    rankList.Add(Convert.ToString(i + 1));
            }
            
            return rankList.Count > 0 ? ranks.AppendJoin(',', rankList).ToString() : "0";
        }
    }
}