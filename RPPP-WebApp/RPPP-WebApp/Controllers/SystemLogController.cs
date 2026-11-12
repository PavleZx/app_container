using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers
{
    public class SystemLogController : Controller
    {
        private readonly Rppp08Context _context;

        public SystemLogController(Rppp08Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.Logs
                .OrderByDescending(l => l.Logged)
                .AsNoTracking()
                .ToListAsync();

            return View(logs);
        }
    }
}