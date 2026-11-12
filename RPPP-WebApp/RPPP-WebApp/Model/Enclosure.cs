using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public class Enclosure
{
        [Key]
        public int EnclosureID { get; set; }
        [StringLength(15)]
        public string? Name { get; set; }
        [StringLength(15)]
        public string? Dimensions { get; set; }
        [StringLength(25)]
        public string? Change { get; set; }
        [StringLength(10)]
        public string? Lighting { get; set; }
        [StringLength(10)]
        public string? Shading { get; set; }

        [ForeignKey(nameof(Location))]
        public int LocationID { get; set; }
        public Location? Location { get; set; }

        [ForeignKey(nameof(AnimalType))]
        public int AnimalTypeID { get; set; }
        public AnimalType? AnimalType { get; set; }

        public ICollection<IndividualAnimals>? IndividualAnimals { get; set; }
        public ICollection<RPPP_WebApp.Model.Task>? Tasks { get; set; }
}
