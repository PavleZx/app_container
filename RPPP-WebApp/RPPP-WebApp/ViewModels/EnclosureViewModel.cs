using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.ViewModels
{
  public class EnclosureViewModel
  {
    public List<EnclosuresViewModel>? Enclosures { get; set; }
    public PagingInfo? PagingInfo { get; set; }
  }
}