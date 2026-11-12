using RPPP_WebApp.Model;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors;

public static class AffectsSort
{
    public static IQueryable<Affects> ApplySort(
        this IQueryable<Affects> query,
        int sort,
        bool ascending)
    {
        return sort switch
        {
            1 => ascending
                ? query.OrderBy(a => a.affectsID)
                : query.OrderByDescending(a => a.affectsID),
            
            2 => ascending
                ? query.OrderBy(a => a.reportID)
                : query.OrderByDescending(a => a.reportID),
            
            3 => ascending
                ? query.OrderBy(a => a.Enclosure != null ? a.Enclosure.Name : string.Empty)
                : query.OrderByDescending(a => a.Enclosure != null ? a.Enclosure.Name : string.Empty),
            
            4 => ascending
                ? query.OrderBy(a => a.IndividualAnimal != null ? a.IndividualAnimal.Nickname : string.Empty)
                : query.OrderByDescending(a => a.IndividualAnimal != null ? a.IndividualAnimal.Nickname : string.Empty),
            
            5 => ascending
                ? query.OrderBy(a => a.description ?? string.Empty)
                : query.OrderByDescending(a => a.description ?? string.Empty),
            
            _ => ascending
                ? query.OrderBy(a => a.affectsID)
                : query.OrderByDescending(a => a.affectsID)
        };
    }
}