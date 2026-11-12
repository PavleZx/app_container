using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVC_EN.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class IndividualAnimalsController : Controller
    {
        private readonly Rppp08Context _context;
        private readonly ILogger<IndividualAnimalsController> _logger;
        private readonly AppSettings _appSettings;

        public IndividualAnimalsController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<IndividualAnimalsController> logger)
        {
            _context = context;
            _logger = logger;
            _appSettings = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pageSize = _appSettings.PageSize;

            var query = _context.IndividualAnimals
                .Include(p => p.Enclosure)
                .AsNoTracking();

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pageSize,
                TotalItems = count
            };
                    int totalPages = pagingInfo.TotalPages;
                    if (totalPages == 0) totalPages = 1;
                if (page < 1 || page > totalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var individualAnimals = query
                .Select(p => new IndividualAnimalsViewModel
                {
                    AnimalID = p.AnimalID,
                    SerialNum = p.SerialNum,
                    Nickname = p.Nickname,
                    Acquisition = p.Acquisition,
                    Status = p.Status,
                    Name = p.Enclosure.Name,
                    AnimalName = p.AnimalName.CroatianName
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new IndividualAnimalViewModel
            {
                IndividualAnimals = individualAnimals,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name");
                ViewData["NameID"] = new SelectList(_context.AnimalNames, "NameID", "CroatianName");
                ViewData["page"] = page;
                ViewData["sort"] = sort;
                ViewData["ascending"] = ascending;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing the Create view.");
                TempData["Message"] = "An error occurred while preparing the page. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SerialNum,Nickname,Acquisition,Status,EnclosureID,NameID")] IndividualAnimals individualAnimals, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                    _context.Add(individualAnimals);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"Created entity '{individualAnimals.Nickname}'.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error occurred while saving a new individual animal.");
                TempData["Message"] = "An error occurred while saving the individual animal. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating the individual animal.");
                TempData["Message"] = "An unexpected error occurred. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var individualAnimals = await _context.IndividualAnimals.FindAsync(id);
                if (individualAnimals == null)
                {
                    return NotFound();
                }

                ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name", individualAnimals.EnclosureID);
                ViewData["NameID"] = new SelectList(_context.AnimalNames, "NameID", "CroatianName", individualAnimals.NameID);
                ViewData["page"] = page;
                ViewData["sort"] = sort;
                ViewData["ascending"] = ascending;

                return View(individualAnimals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the individual animal for editing.");
                TempData["Message"] = "An error occurred while fetching the individual animal data. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AnimalID,SerialNum,Nickname,Acquisition,Status,EnclosureID,NameID")] IndividualAnimals individualAnimals, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                if (id != individualAnimals.AnimalID)
                {
                    return NotFound();
                }
                    _context.Update(individualAnimals);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"Edited entity '{individualAnimals.Nickname}'.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                if (!_context.IndividualAnimals.Any(e => e.AnimalID == id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(dbEx, "Concurrency error while updating individual animals.");
                    TempData["Message"] = "Concurrency error. Please try again later.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while editing the individual animals.");
                TempData["Message"] = "An unexpected error occurred. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var individualAnimals = await _context.IndividualAnimals.FindAsync(id);
                if (individualAnimals == null)
                {
                    return NotFound();
                }

                _context.IndividualAnimals.Remove(individualAnimals);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Deleted entity '{individualAnimals.Nickname}'.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error occurred while deleting the individual animals.");
                TempData["Message"] = "An error occurred while deleting the individual animals. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting the individual animals.");
                TempData["Message"] = "An unexpected error occurred. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
