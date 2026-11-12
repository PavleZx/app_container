using System.ComponentModel.DataAnnotations;

public class TaskStatus
{
    [Key]
    public int TaskStatusID { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    public ICollection<RPPP_WebApp.Model.Task>? Tasks { get; set; }
}
