using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class GroupGuideViewModel
    {
        public int VisitGroupId { get; set; }

        public int WorkerId { get; set; }

        public int GroupGuideId { get; set; }

        [Required]
        [RegularExpression("^(Main Guide|Assistant|Trainee)$",
        ErrorMessage = "Guide role must be one of: Main Guide, Assistant, Trainee.")]
        public string GuideRole { get; set; }

        [StringLength(50, ErrorMessage = "Activity can have at most 50 characters.")]
        public string Activity { get; set; }
    }
}
