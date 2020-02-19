using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SearchRankChecker.Web.ViewModels
{
    public class SearchViewModel
    {
        public string RankString { get; set; }
        
        [BindProperty]
        [Display(Name = "Search Region")]
        public string SearchRegion { get; set; }
    }
}
