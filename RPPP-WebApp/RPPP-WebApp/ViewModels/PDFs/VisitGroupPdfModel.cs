namespace RPPP_WebApp.ViewModels.PDFs
{
    public class VisitGroupPdfModel
    {
        public int VisitGroupId { get; set; }
        public string GroupName { get; set; }
        public DateOnly? DateOfVisiting { get; set; }
        public string? Approval { get; set; }
        public int? VisitorCount { get; set; }
        public string TourName { get; set; }

        public IEnumerable<VisitGroupGuidePdfRow> Guides { get; set; }
    }

    public class VisitGroupGuidePdfRow
    {
        public string WorkerName { get; set; }
        public string WorkerPhone { get; set; }
        public string GuideRole { get; set; }
        public string Activity { get; set; }
    }
}
