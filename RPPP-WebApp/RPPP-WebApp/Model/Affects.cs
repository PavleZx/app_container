using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model
{
    public class Affects
    {
        [Key]
        public int affectsID { get; set; } 

        [Required(ErrorMessage = "RepordID is required..")]
        public int reportID { get; set; }

        public int? groupID { get; set; }

        public int? enclosureID { get; set; }

        public int? animalID { get; set; }

        [StringLength(100)]
        public string? description { get; set; }

        [ForeignKey("reportID")]
        public virtual IncidentReport? IncidentReport { get; set; }

        [ForeignKey("enclosureID")]
        public virtual Enclosure? Enclosure { get; set; }

        [ForeignKey("animalID")]
        public virtual IndividualAnimals? IndividualAnimal { get; set; }

        [NotMapped]
        public bool IsDeleted { get; set; } = false;
    }
}