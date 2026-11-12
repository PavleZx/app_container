using System;

namespace RPPP_WebApp.ViewModels
{
    public class ExpenseViewModel
    {
        public int ExpenseID { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public string? Description { get; set; }
        public decimal PriceAmount { get; set; }
        public bool IsDeleted { get; set; }
        public string? PriceName { get; set; }
        public int? PriceId { get; set; }
        public int TaskId { get; set; }

    }
}
