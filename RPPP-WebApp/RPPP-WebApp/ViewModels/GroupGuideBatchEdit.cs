using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class GroupGuideBatchEdit
    {
        public int VisitGroupId { get; set; }

        public List<GroupGuideBatchRow>? GroupGuides { get; set; }
    }

}
