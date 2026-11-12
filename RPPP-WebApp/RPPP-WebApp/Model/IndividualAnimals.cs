using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public class IndividualAnimals
{
        [Key]
        public int AnimalID { get; set; }

        public int SerialNum { get; set; }

        [StringLength(15)]
        public string? Nickname { get; set; }

        [StringLength(15)]
        public string? Acquisition { get; set; }

        [StringLength(10)]
        public string? Status { get; set; }

        // Foreign keys
        [ForeignKey(nameof(Enclosure))]
        public int EnclosureID { get; set; }

        public Enclosure? Enclosure { get; set; }

        [ForeignKey(nameof(AnimalName))]
        public int? NameID { get; set; }

        public AnimalName? AnimalName { get; set; }
}
