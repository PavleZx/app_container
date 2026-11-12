using RPPP_WebApp.Model;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors;

public static class GroupGuideSort
{
    public static IQueryable<GroupGuide> ApplySort(this IQueryable<GroupGuide> query, int sort, bool ascending)
    {
        Expression<Func<GroupGuide, object?>>? orderSelector = sort switch
        {
            1 => d => d.GroupGuideId,
            2 => d => d.VisitGroup.GroupName,
            3 => d => d.Worker.Name,
            4 => d => d.Worker.Surname,
            5 => d => d.Worker.Phone,
            6 => d => d.GuideRole,
            7 => d => d.Activity,
            _ => null
        };

        if (orderSelector != null)
        {
            query = ascending ?
                   query.OrderBy(orderSelector) :
                   query.OrderByDescending(orderSelector);
        }

        return query;
    }
}