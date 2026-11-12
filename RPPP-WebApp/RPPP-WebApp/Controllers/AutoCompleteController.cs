using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class AutoCompleteController : Controller
    {
        private readonly Rppp08Context context;

        public AutoCompleteController(Rppp08Context context)
        {
            this.context = context;
        }

        public async Task<IActionResult> TourTypes(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<IdLabel>());

            var list = await context.TourTypes
                .Where(t => t.TourName.Contains(term))
                .OrderBy(t => t.TourName)
                .Select(t => new IdLabel(
                    t.TourId,
                    t.TourName
                ))
                .Take(10)
                .ToListAsync();

            return Json(list);
        }
        public async Task<IActionResult> Workers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var list = await context.Workers
                .Where(w =>
                    w.Name.Contains(term) ||
                    w.Surname.Contains(term))
                .OrderBy(w => w.Surname)
                .Select(w => new
                {
                    id = w.WorkerId,
                    label = w.Name + " " + w.Surname,
                    phone = w.Phone
                })
                .Take(10)
                .ToListAsync();

            return Json(list);
        }


    }
}
