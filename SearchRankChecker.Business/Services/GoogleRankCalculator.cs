using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using SearchRankChecker.Business.Interfaces;

namespace SearchRankChecker.Business.Services
{
    public class GoogleRankCalculator : IRankCalculator
    {
        public IConfiguration Configuration { get; }

        public GoogleRankCalculator(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary> 
        /// Examines the search result and retrieves the position. 
        /// </summary> 
        public string GetUrlRanksFromSearchResults(string searchResult, Uri url)
        {
            var lookup = Configuration["HttpClients:GoogleClient:LookupRegex"];
            
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