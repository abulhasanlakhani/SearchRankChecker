using Microsoft.AspNetCore.Mvc;

namespace SearchRankChecker.Web.ViewModels
{
    public class SearchViewModel
    {
        public string RankString { get; set; }
        
        [BindProperty]
        public string SearchRegion { get; set; }
    }
}
