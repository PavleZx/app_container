using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class WorkerCompetenceMasterDetail
    {
        public Worker? Worker { get; set; }
        public List<CompetenceBatchRow>? Competences { get; set; }

        public IEnumerable<SelectListItem>? AvailabilityList { get; set; }
    }
}
