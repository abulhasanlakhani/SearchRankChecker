using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchRankChecker.Business.Interfaces;

namespace SearchRankChecker.Business.Services
{
    public class GoogleRankCalculator : IRankCalculator
    {
        private readonly ILogger<GoogleRankCalculator> _logger;
        public IConfiguration Configuration { get; }

        public GoogleRankCalculator(IConfiguration configuration, ILogger<GoogleRankCalculator> logger)
        {
            Configuration = configuration;
            _logger = logger;
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
            var lookup = Configuration["HttpClients:GoogleClient:LookupRegex"];

            if (string.IsNullOrEmpty(lookup))
            {
                _logger.LogError("Lookup regex not found in the config file");
                throw new ArgumentException("Lookup up regex not found!");
            }
            
            var rankList = new List<string>();
            var ranks = new StringBuilder();

            var matches = Regex.Matches(searchResult, lookup);
            
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Groups[2].Value;
                
                if (match.Contains(url.Host))
                    rankList.Add(Convert.ToString(i + 1));
            }
            
            return rankList.Count > 0 ? ranks.AppendJoin(',', rankList).ToString() : "0";
        }
    }
}