using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels.PDFs;

public abstract class MasterDetailPdf<T> : AbstractPdf
{
    protected readonly IEnumerable<T> data;

    protected MasterDetailPdf(IEnumerable<T> data, string title)
        : base(title)
    {
        this.data = data;
    }

    public override void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header()
                .AlignCenter()
                .Text(title)
                .Bold()
                .FontSize(16);

            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });

            page.Content().Column(col =>
            {
                bool first = true;
                foreach (var item in data)
                {
                    if (!first)
                        col.Item().PageBreak();

                    first = false;

                    col.Item().Element(c => RenderMaster(item, c));
                    col.Item().PaddingVertical(5);
                    col.Item().Element(c => RenderDetails(item, c));
                }
            });
        });
    }

    protected abstract void RenderMaster(T item, IContainer container);
    protected abstract void RenderDetails(T item, IContainer container);
}
