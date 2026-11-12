using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RPPP_WebApp.Model;
public class Price
{
    public int PriceID { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public double PriceValue { get; set; }
}

