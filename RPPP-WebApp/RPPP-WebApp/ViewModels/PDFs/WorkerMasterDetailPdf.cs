using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RPPP_WebApp.ViewModels.PDFs;

public class WorkerMasterDetailPdf
    : MasterDetailPdf<WorkerPdfModel>
{
    public WorkerMasterDetailPdf(
        IEnumerable<WorkerPdfModel> data,
        string title)
        : base(data, title)
    {
    }

    protected override void RenderMaster(
        WorkerPdfModel w,
        IContainer container)
    {
        container.Border(1).Padding(8).Column(col =>
        {
            col.Item().Text(t =>
            {
                t.Span("Worker ID: ").Bold();
                t.Span($"{w.WorkerId}");
            });

            col.Item().Text(t =>
            {
                t.Span("Worker: ").Bold();
                t.Span($"{w.Name} {w.Surname}");
            });

            col.Item().Text(t =>
            {
                t.Span("Email: ").Bold();
                t.Span(w.Email);
            });

            col.Item().Text(t =>
            {
                t.Span("Phone number: ").Bold();
                t.Span(w.Phone);
            });

            col.Item().Text(t =>
            {
                t.Span("Avaibility: ").Bold();
                t.Span(w.Availability);
            });
        });
    }

    protected override void RenderDetails(
        WorkerPdfModel w,
        IContainer container)
    {
        container.PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(4);
                c.RelativeColumn(2);
                c.RelativeColumn(3);
            });

            table.Header(h =>
            {
                h.Cell().Element(HeaderStyle).Text("Competence");
                h.Cell().Element(HeaderStyle).Text("Description");
                h.Cell().Element(HeaderStyle).Text("Valid untill");
                h.Cell().Element(HeaderStyle).Text("Certificate");
            });

            foreach (var c in w.Competences)
            {
                table.Cell().Text(c.Name);
                table.Cell().Text(c.Description);
                table.Cell().Text(
                    c.ExpiryDate.HasValue
                        ? c.ExpiryDate.Value.ToString("dd.MM.yyyy")
                        : "-"
                );
                table.Cell().Text(c.Certificate);
            }
        });
    }
}
