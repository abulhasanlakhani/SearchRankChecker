using System;

namespace SearchRankChecker.Business.Interfaces
{
    public interface IRankCalculator
    {
        string GetUrlRanksFromSearchResults(string searchResult, Uri url);
    }
}
