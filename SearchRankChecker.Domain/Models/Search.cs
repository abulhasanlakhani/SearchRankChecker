using System.ComponentModel.DataAnnotations;

namespace SearchRankChecker.Domain.Models
{
    public class Search
    {
        [Required(ErrorMessage = "Please enter the url")]
        [StringLength(50)]
        [Display(Name = "Url To Search")]
        public string UrlToSearch { get; set; }
        
        [Required(ErrorMessage = "Please enter the search keywords")]
        [StringLength(50)]
        [Display(Name = "Search Keywords")]
        public string SearchKeywords { get; set; }
    }
}
