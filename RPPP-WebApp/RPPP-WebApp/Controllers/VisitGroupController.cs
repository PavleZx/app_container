using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using QuestPDF.Fluent;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.ViewModels.PDFs;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace RPPP_WebApp.Controllers;

public class VisitGroupController : Controller
{
    private readonly Rppp08Context context;
    private readonly ILogger<VisitGroupController> logger;
    private readonly AppSettings appSettings;

    public VisitGroupController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<VisitGroupController> logger)
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

            IQueryable<VisitGroup> query = context.VisitGroups
                                          .Include(g => g.Tour)
                                           .Include(g => g.GroupGuides)
                                            .ThenInclude(g => g.Worker)
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

                var emptyModel = new PagedList<VisitGroup>(
                    new List<VisitGroup>(),
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

            var model = PagedList<VisitGroup>.Create(query, pagingInfo);

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while loading the VisitGroup index page.");
            TempData["Message"] = "An error occurred while loading the VisitGroup index page.";
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        PrepareDropDownList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(VisitGroup visitGroup)
    {
        logger.LogTrace(JsonSerializer.Serialize(visitGroup));
        if (ModelState.IsValid)
        {
            try
            {
                context.Add(visitGroup);
                context.SaveChanges();

                string message = $"Added visit group #{visitGroup.VisitGroupId}";
                logger.LogInformation(message);

                TempData["Message"] = message;
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Error adding new group {0}", exc.Message);
                ModelState.AddModelError(string.Empty, exc.Message);

                PrepareDropDownList();
                return View(visitGroup);
            }
        }
        else
        {
            PrepareDropDownList();
            return View(visitGroup);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int visitGroupId, int page = 1, int sort = 1, bool ascending = true)
    {
        var group = context.VisitGroups.Find(visitGroupId);
        if (group != null)
        {
            try
            {
                context.Remove(group);
                context.SaveChanges();
                string message = $"Visit group #{group.VisitGroupId} has been removed.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
            catch (Exception exc)
            {
                string message = "Eror deleting the group.";
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                logger.LogError(message);
            }
        }
        else
        {
            string message = $"There is no assigment with id {visitGroupId}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
        }
        return RedirectToAction(nameof(Index), new { page, sort, ascending });
    }

    [HttpGet]
    public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var group = context.VisitGroups.AsNoTracking().Where(d => d.VisitGroupId == id).SingleOrDefault();
        if (group == null)
        {
            string message = $"There is no group with id {id}";
            logger.LogWarning(message);
            return NotFound(message);
        }
        else
        {
            PrepareDropDownList();

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            return View(group);
        }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            VisitGroup? group = await context.VisitGroups.FindAsync(id);
            if (group == null)
            {
                return NotFound("Invalid group id: " + id);
            }

            if (await TryUpdateModelAsync<VisitGroup>(group, "",
                d => d.GroupName, 
                d => d.DateOfVisiting,
                d => d.Approval,
                d => d.VisitorCount,
                d => d.TourId
            ))
            {
                PrepareDropDownList();

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                try
                {
                    await context.SaveChangesAsync();
                    string message = $"Visit group #{group.VisitGroupId}";
                    logger.LogInformation(message);
                    TempData["Message"] = "Group updated.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index), new { page, sort, ascending });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                    return View(group);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Cannot update model");
                return View(group);
            }
        }
        catch (Exception exc)
        {
            TempData["Message"] = exc.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Edit), id);
        }
    }

    [HttpGet]
    public IActionResult MasterDetail(int id)
    {
        try
        {
            var group = context.VisitGroups
                .Include(g => g.Tour)
                .SingleOrDefault(g => g.VisitGroupId == id);

            if (group == null)
            {
                string message = $"Invalid group ID {id}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound("Invalid group ID: " + id);
            }

            var model = new VisitGroupGroupGuideMDModel
            {
                VisitGroup = group,

                GroupGuides = context.GroupGuides
                    .Include(gg => gg.Worker)
                    .Where(gg => gg.VisitGroupId == id)
                    .Select(gg => new GroupGuideBatchRow
                    {
                        GroupGuideId = gg.GroupGuideId,
                        VisitGroupId = gg.VisitGroupId,
                        WorkerId = gg.WorkerId,
                        Worker = gg.Worker,
                        GuideRole = gg.GuideRole,
                        Activity = gg.Activity,
                        IsDeleted = false
                    })
                    .ToList()
            };

            PrepareDropDownList();

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MasterDetailEditVisitGroup(VisitGroupGroupGuideMDModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.GroupGuides = context.GroupGuides
                    .Include(gg => gg.Worker)
                    .Where(gg => gg.VisitGroupId == model.VisitGroup.VisitGroupId)
                    .Select(gg => new GroupGuideBatchRow
                    {
                        GroupGuideId = gg.GroupGuideId,
                        VisitGroupId = gg.VisitGroupId,
                        WorkerId = gg.WorkerId,
                        Worker = gg.Worker,
                        GuideRole = gg.GuideRole,
                        Activity = gg.Activity,
                        IsDeleted = false
                    })
                    .ToList();

                    PrepareDropDownList();
                return View("MasterDetail", model);
            }

            VisitGroup? group =
                context.VisitGroups.Find(model.VisitGroup.VisitGroupId);

            if (group == null)
            {
                string message =
                    $"Invalid visit group ID {model.VisitGroup.VisitGroupId}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound(message);
            }

            UpdateVisitGroup(model.VisitGroup);

            return RedirectToAction(nameof(MasterDetail),
                new { id = model.VisitGroup.VisitGroupId });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(MasterDetail),
                new { id = model.VisitGroup.VisitGroupId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveGroupGuidesBatch(GroupGuideBatchEdit model)
    {
        try
        {
            if (model.GroupGuides == null)
            {
                model.GroupGuides = new List<GroupGuideBatchRow>();
            }


            for (int i = 0; i < model.GroupGuides.Count; i++)
            {
                if (model.GroupGuides[i].IsDeleted)
                {
                    ModelState.Remove($"GroupGuides[{i}].WorkerId");
                    ModelState.Remove($"GroupGuides[{i}].GuideRole");
                    ModelState.Remove($"GroupGuides[{i}].Activity");
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var gg in model.GroupGuides)
                {
                    if (gg.IsDeleted || gg.WorkerId == 0)
                        continue;

                    var worker = context.Workers
                        .SingleOrDefault(w => w.WorkerId == gg.WorkerId);

                    if (worker != null)
                    {
                        gg.Worker = worker;
                    }
                }

                var group = context.VisitGroups
                    .Include(g => g.Tour)
                    .Single(g => g.VisitGroupId == model.VisitGroupId);

                var masterModel = new VisitGroupGroupGuideMDModel {
                    VisitGroup = group,
                    GroupGuides = model.GroupGuides
                };

                PrepareDropDownList();

                return View("MasterDetail", masterModel);
            }

            foreach (var row in model.GroupGuides)
            {
                if (row.GroupGuideId == 0 && !row.IsDeleted)
                {
                    var entity = new GroupGuide
                    {
                        VisitGroupId = model.VisitGroupId,
                        WorkerId = row.WorkerId,
                        GuideRole = row.GuideRole,
                        Activity = row.Activity
                    };

                    context.GroupGuides.Add(entity);
                    logger.LogInformation($"Added worker #{entity.WorkerId} as guide to visit group #{entity.VisitGroupId}.");
                    continue;
                }

                var db = context.GroupGuides
                    .SingleOrDefault(g => g.GroupGuideId == row.GroupGuideId);

                if (db == null)
                    continue;

                if (row.IsDeleted)
                {
                    context.GroupGuides.Remove(db);
                    logger.LogInformation($"Removed worker #{row.WorkerId} from visit group #{row.VisitGroupId}.");
                    continue;
                }

                if(db.WorkerId != row.WorkerId ||
                    db.GuideRole != row.GuideRole ||
                    db.Activity != row.Activity)
                {
                    logger.LogInformation($"Guide assigment to visit group #{row.VisitGroupId} has been updated.");
                }
                db.WorkerId = row.WorkerId;
                db.GuideRole = row.GuideRole;
                db.Activity = row.Activity;
            }

            context.SaveChanges();

            TempData["Message"] = "Guides updated";
            TempData["MessageType"] = "success";

            return RedirectToAction(
                nameof(MasterDetail),
                new { id = model.VisitGroupId }
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";

            return RedirectToAction(
                nameof(MasterDetail),
                new { id = model.VisitGroupId }
            );
        }
    }


    private void UpdateVisitGroup(VisitGroup groupEdit)
    {
        try
        {
            var group = context.VisitGroups.Find(groupEdit.VisitGroupId);
            if (group == null)
            {
                string message = $"Invalid group ID {groupEdit.VisitGroupId}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
            }
            else
            {
                group.GroupName = groupEdit.GroupName;
                group.DateOfVisiting = groupEdit.DateOfVisiting;
                group.Approval = groupEdit.Approval;
                group.VisitorCount = groupEdit.VisitorCount;
                group.TourId = groupEdit.TourId;

                context.SaveChanges();

                string message = $"Group #{group.VisitGroupId} updated.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
        }
    }

    private void PrepareDropDownList()
    {
        ViewBag.TourTypeList = new SelectList(
            context.TourTypes.AsNoTracking(),
            "TourId",
            "TourName"
        );
    }

    [HttpGet]
    public IActionResult MasterDetailPdf(int id)
    {
        var group = context.VisitGroups
            .Include(g => g.Tour)
            .Include(g => g.GroupGuides)
                .ThenInclude(gg => gg.Worker)
            .SingleOrDefault(g => g.VisitGroupId == id);

        if (group == null)
            return NotFound();

        var model = new VisitGroupPdfModel
        {
            VisitGroupId = group.VisitGroupId,
            GroupName = group.GroupName,
            DateOfVisiting = group.DateOfVisiting,
            Approval = group.Approval,
            VisitorCount = group.VisitorCount,
            TourName = group.Tour.TourName,
            Guides = group.GroupGuides.Select(gg => new VisitGroupGuidePdfRow
            {
                WorkerName = gg.Worker.Name + " " + gg.Worker.Surname,
                WorkerPhone = gg.Worker.Phone,
                GuideRole = gg.GuideRole,
                Activity = gg.Activity
            }).ToList()
        };

        var pdf = new VisitGroupMasterDetailPdf(
            new[] { model },
            $"Report for Visit Group #{group.VisitGroupId}"
        );

        byte[] bytes = pdf.GeneratePdf();

        Response.Headers.Append(
            "content-disposition",
            $"inline; filename=visitgroup_{id}.pdf"
        );

        return File(bytes, "application/pdf");
    }

    [HttpGet]
    public IActionResult MasterDetailPdfRaport()
    {
        var groups = context.VisitGroups
            .Include(g => g.Tour)
            .Include(g => g.GroupGuides)
                .ThenInclude(gg => gg.Worker)
            .AsNoTracking()
            .OrderBy(g => g.DateOfVisiting)
            .ThenBy(g => g.GroupName)
            .ToList();

        if (groups.Count == 0)
            return NotFound("No visit groups found.");

        var models = groups.Select(g => new VisitGroupPdfModel
        {
            VisitGroupId = g.VisitGroupId,
            GroupName = g.GroupName,
            DateOfVisiting = g.DateOfVisiting,
            Approval = g.Approval,
            VisitorCount = g.VisitorCount,
            TourName = g.Tour?.TourName ?? "-",

            Guides = g.GroupGuides.Select(gg => new VisitGroupGuidePdfRow
            {
                WorkerName = gg.Worker != null
                    ? $"{gg.Worker.Name} {gg.Worker.Surname}"
                    : "-",
                WorkerPhone = gg.Worker.Phone,
                GuideRole = gg.GuideRole,
                Activity = gg.Activity
            }).ToList()
        }).ToList();

        var pdf = new VisitGroupMasterDetailPdf(
            models,
            $"Visit groups – master-detail report"
        );

        byte[] bytes = pdf.GeneratePdf();

        Response.Headers.Append(
            "content-disposition",
            "inline; filename=visitgroups_master_detail.pdf"
        );

        return File(bytes, "application/pdf");
    }

    [HttpGet]
    public IActionResult MasterDetailExcelReport()
    {
        var groups = context.VisitGroups
            .Include(g => g.Tour)
            .Include(g => g.GroupGuides)
                .ThenInclude(gg => gg.Worker)
            .AsNoTracking()
            .OrderBy(g => g.DateOfVisiting)
            .ThenBy(g => g.GroupName)
            .ToList();

        if (!groups.Any())
            return NotFound("No visit groups");

        using var workbook = new XLWorkbook();

        foreach (var group in groups)
        {
            var sheetName = $"{group.GroupName}_{group.VisitGroupId}";
            if (sheetName.Length > 31)
                sheetName = sheetName[..31];

            var ws = workbook.Worksheets.Add(sheetName);
            int row = 1;

            ws.Cell(row, 1).Value = "Visit Group";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "#";
            ws.Cell(row, 2).Value = group.VisitGroupId;
            row++;

            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = group.GroupName;
            row++;

            ws.Cell(row, 1).Value = "Date of visiting";
            ws.Cell(row, 2).Value =
                group.DateOfVisiting.HasValue
                    ? group.DateOfVisiting.Value
                        .ToDateTime(TimeOnly.MinValue)
                    : "-";
            ws.Cell(row, 2).Style.DateFormat.Format = "yyyy-MM-dd";
            row++;

            ws.Cell(row, 1).Value = "Approved";
            ws.Cell(row, 2).Value = group.Approval;
            row++;

            ws.Cell(row, 1).Value = "Number of visitors";
            ws.Cell(row, 2).Value = group.VisitorCount;
            row++;

            ws.Cell(row, 1).Value = "Tour type";
            ws.Cell(row, 2).Value = group.Tour?.TourName ?? "-";
            row += 2;


            ws.Cell(row, 1).Value = "Guides";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "#";
            ws.Cell(row, 2).Value = "Name";
            ws.Cell(row, 3).Value = "Phone number";
            ws.Cell(row, 4).Value = "Guide's role";
            ws.Cell(row, 5).Value = "Activity";

            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            row++;

            foreach (var gg in group.GroupGuides)
            {
                ws.Cell(row, 1).Value = gg.GroupGuideId;
                ws.Cell(row, 2).Value =
                    gg.Worker != null
                        ? $"{gg.Worker.Name} {gg.Worker.Surname}"
                        : "-";

                ws.Cell(row, 3).Value = gg.Worker?.Phone ?? "-";
                ws.Cell(row, 4).Value = gg.GuideRole;
                ws.Cell(row, 5).Value = gg.Activity;

                row++;
            }

            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "visitgroups_master_detail.xlsx"
        );
    }


}