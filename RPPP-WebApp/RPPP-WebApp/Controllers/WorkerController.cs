using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
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

/// <summary>
/// MVC controller responsible for managing workers.
/// Provides CRUD operations, master-detail editing of worker competences,
/// and export of worker data to PDF and Excel formats.
/// </summary>
/// <remarks>
/// This controller handles:
/// <list type="bullet">
/// <item>paged and sorted listing of workers</item>
/// <item>creation, editing and deletion of workers</item>
/// <item>master-detail editing of worker competences</item>
/// <item>generation of master-detail PDF and Excel reports</item>
/// </list>
/// </remarks>
public class WorkerController : Controller
{
    private readonly Rppp08Context context;
    private readonly ILogger<WorkerController> logger;
    private readonly AppSettings appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerController"/> class.
    /// </summary>
    /// <param name="context">Database context used for data access.</param>
    /// <param name="options">Application configuration settings.</param>
    /// <param name="logger">Logger instance for diagnostic messages.</param>
    public WorkerController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<WorkerController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }

    /// <summary>
    /// Displays a paged and sorted list of workers.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sort">Sort column identifier.</param>
    /// <param name="ascending">Determines sort order.</param>
    /// <returns>
    /// View containing a paged list of workers.
    /// </returns>
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            int pageSize = appSettings.PageSize;

            IQueryable<Worker> query = context.Workers
                                          .Include(w => w.Availability)
                                          .Include(w => w.Competences)
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

                var emptyModel = new PagedList<Worker>(
                    new List<Worker>(),
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

            var model = PagedList<Worker>.Create(query, pagingInfo);

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while loading the worker index page.");
            TempData["Message"] = "An error occurred while loading the worker index page.";
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
    }

    /// <summary>
    /// Displays the worker creation form.
    /// </summary>
    /// <returns>
    /// View for creating a new worker.
    /// </returns>
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.AvailabilityList = new SelectList(
            context.Availabilities.AsNoTracking(),
            "AvailabilityId",
            "WorkHours" 
        );
        return View();
    }

    /// <summary>
    /// Creates a new worker.
    /// </summary>
    /// <param name="worker">Worker entity populated from the form.</param>
    /// <returns>
    /// Redirects to index view on success or redisplays the form on validation error.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Worker worker)
    {
        logger.LogTrace(JsonSerializer.Serialize(worker));

        if (ModelState.IsValid)
        {
            try
            {
                context.Add(worker);
                context.SaveChanges();

                string message = $"Worker #{worker.WorkerId} added.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding a new worker.");
                TempData["Message"] = "An error occurred while adding a new worker.";
                TempData["MessageType"] = "error";

                ViewBag.AvailabilityList = new SelectList(
                    context.Availabilities.AsNoTracking(),
                    "AvailabilityId",
                    "WorkHours"
                );

                return View(worker);
            }
        }

        ViewBag.AvailabilityList = new SelectList(
            context.Availabilities.AsNoTracking(),
            "AvailabilityId",
            "WorkHours"
        );

        return View(worker);
    }

    /// <summary>
    /// Deletes a worker by identifier.
    /// </summary>
    /// <param name="workerID">Identifier of the worker to delete.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="sort">Current sort column.</param>
    /// <param name="ascending">Current sort order.</param>
    /// <returns>
    /// Redirects to the index view.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int workerID, int page = 1, int sort = 1, bool ascending = true)
    {
        var worker = context.Workers.Find(workerID);
        if (worker != null)
        {
            try
            {
                context.Remove(worker);
                context.SaveChanges();
                string message = $"Worker #{worker.WorkerId} deleted";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                string message = "An error occurred while deleting a worker.";
                logger.LogError(ex, message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
            }
        }
        else
        {
            string message = $"There is no worker with ID {workerID}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
        }
        return RedirectToAction(nameof(Index), new { page, sort, ascending });
    }

    /// <summary>
    /// Displays the worker edit form.
    /// </summary>
    /// <param name="id">Worker identifier.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="sort">Current sort column.</param>
    /// <param name="ascending">Current sort order.</param>
    /// <returns>
    /// Edit view for the specified worker or 404 if not found.
    /// </returns>
    [HttpGet]
    public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var worker = context.Workers
            .AsNoTracking()
            .SingleOrDefault(w => w.WorkerId == id);

        if (worker == null)
        {
            string message = $"There is no worker with ID {id}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
            return NotFound(message);
        }

        ViewBag.AvailabilityList = new SelectList(
            context.Availabilities.AsNoTracking(),
            "AvailabilityId",
            "WorkHours"
        );

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        return View(worker);
    }

    /// <summary>
    /// Updates an existing worker.
    /// </summary>
    /// <param name="id">Worker identifier.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="sort">Current sort column.</param>
    /// <param name="ascending">Current sort order.</param>
    /// <returns>
    /// Redirects to index view on success or redisplays the edit form on error.
    /// </returns>
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            Worker? worker = await context.Workers.FindAsync(id);
            if (worker == null)
            {
                string message = $"Invalid worker ID {id}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound("Invalid worker ID: " + id);
            }

            if (await TryUpdateModelAsync<Worker>(
                worker,
                "",
                w => w.Name,
                w => w.Surname,
                w => w.Email,
                w => w.Phone,
                w => w.AvailabilityId
            ))
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                try
                {
                    await context.SaveChangesAsync();
                    string message = $"Worker #{id} updated.";
                    logger.LogInformation(message);
                    TempData["Message"] = message;
                    TempData["MessageType"] = "success";

                    return RedirectToAction(nameof(Index), new { page, sort, ascending });

                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message);
                    TempData["Message"] = ex.Message;
                    TempData["MessageType"] = "error";

                    ViewBag.AvailabilityList = new SelectList(
                        context.Availabilities.AsNoTracking(),
                        "AvailabilityId",
                        "WorkHours"
                    );

                    return View(worker);
                }
            }
            else
            {
                logger.LogWarning("Cannot update worker.");

                ViewBag.AvailabilityList = new SelectList(
                    context.Availabilities.AsNoTracking(),
                    "AvailabilityId",
                    "WorkHours"
                );

                return View(worker);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Edit), id);
        }
    }

    /// <summary>
    /// Displays master-detail view of a worker and their competences.
    /// </summary>
    /// <param name="id">Worker identifier.</param>
    /// <returns>
    /// Master-detail view for the specified worker.
    /// </returns>
    [HttpGet]
    public IActionResult MasterDetail(int id)
    {
        try
        {
            var worker = context.Workers
                .Include(w => w.Competences)
                .Include(w => w.Availability)
                .SingleOrDefault(w => w.WorkerId == id);

            if (worker == null)
            {
                string message = $"Invalid worker ID {id} while loading master-detail";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound("Invalid worker ID: " + id);
            }

            var model = new WorkerCompetenceMasterDetail
            {
                Worker = worker
            };

            ReloadMasterDetailLists(model);

            ViewBag.CertificateList = context.Certificates
                .Select(c => new SelectListItem
                {
                    Value = c.CertificateId.ToString(),
                    Text = c.CertificateName
                })
                .ToList();

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

    /// <summary>
    /// Updates worker and competence data from master-detail form.
    /// </summary>
    /// <param name="model">Master-detail view model containing worker and competences.</param>
    /// <returns>
    /// Redirects back to the master-detail view.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MasterDetailEdit(WorkerCompetenceMasterDetail model)
    {
        try
        {
            if (!ModelState.IsValid)
            {

                ReloadMasterDetailLists(model);

                ViewBag.CertificateList = context.Certificates
                    .Select(c => new SelectListItem
                    {
                        Value = c.CertificateId.ToString(),
                        Text = c.CertificateName
                    })
                    .ToList();
                return View("MasterDetail", model);
            }

            Worker? worker = context.Workers.Find(model.Worker.WorkerId);
            if (worker == null)
            {
                string message = $"Invalid worker ID {model.Worker.WorkerId}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound("Invalid worker ID: " + model.Worker.WorkerId);
            }

            UpdateWorker(model.Worker);

            return RedirectToAction(nameof(MasterDetail),
                new { id = model.Worker.WorkerId });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(MasterDetail), model.Worker.WorkerId);
        }
    }

    private void UpdateWorker(Worker source)
    {
        try
        {
            var worker = context.Workers.Find(source.WorkerId);
            if (worker == null)
            {
                string message = $"Invalid worker ID {source.WorkerId}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
            }
            else
            {
                worker.Name = source.Name;
                worker.Surname = source.Surname;
                worker.Email = source.Email;
                worker.Phone = source.Phone;
                worker.AvailabilityId = source.AvailabilityId;

                context.SaveChanges();

                string message = $"Updated worker #{source.WorkerId}";
                logger.LogInformation(message);
                TempData["Message"] = "Updated worker";
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

    /// <summary>
    /// Saves batch changes to worker competences.
    /// Handles creation, update and deletion of competences.
    /// </summary>
    /// <param name="model">Batch edit model containing competence rows.</param>
    /// <returns>
    /// Redirects to master-detail view after processing.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveCompetencesBatch(CompetenceBatchEdit model)
    {
        try
        {
            if(model.Competences == null)
            {
                model.Competences = new List<CompetenceBatchRow>();
            }

            for (int i = 0; i < model.Competences.Count; i++)
            {
                if (model.Competences[i].IsDeleted)
                {
                    ModelState.Remove($"Competences[{i}].Name");
                    ModelState.Remove($"Competences[{i}].ExpiryDate");
                    ModelState.Remove($"Competences[{i}].WorkerId");
                }
            }

            if (!ModelState.IsValid)
            {
                Worker worker = context.Workers
                    .Include(w => w.Availability)
                    .Single(w => w.WorkerId == model.WorkerId);

                var masterModel = new WorkerCompetenceMasterDetail
                {
                    Worker = worker,
                    Competences = model.Competences
                };

                masterModel.AvailabilityList = context.Availabilities
                    .Select(a => new SelectListItem
                    {
                        Value = a.AvailabilityId.ToString(),
                        Text = a.WorkHours.ToString("yyyy-MM-dd HH:mm")
                    })
                    .ToList();

                ViewBag.CertificateList = context.Certificates
                    .Select(c => new SelectListItem
                    {
                        Value = c.CertificateId.ToString(),
                        Text = c.CertificateName
                    })
                    .ToList();

                return View("MasterDetail", masterModel);
            }

            foreach (var row in model.Competences)
            {
                if (row.CompetenceId == 0 && !row.IsDeleted)
                {
                    var newCompetence = new Competence
                    {
                        WorkerId = model.WorkerId,
                        Name = row.Name,
                        Description = row.Description,
                        ExpiryDate = row.ExpiryDate,
                        CertificateId = row.CertificateId
                    };

                    context.Competences.Add(newCompetence);
                    string message = $"Added new competence (master-detail)";
                    logger.LogInformation(message);
                    continue;
                }

                var db = context.Competences
                    .SingleOrDefault(c => c.CompetenceId == row.CompetenceId);

                if (db == null) continue;

                if (row.IsDeleted)
                {
                    context.Competences.Remove(db);
                    string message = $"Delted competence #{row.CompetenceId} (master-detail)";
                    logger.LogInformation(message);
                    continue;
                }

                db.Name = row.Name;
                db.Description = row.Description;
                db.ExpiryDate = row.ExpiryDate;
                db.CertificateId = row.CertificateId;
            }

            context.SaveChanges();

            TempData["Message"] = "Updated competences";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(MasterDetail),
                new { id = model.WorkerId });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            TempData["Message"] = ex.Message;
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(MasterDetail), new { id = model.WorkerId });
        }
    }

    private void ReloadMasterDetailLists(WorkerCompetenceMasterDetail model)
    {
        model.AvailabilityList = context.Availabilities
            .Select(a => new SelectListItem
            {
                Value = a.AvailabilityId.ToString(),
                Text = a.WorkHours.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        model.Competences = context.Competences
            .Where(c => c.WorkerId == model.Worker.WorkerId)
            .Select(c => new CompetenceBatchRow
            {
                CompetenceId = c.CompetenceId,
                WorkerId = c.WorkerId,
                Name = c.Name,
                Description = c.Description,
                ExpiryDate = c.ExpiryDate,
                CertificateId = c.CertificateId,
                IsDeleted = false
            })
            .ToList();
    }

    /// <summary>
    /// Generates a master-detail PDF report for all workers and their competences.
    /// </summary>
    /// <returns>
    /// PDF file containing worker competence report.
    /// </returns>
    [HttpGet]
    public IActionResult MasterDetailPdfAll()
    {
        var workers = context.Workers
            .Include(w => w.Availability)
            .Include(w => w.Competences)
                .ThenInclude(c => c.Certificate)
            .AsNoTracking()
            .OrderBy(w => w.Surname)
            .ThenBy(w => w.Name)
            .ToList();

        if (workers.Count == 0)
            return NotFound("No workers found.");

        var models = workers.Select(worker => new WorkerPdfModel
        {
            WorkerId = worker.WorkerId,
            Name = worker.Name,
            Surname = worker.Surname,
            Email = worker.Email,
            Phone = worker.Phone,
            Availability = worker.Availability?.WorkHours
                .ToString() ?? "-",

            Competences = worker.Competences.Select(c => new WorkerCompetencePdfRow
            {
                Name = c.Name,
                Description = c.Description,
                ExpiryDate = c.ExpiryDate,
                Certificate = c.Certificate?.CertificateName ?? "-"
            }).ToList()
        }).ToList();

        var pdf = new WorkerMasterDetailPdf(
            models,
            $"Workers – competence report"
        );

        byte[] bytes = pdf.GeneratePdf();

        Response.Headers.Append(
            "content-disposition",
            "inline; filename=workers_master_detail.pdf"
        );

        return File(bytes, "application/pdf");
    }

    /// <summary>
    /// Generates a master-detail Excel report for all workers and their competences.
    /// Each worker is exported to a separate worksheet.
    /// </summary>
    /// <returns>
    /// Excel file containing worker competence data.
    /// </returns>
    [HttpGet]
    public IActionResult MasterDetailExcelAll()
    {
        var workers = context.Workers
            .Include(w => w.Availability)
            .Include(w => w.Competences)
                .ThenInclude(c => c.Certificate)
            .AsNoTracking()
            .OrderBy(w => w.Surname)
            .ThenBy(w => w.Name)
            .ToList();

        if (!workers.Any())
            return NotFound("No workers.");

        using var workbook = new XLWorkbook();

        foreach (var worker in workers)
        {
            var sheetName = $"{worker.Surname}_{worker.Name}_{worker.WorkerId}";
            if (sheetName.Length > 31)
                sheetName = sheetName[..31];

            var ws = workbook.Worksheets.Add(sheetName);
            int row = 1;

            ws.Cell(row, 1).Value = "WORKER";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = worker.Name;
            row++;

            ws.Cell(row, 1).Value = "Surname";
            ws.Cell(row, 2).Value = worker.Surname;
            row++;

            ws.Cell(row, 1).Value = "Email";
            ws.Cell(row, 2).Value = worker.Email;
            row++;

            ws.Cell(row, 1).Value = "Phone number";
            ws.Cell(row, 2).Value = worker.Phone;
            row++;

            ws.Cell(row, 1).Value = "Avaibility";
            ws.Cell(row, 2).Value =
                worker.Availability?.WorkHours.ToString() ?? "-";

            row += 2;

            ws.Cell(row, 1).Value = "COMPETENCES";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;

            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = "Description";
            ws.Cell(row, 3).Value = "Expiry date";
            ws.Cell(row, 4).Value = "Certificate";

            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;

            foreach (var c in worker.Competences)
            {
                ws.Cell(row, 1).Value = c.Name;
                ws.Cell(row, 2).Value = c.Description;
                ws.Cell(row, 3).Value = 
                    c.ExpiryDate.HasValue
                        ? c.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue)
                        : "-";
                ws.Cell(row, 4).Value =
                    c.Certificate?.CertificateName ?? "-";
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
            "workers_master_detail.xlsx"
        );
    }

}