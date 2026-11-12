using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Model
{
    [Table("Log")] 
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }
        public DateTime Logged { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string? Logger { get; set; }
        public string? Exception { get; set; }
    }
}