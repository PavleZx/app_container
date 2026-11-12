using RPPP_WebApp.Model;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class IncidentAffectsViewModel
    {
        public IncidentReport Report { get; set; }
        public List<Affects> Affects { get; set; } = new List<Affects>();
    }
}