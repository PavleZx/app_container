using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels
{
  public class EnclosuresViewModel
  {
    public int EnclosureID { get; set; } 
    public string? Name { get; set; }
    public string? Dimensions { get; set; }
    public string? Change { get; set; }
    public string? Lighting { get; set; }
    public string? Shading { get; set; }
    public string? LocationName { get; set; }
    public string? AnimalTypes { get; set; }
  }
}

