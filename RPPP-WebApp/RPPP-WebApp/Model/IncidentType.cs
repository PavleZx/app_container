using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;
    public class IncidentType
    {
        [Key]
        public int IncidentTypeID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(15)]
        public string incidentType { get; set; }
        [StringLength(30)]
        public string? Description { get; set; }

        public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
    }
