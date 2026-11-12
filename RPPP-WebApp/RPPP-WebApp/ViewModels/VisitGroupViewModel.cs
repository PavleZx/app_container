using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class VisitGroupViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string GroupName { get; set; }

        [Required(ErrorMessage = "Date of visiting is required.")]
        public DateOnly? DateOfVisiting { get; set; }

        [StringLength(15, ErrorMessage = "Approval cannot be longer than 15 characters.")]
        public string Approval { get; set; }

        [Required(ErrorMessage = "Visitor count is required.")]
        public int? VisitorCount { get; set; }

        [Key]
        public int VisitGroupId { get; set; }

        [Required]
        public int TourId { get; set; }
    }
}
