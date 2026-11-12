using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class VisitGroupGroupGuideMDModel
    {
        public VisitGroup? VisitGroup { get; set; }
        public List<GroupGuideBatchRow>? GroupGuides { get; set; }
    }
}
