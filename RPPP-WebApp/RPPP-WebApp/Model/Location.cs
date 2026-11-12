using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public class Location
{
        [Key]
        public int LocationID { get; set; }

        [StringLength(20)]
        public string? LocationName { get; set; }

        // Navigation
        public ICollection<Enclosure>? Enclosures { get; set; }
}
