using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace MVC_EN.Extensions.Selectors;

public static class IndividualAnimalsSort
{
  public static IQueryable<IndividualAnimals> ApplySort(this IQueryable<IndividualAnimals> query, int sort, bool ascending)
  {
    Expression<Func<IndividualAnimals, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = m => m.AnimalID;
        break;
      case 2:
        orderSelector = m => m.SerialNum;
        break;
      case 3:
        orderSelector = m => m.Nickname;
        break;
      case 4:
        orderSelector = m => m.Acquisition;
        break;
      case 5:
        orderSelector = m => m.Status;
      break;
      case 6:
        orderSelector = m => m.Enclosure.Name;
      break;
      case 7:
        orderSelector = m => m.AnimalName.CroatianName;
      break;
    }
    if (orderSelector != null)
    {
      query = ascending ?
             query.OrderBy(orderSelector) :
             query.OrderByDescending(orderSelector);
    }

    return query;
  }
}