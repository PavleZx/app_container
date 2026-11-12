using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class WorkerViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(15, ErrorMessage = "Name cannot be longer than 15 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(15, ErrorMessage = "Surname cannot be longer than 15 characters.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        public int? AvailabilityId { get; set; }

        [Key]
        public int WorkerId { get; set; }

        [MaxLength(20)]
        [RegularExpression(
        @"^\+?[0-9]{6,15}([ -]?[0-9]{2,4})*$",
        ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

    }
}
