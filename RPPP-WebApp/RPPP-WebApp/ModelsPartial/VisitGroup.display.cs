namespace RPPP_WebApp.Model;

public partial class VisitGroup
{
    public string GuidesList =>
        GroupGuides != null && GroupGuides.Any()
            ? string.Join(", ",
                GroupGuides
                    .Where(gg => gg.Worker != null)
                    .Select(gg => gg.Worker.Surname))
            : string.Empty;
}
