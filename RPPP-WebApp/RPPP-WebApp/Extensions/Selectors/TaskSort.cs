using RPPP_WebApp.Model;
using System.Linq.Expressions;

namespace MVC_EN.Extensions.Selectors;

public static class TaskSort
{
    public static IQueryable<RPPP_WebApp.Model.Task> ApplySort(this IQueryable<RPPP_WebApp.Model.Task> query, int sort, bool ascending)
    {
        Expression<Func<RPPP_WebApp.Model.Task, object>> orderSelector = null;

        switch (sort)
        {
            case 1: orderSelector = t => t.TaskID; break;
            case 2: orderSelector = t => t.TimeStart; break;
            case 3: orderSelector = t => t.TimeEnd; break;
            case 4: orderSelector = t => t.Enclosure.Name; break;
            case 5: orderSelector = t => t.TaskType.Name; break;
            case 6: orderSelector = t => t.TaskStatus.Status; break;
        }

        if (orderSelector != null)
        {
            query = ascending
                ? query.OrderBy(orderSelector)
                : query.OrderByDescending(orderSelector);
        }

        return query;
    }
}
