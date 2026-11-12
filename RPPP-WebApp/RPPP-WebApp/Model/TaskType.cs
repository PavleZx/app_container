public class TaskType
{
    public int TaskTypeID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<RPPP_WebApp.Model.Task>? Tasks { get; set; }
}
