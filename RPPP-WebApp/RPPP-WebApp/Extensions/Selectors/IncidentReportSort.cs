using RPPP_WebApp.Model;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors;

public static class IncidentReportSort
{
    public static IQueryable<IncidentReport> ApplySort(
        this IQueryable<IncidentReport> query,
        int sort,
        bool ascending)
    {
        return sort switch
        {
            1 => ascending
                ? query.OrderBy(r => r.ReportID)
                : query.OrderByDescending(r => r.ReportID),
            
            2 => ascending
                ? query.OrderBy(r => r.DateReport)
                : query.OrderByDescending(r => r.DateReport),
            
            3 => ascending
                ? query.OrderBy(r => r.DateSolved)
                : query.OrderByDescending(r => r.DateSolved),
            
            4 => ascending
                ? query.OrderBy(r => r.Status)
                : query.OrderByDescending(r => r.Status),
            
            5 => ascending
                ? query.OrderBy(r => r.IncidentType != null ? r.IncidentType.incidentType : string.Empty) 
                : query.OrderByDescending(r => r.IncidentType != null ? r.IncidentType.incidentType : string.Empty),
            
            _ => ascending
                ? query.OrderBy(r => r.ReportID)
                : query.OrderByDescending(r => r.ReportID)
        };
    }
}