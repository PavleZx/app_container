using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System.Diagnostics.Metrics;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RPPP_WebApp.Controllers;

public class GroupGuideController : Controller
{
    private readonly Rppp08Context context;
    private readonly ILogger<GroupGuideController> logger;
    private readonly AppSettings appSettings;

    public GroupGuideController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<GroupGuideController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            int pageSize = appSettings.PageSize;

            IQueryable<GroupGuide> query = context.GroupGuides
                                          .Include(g => g.VisitGroup)
                                          .Include(g => g.Worker)
                                          .AsNoTracking();
            PagingInfo pagingInfo;

            int count = query.Count();
            if (count == 0)
            {
                pagingInfo = new PagingInfo
                {
                    CurrentPage = 1,
                    ItemsPerPage = pageSize,
                    TotalItems = 0,
                    Sort = sort,
                    Ascending = ascending
                };

                var emptyModel = new PagedList<GroupGuide>(
                    new List<GroupGuide>(),
                    pagingInfo
                );

                return View(emptyModel);
            }


            pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pageSize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index),
                    new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var model = PagedList<GroupGuide>.Create(query, pagingInfo);

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while loading the GroupGuide index page.");
            TempData["Message"] = "An error occurred while loading the GroupGuide index page.";
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        PrepareDropDownLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(GroupGuide groupGuide)
    {
        logger.LogTrace(JsonSerializer.Serialize(groupGuide));
        if (ModelState.IsValid)
        {
            try
            {
                context.Add(groupGuide);
                context.SaveChanges();

                var worker = context.Workers.Find(groupGuide.WorkerId);
                var group = context.VisitGroups.Find(groupGuide.VisitGroupId);

                string message =$"Assigned worker #{worker.WorkerId} to group {group.VisitGroupId}.";
                logger.LogInformation(message);

                TempData["Message"] = message;
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Error assigning guide to group: {0}", exc.Message);
                ModelState.AddModelError(string.Empty, exc.Message);

                PrepareDropDownLists();
                return View(groupGuide);
            }
        }
        else
        {
            PrepareDropDownLists();
            return View(groupGuide);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int groupGuideId, int page = 1, int sort = 1, bool ascending = true)
    {
        var groupGuide = context.GroupGuides.Find(groupGuideId);
        if (groupGuide != null)
        {
            try
            {
                int id = groupGuide.GroupGuideId;
                context.Remove(groupGuide);
                context.SaveChanges();
                string message = $"Assigment #{id} of worker #{groupGuide.WorkerId} as guide of group {groupGuide.VisitGroupId} has been removed.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
            catch (Exception exc)
            {
                string message = "Eror deleting the assigment.";
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                logger.LogError(message);
            }
        }
        else
        {
            string message = $"There is no assigment with id {groupGuideId}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
        }
        return RedirectToAction(nameof(Index), new { page, sort, ascending });
    }

    [HttpGet]
    public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var groupGuide = context.GroupGuides.AsNoTracking().Where(d => d.GroupGuideId == id).SingleOrDefault();
        if (groupGuide == null)
        {
            string message = $"There is no assigment with id {id}";
            logger.LogWarning(message);
            return NotFound(message);
        }
        else
        {
            PrepareDropDownLists();

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View(groupGuide);
        }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            GroupGuide? groupGuide = await context.GroupGuides.FindAsync(id);
            if (groupGuide == null)
            {
                return NotFound("Invalid assigment id: " + id);
            }

            if (await TryUpdateModelAsync<GroupGuide>(groupGuide, "",
                d => d.VisitGroupId, 
                d => d.WorkerId,
                d => d.GuideRole,
                d => d.Activity
            ))
            {
                PrepareDropDownLists();

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation($"Assigment {groupGuide.GroupGuideId} updated.");
                    TempData["Message"] = "Assigment updated.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index), new { page, sort, ascending });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                    return View(groupGuide);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Cannot update model");
                return View(groupGuide);
            }
        }
        catch (Exception exc)
        {
            TempData["Message"] = exc.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Edit), id);
        }
    }

    private void PrepareDropDownLists()
    {
        ViewBag.VisitGroupList = new SelectList(
            context.VisitGroups.AsNoTracking(),
            "VisitGroupId",
            "GroupName"
        );

        ViewBag.WorkerList = new SelectList(
            context.Workers
                .AsNoTracking()
                .Select(w => new
                {
                    w.WorkerId,
                    FullName = w.Name + " " + w.Surname
                })
                .ToList(),
            "WorkerId",
            "FullName"
        );
    }

}