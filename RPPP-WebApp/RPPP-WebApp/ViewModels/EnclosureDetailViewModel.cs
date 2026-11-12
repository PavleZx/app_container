using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels
{
    public class EnclosureDetailViewModel
    {
        public EnclosuresViewModel? Enclosure { get; set; } // Master data
        public List<IndividualAnimalsViewModel>? IndividualAnimals { get; set; } = new List<IndividualAnimalsViewModel>(); // Detail data
    
            public List<IndividualAnimalsViewModel> NewAnimals { get; set; }
        = new();
    }
}