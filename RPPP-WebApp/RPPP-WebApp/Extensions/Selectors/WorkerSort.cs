using RPPP_WebApp.Model;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors;

public static class WorkerSort
{
    public static IQueryable<Worker> ApplySort(
    this IQueryable<Worker> query,
    int sort,
    bool ascending)
    {
        return sort switch
        {
            1 => ascending
                ? query.OrderBy(w => w.WorkerId)
                : query.OrderByDescending(w => w.WorkerId),

            2 => ascending
                ? query.OrderBy(w => w.Surname)
                : query.OrderByDescending(w => w.Surname),

            3 => ascending
                ? query.OrderBy(w => w.Name)
                : query.OrderByDescending(w => w.Name),

            _ => query
        };
    }
}
