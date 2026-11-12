using RPPP_WebApp.Model;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors;

public static class VisitGroupSort
{
    public static IQueryable<VisitGroup> ApplySort(this IQueryable<VisitGroup> query, int sort, bool ascending)
    {
        Expression<Func<VisitGroup, object?>>? orderSelector = sort switch
        {
            1 => d => d.VisitGroupId,
            2 => d => d.GroupName,
            3 => d => d.DateOfVisiting,
            4 => d => d.Approval,
            5 => d => d.VisitorCount,
            6 => d => d.Tour.TourName,
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