namespace RPPP_WebApp.Model;

public partial class Worker
{
    public string CompetenceList =>
        Competences != null && Competences.Any()
            ? string.Join(", ", Competences.Select(c => c.Name))
            : string.Empty;
}