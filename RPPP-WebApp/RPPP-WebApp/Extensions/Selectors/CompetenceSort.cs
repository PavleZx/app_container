using RPPP_WebApp.Model;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors;

public static class CompetenceSort
{
    public static IQueryable<Competence> ApplySort(
    this IQueryable<Competence> query,
    int sort,
    bool ascending)
    {
        return sort switch
        {
            1 => ascending
                ? query.OrderBy(c => c.CompetenceId)
                : query.OrderByDescending(c => c.CompetenceId),

            2 => ascending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name),

            3 => ascending
                ? query.OrderBy(c => c.ExpiryDate)
                : query.OrderByDescending(c => c.ExpiryDate),

            4 => ascending
                ? query.OrderBy(c => c.Certificate.CertificateName)
                : query.OrderByDescending(c => c.Certificate.CertificateName),

            _ => query
        };
    }
}
