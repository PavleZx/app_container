using System;

namespace RPPP_WebApp.ViewModels
{
    public class TaskListViewModel
    {
        public int TaskID { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public string EnclosureName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
    }
}
