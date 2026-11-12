using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model;

[Table("Task")]
public class Task
{
    [Key]
    [Column("taskID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TaskID { get; set; }

    [Column("timeStart")]
    public DateTime? TimeStart { get; set; }  // Made nullable to match DB

    [Column("timeEnd")]
    public DateTime? TimeEnd { get; set; }  // Made nullable to match DB

    [Column("notes")]
    [StringLength(40)]
    public string? Notes { get; set; }

    [Column("enclosureID")]
    public int EnclosureID { get; set; }

    [Column("typeID")]
    public int TypeID { get; set; }

    [Column("statusID")]
    public int StatusID { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EnclosureID))]
    public Enclosure? Enclosure { get; set; }

    [ForeignKey(nameof(TypeID))]
    public TaskType? TaskType { get; set; }

    [ForeignKey(nameof(StatusID))]
    public TaskStatus? TaskStatus { get; set; }

    // Collection navigation property
    public ICollection<Expense>? Expenses { get; set; }
}