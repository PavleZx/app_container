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

namespace RPPP_WebApp.Controllers;

public class CompetenceController : Controller
{
    private readonly Rppp08Context context;
    private readonly ILogger<CompetenceController> logger;
    private readonly AppSettings appSettings;

    public CompetenceController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<CompetenceController> logger)
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

            IQueryable<Competence> query = context.Competences
                                          .Include(w => w.Certificate)
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

                var emptyModel = new PagedList<Competence>(
                    new List<Competence>(),
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

            var model = PagedList<Competence>.Create(query, pagingInfo);

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while loading the competence index page.");
            TempData["Message"] = "An error occurred while loading the competence index page.";
            TempData["MessageType"] = "error";
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.CertificateList = new SelectList(
            context.Certificates.AsNoTracking(),
            "CertificateId",
            "CertificateName"
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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Competence competence)
    {
        logger.LogTrace(JsonSerializer.Serialize(competence));

        if (ModelState.IsValid)
        {
            try
            {
                context.Add(competence);
                context.SaveChanges();

                string message = $"New competence added.";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding a new competence.");
                TempData["Message"] = "An error occurred while adding a new competence.";
                TempData["MessageType"] = "error";

                ViewBag.CertificateList = new SelectList(
                    context.Certificates.AsNoTracking(),
                    "CertificateId",
                    "CertificateName"
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
                return View(competence);
            }
        }

        ViewBag.CertificateList = new SelectList(
            context.Certificates.AsNoTracking(),
            "CertificateId",
            "CertificateName"
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
        return View(competence);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int competenceID, int page = 1, int sort = 1, bool ascending = true)
    {
        var competence = context.Competences.Find(competenceID);
        if (competence != null)
        {
            try
            {
                string name = competence.Name;
                context.Remove(competence);
                context.SaveChanges();
                string message = $"Competence {competence.CompetenceId} deleted";
                logger.LogInformation(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                string message = "An error occurred while deleting a competence.";
                logger.LogError(ex, message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
            }
        }
        else
        {
            string message = $"There is no competence with ID {competenceID}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
        }
        return RedirectToAction(nameof(Index), new { page, sort, ascending });
    }

    [HttpGet]
    public IActionResult Edit(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        var competence = context.Competences
            .AsNoTracking()
            .SingleOrDefault(c => c.CompetenceId == id);

        if (competence == null)
        {
            string message = $"There is no competence with ID {id}";
            logger.LogWarning(message);
            TempData["Message"] = message;
            TempData["MessageType"] = "error";
            return NotFound(message);
        }

        ViewBag.CertificateList = new SelectList(
            context.Certificates.AsNoTracking(),
            "CertificateId",
            "CertificateName"
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

        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;

        return View(competence);
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            Competence? competence = await context.Competences.FindAsync(id);
            if (competence == null)
            {
                string message = $"Invalid competence ID {id}";
                logger.LogWarning(message);
                TempData["Message"] = message;
                TempData["MessageType"] = "error";
                return NotFound("Invalid competence ID: " + id);
            }

            if (await TryUpdateModelAsync<Competence>(
                competence,
                "",
                c => c.Name,
                c => c.Description,
                c => c.ExpiryDate,
                c => c.CertificateId,
                c => c.WorkerId
            ))
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                try
                {
                    await context.SaveChangesAsync();
                    string message = $"Competence #{id} updated.";
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

                    ViewBag.CertificateList = new SelectList(
                        context.Certificates.AsNoTracking(),
                        "CertificateId",
                        "CertificateName"
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

                    return View(competence);
                }
            }
            else
            {
                logger.LogWarning("Cannot update worker.");

                ViewBag.CertificateList = new SelectList(
                    context.Certificates.AsNoTracking(),
                    "CertificateId",
                    "CertificateName"
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

                return View(competence);
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
}