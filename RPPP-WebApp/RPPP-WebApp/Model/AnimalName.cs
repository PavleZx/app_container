using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

public class AnimalName
{
        [Key]
        public int NameID { get; set; }
        [StringLength(20)]
        public string? LatinName { get; set; }
        [StringLength(20)]
        public string? CroatianName { get; set; }
        public ICollection<IndividualAnimals>? IndividualAnimals { get; set; }
}
