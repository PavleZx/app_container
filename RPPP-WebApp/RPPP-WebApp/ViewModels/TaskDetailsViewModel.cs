namespace RPPP_WebApp.ViewModels
{
    public class TaskDetailsViewModel
    {
        public TaskViewModel Task { get; set; } = new();

        public List<ExpenseViewModel> Expenses { get; set; }
            = new List<ExpenseViewModel>();
    }
}