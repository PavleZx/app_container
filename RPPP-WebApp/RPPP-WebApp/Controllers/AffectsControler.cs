using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers;

public class AffectsController : Controller
{
    private readonly Rppp08Context context;
    private readonly ILogger<AffectsController> logger;
    private readonly AppSettings appSettings;

    public AffectsController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<AffectsController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pageSize = appSettings.PageSize;

        var query = context.Affects
            .Include(a => a.IncidentReport)
            .Include(a => a.Enclosure)
            .Include(a => a.IndividualAnimal)
            .AsNoTracking();

        query = query.ApplySort(sort, ascending);

        int count = query.Count();

        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pageSize,
            TotalItems = count
        };

        if (page < 1 && count > 0)
        {
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }

        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var model = new PagedList<Affects>(items, pagingInfo);
        return View(model);
    }

    [HttpGet]
    public IActionResult Create(int? reportId, string? returnUrl)
    {
        PrepareDropDownLists();
        
        var model = new Affects();
        if (reportId.HasValue)
        {
            model.reportID = reportId.Value;
        }

        ViewBag.ReturnUrl = returnUrl;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Affects affects, string? returnUrl)
    {
        ModelState.Remove("IncidentReport");
        ModelState.Remove("Enclosure");
        ModelState.Remove("IndividualAnimal"); 

        if (ModelState.IsValid)
        {
            try
            {
                int nextId = context.Affects.Any()
                             ? context.Affects.Max(a => a.affectsID) + 1
                             : 1;
                affects.affectsID = nextId;

                context.Add(affects);
                context.SaveChanges();

                string message = $"Affects record #{affects.affectsID} added.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("MasterDetail", "IncidentReport", new { id = affects.reportID });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding a new affects record.");
                TempData["Message"] = "An error occurred while adding a new affects record.";
                TempData["MessageType"] = "error";
                
                PrepareDropDownLists();
                ViewBag.ReturnUrl = returnUrl;
                return View(affects);
            }
        }

        PrepareDropDownLists();
        ViewBag.ReturnUrl = returnUrl;
        return View(affects);
    }

    [HttpGet]
    public IActionResult Edit(int id, string? returnUrl)
    {
        var affects = context.Affects
            .AsNoTracking()
            .SingleOrDefault(a => a.affectsID == id);

        if (affects == null)
        {
            return NotFound($"There is no affects record with ID {id}");
        }

        PrepareDropDownLists();
        ViewBag.ReturnUrl = returnUrl;

        return View(affects);
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string? returnUrl)
    {
        try
        {
            var affects = await context.Affects.FindAsync(id);
            if (affects == null)
            {
                return NotFound("Invalid affects ID: " + id);
            }

            ModelState.Remove("IncidentReport");
            ModelState.Remove("Enclosure");
            ModelState.Remove("IndividualAnimal");

            if (await TryUpdateModelAsync<Affects>(
                affects,
                "",
                a => a.reportID,
                a => a.groupID,
                a => a.enclosureID,
                a => a.animalID,
                a => a.description
            ))
            {
                try
                {
                    await context.SaveChangesAsync();
                    string message = $"Affects #{id} updated.";
                    logger.LogInformation(message);
                    TempData["Message"] = message;
                    TempData["MessageType"] = "success";

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("MasterDetail", "IncidentReport", new { id = affects.reportID });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    TempData["Message"] = "Error saving changes.";
                    PrepareDropDownLists();
                    ViewBag.ReturnUrl = returnUrl; 
                    return View(affects);
                }
            }
            else
            {
                PrepareDropDownLists();
                ViewBag.ReturnUrl = returnUrl;
                return View(affects);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return RedirectToAction(nameof(Edit), new { id, returnUrl });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int affectsID, string? returnUrl)
    {
        var affects = context.Affects.Find(affectsID);
        if (affects != null)
        {
            try
            {
                int rId = affects.reportID;
                context.Remove(affects);
                context.SaveChanges();
                
                string message = $"Affects #{affectsID} deleted";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("MasterDetail", "IncidentReport", new { id = rId });
            }
            catch (Exception ex)
            {
                string message = "An error occurred while deleting the record.";
                logger.LogError(ex, message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                
                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Affects");
            }
        }
        else
        {
            TempData["Message"] = "Record not found";
            TempData["MessageType"] = "error";
        }

        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Affects");
    }

    private void PrepareDropDownLists()
    {   
        ViewBag.ReportList = new SelectList(
            context.IncidentReports.Select(r => new { 
                ReportID = r.ReportID, 
                DisplayName = $"Report #{r.ReportID} ({r.DateReport})" 
            }).AsNoTracking(),
            "ReportID",
            "DisplayName"
        );

        ViewBag.EnclosureList = new SelectList(
            context.Enclosures.AsNoTracking(),
            "EnclosureID",
            "Name"
        );

        ViewBag.AnimalList = new SelectList(
            context.IndividualAnimals.AsNoTracking(),
            "AnimalID",
            "Nickname"
        );
    }
}