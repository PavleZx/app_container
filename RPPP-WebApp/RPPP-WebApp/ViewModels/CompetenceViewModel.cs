using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class CompetenceViewModel
    {
        [Key]
        public int CompetenceId { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(255, ErrorMessage = "Description cannot be longer than 255 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Worker is required.")]
        public int? WorkerId { get; set; }

        public int? CertificateId { get; set; }
    }
}
