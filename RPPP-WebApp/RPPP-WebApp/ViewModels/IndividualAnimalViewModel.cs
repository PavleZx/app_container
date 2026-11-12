using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels
{
  public class IndividualAnimalViewModel
  {
    public List<IndividualAnimalsViewModel>? IndividualAnimals { get; set; }
    public PagingInfo? PagingInfo { get; set; }
  }
}