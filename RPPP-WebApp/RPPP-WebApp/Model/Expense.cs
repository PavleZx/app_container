using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

[Table("Expenses")]
public class Expense
{
    [Key]
    [Column("expensesID")]
    public int Id { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("arrivalDate")]
    public DateTime ArrivalDate { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("taskID")]
    public int? TaskId { get; set; }

    [Column("priceID")]
    public int? PriceId { get; set; }

    [ForeignKey(nameof(PriceId))]
    public Price? Price { get; set; }

    [ForeignKey(nameof(TaskId))]  // ADD THIS
    public Task? Task { get; set; }  // MAKE NULLABLE
}