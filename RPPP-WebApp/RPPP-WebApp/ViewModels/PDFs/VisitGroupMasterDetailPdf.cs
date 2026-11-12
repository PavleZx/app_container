using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RPPP_WebApp.ViewModels.PDFs;

public class VisitGroupMasterDetailPdf
    : MasterDetailPdf<VisitGroupPdfModel>
{
    public VisitGroupMasterDetailPdf(
        IEnumerable<VisitGroupPdfModel> data,
        string title)
        : base(data, title) { }

    protected override void RenderMaster(
        VisitGroupPdfModel g,
        IContainer container)
    {
        container.Border(1).Padding(8).Column(col =>
        {
            col.Item().Text(t =>
            {
                t.Span("Group ID: ").Bold();
                t.Span(g.VisitGroupId.ToString());
            });

            col.Item().Text(t =>
            {
                t.Span("Group Name: ").Bold();
                t.Span(g.GroupName);
            });

            col.Item().Text(t =>
            {
                t.Span("Tour type: ").Bold();
                t.Span(g.TourName);
            });

            col.Item().Text(t =>
            {
                t.Span("Date of visit: ").Bold();
                t.Span(g.DateOfVisiting.ToString());
            });

            col.Item().Text(t =>
            {
                t.Span("Approval: ").Bold();
                t.Span(g.Approval);
            });

            col.Item().Text(t =>
            {
                t.Span("Number of visitors: ").Bold();
                t.Span(g.VisitorCount.ToString());
            });
        });
    }

    protected override void RenderDetails(
        VisitGroupPdfModel g,
        IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(4);
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderStyle).Text("Guide");
                h.Cell().Element(HeaderStyle).Text("Guide's phone number");
                h.Cell().Element(HeaderStyle).Text("Guide's role");
                h.Cell().Element(HeaderStyle).Text("Activity");
            });

            foreach (var gg in g.Guides)
            {
                table.Cell().Text(gg.WorkerName);
                table.Cell().Text(gg.WorkerPhone);
                table.Cell().Text(gg.GuideRole);
                table.Cell().Text(gg.Activity);
            }
        });
    }
}
