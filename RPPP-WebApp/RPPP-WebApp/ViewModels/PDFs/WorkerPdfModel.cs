namespace RPPP_WebApp.ViewModels.PDFs;

public class WorkerPdfModel
{
    public int WorkerId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Availability { get; set; }

    public IEnumerable<WorkerCompetencePdfRow> Competences { get; set; }
}

public class WorkerCompetencePdfRow
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string Certificate { get; set; }
}
