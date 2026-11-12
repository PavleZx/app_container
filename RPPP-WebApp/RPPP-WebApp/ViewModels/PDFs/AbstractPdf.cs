using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RPPP_WebApp.ViewModels.PDFs;

public abstract class AbstractPdf : IDocument
{
    protected readonly string title;

    protected AbstractPdf(string title)
    {
        this.title = title;
    }

    public virtual DocumentMetadata GetMetadata() => new()
    {
        Title = title,
        Author = "RPPP",
        Subject = "Report"
    };

    public virtual DocumentSettings GetSettings() => DocumentSettings.Default;

    protected static IContainer HeaderStyle(IContainer container) =>
        container.DefaultTextStyle(x => x.SemiBold())
                 .PaddingVertical(5)
                 .BorderBottom(1)
                 .BorderColor(Colors.Black);

    public abstract void Compose(IDocumentContainer container);
}
