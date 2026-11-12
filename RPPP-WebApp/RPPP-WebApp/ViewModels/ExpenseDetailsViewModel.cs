using System;

namespace RPPP_WebApp.ViewModels
{
    public class ExpenseDetailsViewModel
    {
        public int ExpenseID { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public string? Description { get; set; }
        public decimal PriceAmount { get; set; }
        public int TaskID { get; set; }

    }
}
