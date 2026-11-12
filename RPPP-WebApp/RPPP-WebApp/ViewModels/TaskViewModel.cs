namespace RPPP_WebApp.ViewModels;

public class TaskViewModel
{
    public int TaskID { get; set; }
    public DateTime? TimeStart { get; set; }
    public DateTime? TimeEnd { get; set; }
    public string? Notes { get; set; }

    public string EnclosureName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string StatusName { get; set; } = "";

    public int EnclosureID { get; set; }
    public int TypeID { get; set; }
    public int StatusID { get; set; }

    public List<ExpenseViewModel> Expenses { get; set; }
            = new List<ExpenseViewModel>();
}