using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace MVC_EN.Extensions.Selectors;

public static class EnclosureSort
{
  public static IQueryable<Enclosure> ApplySort(this IQueryable<Enclosure> query, int sort, bool ascending)
  {
    Expression<Func<Enclosure, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = m => m.EnclosureID;
        break;
      case 2:
        orderSelector = m => m.Name;
        break;
      case 3:
        orderSelector = m => m.Dimensions;
        break;
      case 4:
        orderSelector = m => m.Change;
        break;
      case 5:
        orderSelector = m => m.Lighting;
        break;
      case 6:
        orderSelector = m => m.Shading;
        break;
      case 7:
        orderSelector = m => m.Location.LocationName;
        break;
      case 8:
        orderSelector = m => m.AnimalType.AnimalTypes;
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