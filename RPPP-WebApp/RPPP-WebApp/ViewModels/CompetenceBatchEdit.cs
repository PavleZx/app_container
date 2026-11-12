using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class CompetenceBatchEdit
    {
        public int WorkerId { get; set; }
        public List<CompetenceBatchRow>? Competences { get; set; }
    }
}
