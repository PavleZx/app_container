using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public class AnimalType
{
        [Key]
        public int AnimalTypeID { get; set; }

        [StringLength(20)]
        public string? AnimalTypes { get; set; }

        public ICollection<Enclosure>? Enclosures { get; set; }
}
