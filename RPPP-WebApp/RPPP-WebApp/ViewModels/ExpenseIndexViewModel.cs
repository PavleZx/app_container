using System;

namespace RPPP_WebApp.ViewModels
{
    public class ExpenseIndexViewModel
    {
        public int TaskId { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; } = [];
        public PagingInfo PagingInfo { get; set; } = new();
    }
}