using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;
    public class IncidentReport
    {
        [Key]
        public int ReportID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateReport { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateSolved { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(10)]
        public string Status { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public int TypeID { get; set; }

        [ForeignKey("TypeID")]
        public virtual IncidentType? IncidentType { get; set; }

        public virtual ICollection<Affects> Affects { get; set; } = new List<Affects>();
    }
