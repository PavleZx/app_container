using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors; 
using RPPP_WebApp.Model;              
using RPPP_WebApp.ViewModels;           
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// MVC controller responsible for managing Incident Reports.
    /// Provides CRUD operations, master-detail editing of affected items (Affects),
    /// and export of reports to PDF and Excel formats.
    /// </summary>
    /// <remarks>
    /// This controller handles:
    /// <list type="bullet">
    /// <item>Paged and sorted listing of incident reports</item>
    /// <item>Creation, editing, and deletion of reports</item>
    /// <item>Master-detail view for managing items affected by the incident</item>
    /// <item>Batch updating of affected items details</item>
    /// <item>Generation of full log PDF and master-detail Excel reports</item>
    /// </list>
    /// </remarks>
    public class IncidentReportController : Controller
    {
        private readonly Rppp08Context context;
        private readonly ILogger<IncidentReportController> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentReportController"/> class.
        /// </summary>
        /// <param name="context">Database context used for data access.</param>
        /// <param name="options">Application configuration settings (e.g., page size).</param>
        /// <param name="logger">Logger instance for diagnostic messages.</param>
        public IncidentReportController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<IncidentReportController> logger)
        {
            this.context = context;
            this.logger = logger;
            appSettings = options.Value;
        }
        /// <summary>
        /// Displays a paged and sorted list of incident reports.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="sort">Sort column identifier.</param>
        /// <param name="ascending">Determines sort order (true for ascending).</param>
        /// <returns>View containing a paged list of incident reports.</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                int pageSize = appSettings.PageSize;
                IQueryable<IncidentReport> query = context.IncidentReports
                    .Include(r => r.IncidentType)
                    .AsNoTracking();

                PagingInfo pagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    Sort = sort,
                    Ascending = ascending,
                    ItemsPerPage = pageSize,
                    TotalItems = query.Count()
                };

                if (pagingInfo.TotalItems > 0 && (page < 1 || page > pagingInfo.TotalPages))
                {
                    return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
                }

                query = query.ApplySort(sort, ascending);
                var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var model = new PagedList<IncidentReport>(items, pagingInfo);

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Index error");
                TempData["Message"] = "Error loading index.";
                return View(new PagedList<IncidentReport>(new List<IncidentReport>(), new PagingInfo()));
            }
        }
        /// <summary>
        /// Displays the form for creating a new incident report.
        /// </summary>
        /// <returns>View for creating a new report.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            PrepareDropDownLists();
            return View();
        }
        /// <summary>
        /// Creates a new incident report based on form data.
        /// </summary>
        /// <param name="incidentReport">The incident report entity to create.</param>
        /// <returns>Redirects to Index on success, or redisplays form on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IncidentReport incidentReport)
        {
            ModelState.Remove("IncidentType");
            if (ModelState.IsValid)
            {
                try
                {
                    int nextId = context.IncidentReports.Any() ? context.IncidentReports.Max(r => r.ReportID) + 1 : 1;
                    incidentReport.ReportID = nextId;
                    context.Add(incidentReport);
                    context.SaveChanges();
                    TempData["Message"] = $"Report #{nextId} added.";
                    TempData["MessageType"] = "success";
                    logger.LogInformation($"Created new Incident Report #{nextId}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Error: " + ex.Message;
                    logger.LogError(ex, "Error creating report");
                }
            }
            PrepareDropDownLists();
            return View(incidentReport);
        }
        /// <summary>
        /// Deletes an incident report by its identifier.
        /// </summary>
        /// <param name="reportID">The ID of the report to delete.</param>
        /// <param name="page">Current page number (to maintain state).</param>
        /// <param name="sort">Current sort column (to maintain state).</param>
        /// <param name="ascending">Current sort order (to maintain state).</param>
        /// <returns>Redirects to the Index view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int reportID, int page = 1, int sort = 1, bool ascending = true)
        {
            var report = context.IncidentReports.Find(reportID);
            if (report != null)
            {
                try
                {
                    context.Remove(report);
                    context.SaveChanges();
                    logger.LogInformation($"Deleted Incident Report #{reportID}");
                    TempData["Message"] = $"Report #{reportID} deleted.";
                    TempData["MessageType"] = "success";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error deleting report #{reportID}");
                    TempData["Message"] = "Error deleting: " + ex.Message;
                    TempData["MessageType"] = "error";
                }
            }
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }
        /// <summary>
        /// Displays the Master-Detail view for a specific report and its affected items.
        /// </summary>
        /// <param name="id">Report identifier.</param>
        /// <param name="sort">Sort order for the details list.</param>
        /// <param name="ascending">Sort direction for the details list.</param>
        /// <returns>View model containing the report and list of affected items.</returns>
        public async Task<IActionResult> MasterDetail(int id, int sort = 1, bool ascending = true)
        {
            var report = await context.IncidentReports
                .Include(r => r.IncidentType)
                .Include(r => r.Affects).ThenInclude(a => a.Enclosure)
                .Include(r => r.Affects).ThenInclude(a => a.IndividualAnimal)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ReportID == id);

            if (report == null) return NotFound();

            PrepareDropDownLists();

            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            if (report.Affects != null)
            {
                report.Affects = report.Affects.AsQueryable().ApplySort(sort, ascending).ToList();
            }

            var viewModel = new IncidentAffectsViewModel
            {
                Report = report,
                Affects = report.Affects.ToList()
            };

            return View(viewModel);
        }
        /// <summary>
        /// Displays the form to edit an existing incident report (Master data only).
        /// </summary>
        /// <param name="id">The ID of the report to edit.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="sort">Current sort column.</param>
        /// <param name="ascending">Current sort order.</param>
        /// <returns>Edit view for the report.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var incidentReport = await context.IncidentReports
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ReportID == id);

            if (incidentReport == null)
            {
                return NotFound();
            }

            PrepareDropDownLists();
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            return View(incidentReport);
        }
        /// <summary>
        /// Updates an existing incident report.
        /// </summary>
        /// <param name="id">The ID of the report.</param>
        /// <param name="incidentReport">The updated report entity.</param>
        /// <param name="returnUrl">URL to return to after update.</param>
        /// <returns>Redirects to Index or ReturnUrl on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Report")] IncidentReport incidentReport, string? returnUrl = null)
        {
            if (id != incidentReport.ReportID)
            {
                return NotFound();
            }

            ModelState.Remove("IncidentType");
            ModelState.Remove("Affects");

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(incidentReport);
                    await context.SaveChangesAsync();
                    logger.LogInformation($"Updated Incident Report #{id}");
                    TempData["Message"] = "Report updated successfully.";
                    TempData["MessageType"] = "success";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error updating report #{id}");
                    TempData["Message"] = "Error: " + ex.Message;
                    TempData["MessageType"] = "error";
                    PrepareDropDownLists();
                    return View(incidentReport);
                }
            }
            else
            {
                PrepareDropDownLists();
                return View(incidentReport);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Adds an empty 'Affects' item to a report in the Master-Detail view.
        /// Used for inline editing/creation of details.
        /// </summary>
        /// <param name="reportId">The ID of the parent report.</param>
        /// <returns>Redirects back to the MasterDetail view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmptyItem(int reportId)
        {
            int nextId = context.Affects.Any() ? context.Affects.Max(a => a.affectsID) + 1 : 1;

            var newAffect = new Affects
            {
                affectsID = nextId,
                reportID = reportId,
                description = "",
                enclosureID = null,
                animalID = null
            };

            try
            {
                context.Affects.Add(newAffect);
                await context.SaveChangesAsync();
                logger.LogInformation($"Added new empty Affects item #{nextId} to Report #{reportId}");
                TempData["Message"] = "New record added.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding empty item");
                TempData["Message"] = "Error adding record: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction(nameof(MasterDetail), new { id = reportId });
        }
        /// <summary>
        /// Saves changes to a batch of 'Affects' items associated with a report.
        /// Handles updates and deletions of multiple items at once.
        /// </summary>
        /// <param name="model">ViewModel containing the list of affected items.</param>
        /// <returns>Redirects back to the MasterDetail view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAffectsBatch(IncidentAffectsViewModel model)
        {
            foreach (var item in model.Affects)
            {
                if (item.IsDeleted)
                {
                    context.Affects.Remove(new Affects { affectsID = item.affectsID });
                    logger.LogInformation($"Deleted Affects item #{item.affectsID} in batch");
                    continue;
                }
                var dbItem = await context.Affects.FindAsync(item.affectsID);
                if (dbItem == null) continue;
                dbItem.description = item.description;
                dbItem.enclosureID = item.enclosureID;
                dbItem.animalID = item.animalID;
            }

            await context.SaveChangesAsync();
            logger.LogInformation($"Batch updated Affects list for Report #{model.Report.ReportID}");
            return RedirectToAction(nameof(MasterDetail), new { id = model.Report.ReportID });
        }
        /// <summary>
        /// Generates a PDF report containing a full log of all incident reports and their details.
        /// </summary>
        /// <returns>A PDF file download.</returns>
        public async Task<IActionResult> ExportAllReportsPdf()
        {
            var reports = await context.IncidentReports
                .Include(r => r.IncidentType)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.Enclosure)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.IndividualAnimal)
                .OrderByDescending(r => r.DateReport)
                .AsNoTracking()
                .ToListAsync();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    page.Header()
                        .Text("Full Incident Report Log")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        foreach (var report in reports)
                        {
                            column.Item().ShowEntire().Column(reportSection => 
                            {
                                reportSection.Item().Text(text =>
                                {
                                    text.Span($"Report #{report.ReportID}").Bold().FontSize(16);
                                    text.Span($" | Date: {report.DateReport?.ToShortDateString() ?? "-"}").FontSize(14);
                                });
                                var typeName = report.IncidentType?.incidentType ?? "Unknown Type"; 
                                reportSection.Item().PaddingBottom(5).Text($"Type: {typeName} | Status: {report.Status}")
                                    .FontColor(Colors.Grey.Darken2);
                                if (report.Affects != null && report.Affects.Any())
                                {
                                    reportSection.Item().PaddingBottom(20).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(40);
                                            columns.RelativeColumn();
                                            columns.RelativeColumn(); 
                                            columns.RelativeColumn(); 
                                        });
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("ID");
                                            header.Cell().Element(CellStyle).Text("Description");
                                            header.Cell().Element(CellStyle).Text("Enclosure");
                                            header.Cell().Element(CellStyle).Text("Animal");
                                        });
                                        foreach (var item in report.Affects)
                                        {
                                            table.Cell().Element(CellStyleContent).Text(item.affectsID.ToString());
                                            table.Cell().Element(CellStyleContent).Text(item.description ?? "-");
                                            var enclosureName = item.Enclosure?.Name ?? "---";
                                            table.Cell().Element(CellStyleContent).Text(enclosureName);

                                            var animalName = item.IndividualAnimal?.Nickname ?? "---";
                                            table.Cell().Element(CellStyleContent).Text(animalName);
                                        }
                                    });
                                }
                                else
                                {
                                    reportSection.Item().PaddingBottom(20).Text("No affected items listed.").Italic().FontColor(Colors.Grey.Medium);
                                }
                        
                                reportSection.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                reportSection.Item().PaddingBottom(20);
                            });
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;
            return File(stream, "application/pdf", "All_Incident_Reports.pdf");
        }

        static IContainer CellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold())
                .PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Black);
        }

        static IContainer CellStyleContent(IContainer container)
        {
            return container.PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3);
        }
        /// <summary>
        /// Exports all incident reports and their details to an Excel file.
        /// Each report is placed in a separate worksheet.
        /// </summary>
        /// <returns>An Excel file download.</returns>
        public async Task<IActionResult> ExportToExcel()
        {
            var reports = await context.IncidentReports
                .Include(r => r.IncidentType)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.Enclosure)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.IndividualAnimal)
                .OrderByDescending(r => r.DateReport)
                .AsNoTracking()
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                foreach (var report in reports)
                {
                    string sheetName = $"Report #{report.ReportID}";
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    worksheet.Cell(1, 1).Value = "Incident Report Details";
                    worksheet.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.SetFontSize(14);

                    worksheet.Cell(3, 1).Value = "Report ID:";
                    worksheet.Cell(3, 2).Value = report.ReportID;

                    worksheet.Cell(4, 1).Value = "Date Reported:";
                    worksheet.Cell(4, 2).Value = report.DateReport?.ToShortDateString() ?? "-";

                    worksheet.Cell(5, 1).Value = "Status:";
                    worksheet.Cell(5, 2).Value = report.Status;

                    worksheet.Cell(6, 1).Value = "Incident Type:";
                    worksheet.Cell(6, 2).Value = report.IncidentType?.incidentType ?? "Unknown";
                    worksheet.Range(3, 1, 6, 1).Style.Font.SetBold();

                    int row = 9;

                    worksheet.Cell(row, 1).Value = "ID";
                    worksheet.Cell(row, 2).Value = "Description";
                    worksheet.Cell(row, 3).Value = "Enclosure";
                    worksheet.Cell(row, 4).Value = "Animal";
                    
                    var headerRange = worksheet.Range(row, 1, row, 4);
                    headerRange.Style.Font.SetBold();
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                    row++;
                    if (report.Affects != null && report.Affects.Any())
                    {
                        foreach (var item in report.Affects)
                        {
                            worksheet.Cell(row, 1).Value = item.affectsID;
                            worksheet.Cell(row, 2).Value = item.description ?? "-";
                            string enclosureName = item.Enclosure?.Name ?? "---";
                            worksheet.Cell(row, 3).Value = enclosureName;

                            string animalName = item.IndividualAnimal?.Nickname ?? "---";
                            worksheet.Cell(row, 4).Value = animalName;

                            row++;
                        }
                    }
                    else
                    {
                        worksheet.Cell(row, 1).Value = "No affected items recorded.";
                        worksheet.Range(row, 1, row, 4).Merge().Style.Font.SetItalic();
                    }
                    worksheet.Columns().AdjustToContents();
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    
                    return File(content, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        "IncidentReports_MasterDetail.xlsx");
                }
            }
        }
        private void PrepareDropDownLists()
        {
            ViewBag.IncidentTypeList = new SelectList(context.IncidentTypes.AsNoTracking(), "IncidentTypeID", "incidentType");
            ViewBag.EnclosureList = new SelectList(context.Enclosures.AsNoTracking(), "EnclosureID", "Name");
            ViewBag.AnimalList = new SelectList(context.IndividualAnimals.AsNoTracking(), "AnimalID", "Nickname");
        }
    }
}