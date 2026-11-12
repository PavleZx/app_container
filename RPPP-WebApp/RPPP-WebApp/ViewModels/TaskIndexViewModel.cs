using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class TaskIndexViewModel
    {
        public IEnumerable<TaskListViewModel> Tasks { get; set; } = new List<TaskListViewModel>();
        public PagingInfo PagingInfo { get; set; } = null!;
    }
}
