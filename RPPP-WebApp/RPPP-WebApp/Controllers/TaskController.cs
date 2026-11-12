using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVC_EN.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
namespace RPPP_WebApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly Rppp08Context _context;
        private readonly ILogger<TaskController> _logger;
        private readonly AppSettings _appSettings;

        public TaskController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<TaskController> logger)
        {
            _context = context;
            _logger = logger;
            _appSettings = options.Value;
        }

        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                int pageSize = _appSettings.PageSize;

                var query = _context.Tasks
                    .Include(t => t.Enclosure)
                    .Include(t => t.TaskType)
                    .Include(t => t.TaskStatus)
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

                int totalPages = pagingInfo.TotalPages == 0 ? 1 : pagingInfo.TotalPages;
                if (page < 1 || page > totalPages)
                    return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });

                query = query.ApplySort(sort, ascending);

                var tasks = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TaskListViewModel
                    {
                        TaskID = t.TaskID,
                        TimeStart = t.TimeStart,
                        TimeEnd = t.TimeEnd,
                        EnclosureName = t.Enclosure.Name,
                        TypeName = t.TaskType.Name,
                        StatusName = t.TaskStatus.Status
                    })
                    .ToList();

                return View(new TaskIndexViewModel

                {
                    Tasks = tasks,
                    PagingInfo = pagingInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Task index.");
                TempData["Message"] = "Error loading tasks.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }
        }

        // GET
        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name");
            ViewData["TypeID"] = new SelectList(_context.TaskTypes, "TaskTypeID", "Name");
            ViewData["StatusID"] = new SelectList(_context.TaskStatuses, "TaskStatusID", "Status");
            ViewData["page"] = page;
            ViewData["sort"] = sort;
            ViewData["ascending"] = ascending;
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RPPP_WebApp.Model.Task task, int page = 1, int sort = 1, bool ascending = true)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name", task.EnclosureID);
                ViewData["TypeID"] = new SelectList(_context.TaskTypes, "TaskTypeID", "Name", task.TypeID);
                ViewData["StatusID"] = new SelectList(_context.TaskStatuses, "TaskStatusID", "Status", task.StatusID);
                return View(task);
            }

            _context.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} created. Enclosure={EnclosureId}, Type={TypeId}, Status={StatusId}",
                task.TaskID,
                task.EnclosureID,
                task.TypeID,
                task.StatusID
            );
            TempData["Message"] = "Task created.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        // GET
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name", task.EnclosureID);
            ViewData["TypeID"] = new SelectList(_context.TaskTypes, "TaskTypeID", "Name", task.TypeID);
            ViewData["StatusID"] = new SelectList(_context.TaskStatuses, "TaskStatusID", "Status", task.StatusID);
            ViewData["page"] = page;
            ViewData["sort"] = sort;
            ViewData["ascending"] = ascending;
            // ViewData["my_key"] = "tvoja_staraaaaaaaaaa";

            return View(task);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RPPP_WebApp.Model.Task task, int page = 1, int sort = 1, bool ascending = true)
        {
            if (id != task.TaskID) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["EnclosureID"] = new SelectList(_context.Enclosures, "EnclosureID", "Name", task.EnclosureID);
                ViewData["TypeID"] = new SelectList(_context.TaskTypes, "TaskTypeID", "Name", task.TypeID);
                ViewData["StatusID"] = new SelectList(_context.TaskStatuses, "TaskStatusID", "Status", task.StatusID);
                return View(task);
            }

            _context.Update(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Task {TaskId} updated.",
                task.TaskID
            );
            TempData["Message"] = "Task updated.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogWarning(
                "Task {TaskId} deleted.",
                id
            );

            TempData["Message"] = "Task deleted.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Index), new { page, sort, ascending });
        }

        public async Task<IActionResult> ExportAllTasksPdf()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Enclosure)
                .Include(t => t.TaskType)
                .Include(t => t.TaskStatus)
                .Include(t => t.Expenses!)
                .ThenInclude(e => e.Price)
                .OrderByDescending(t => t.TimeStart)
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
                        .Text("Full Task Log")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        foreach (var task in tasks)
                        {
                            column.Item().ShowEntire().Column(section =>
                            {
                                section.Item().Text(text =>
                                {
                                    text.Span($"Task #{task.TaskID}").Bold().FontSize(16);
                                    text.Span($" | {task.TimeStart:yyyy-MM-dd} – {task.TimeEnd:yyyy-MM-dd}")
                                        .FontSize(14);
                                });

                                section.Item().PaddingBottom(5).Text(
                                    $"Enclosure: {task.Enclosure?.Name ?? "-"} | " +
                                    $"Type: {task.TaskType?.Name ?? "-"} | " +
                                    $"Status: {task.TaskStatus?.Status ?? "-"}")
                                    .FontColor(Colors.Grey.Darken2);

                                if (task.Expenses != null && task.Expenses.Any())
                                {
                                    section.Item().PaddingBottom(20).Table(table =>
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
                                            header.Cell().Element(CellStyle).Text("Type");
                                            header.Cell().Element(CellStyle).Text("Description");
                                            header.Cell().Element(CellStyle).Text("Price");
                                        });

                                        foreach (var e in task.Expenses)
                                        {
                                            table.Cell().Element(CellStyleContent).Text(e.Id.ToString());
                                            table.Cell().Element(CellStyleContent).Text(e.Type ?? "-");
                                            table.Cell().Element(CellStyleContent).Text(e.Description ?? "-");
                                            table.Cell().Element(CellStyleContent)
                                                .Text(e.Price?.ItemName ?? "-");
                                        }
                                    });
                                }
                                else
                                {
                                    section.Item().PaddingBottom(20)
                                        .Text("No expenses listed.")
                                        .Italic().FontColor(Colors.Grey.Medium);
                                }

                                section.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                section.Item().PaddingBottom(20);
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", "All_Tasks.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(int taskId)
        {
            _context.Expenses.Add(new Expense
            {
                TaskId = taskId
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MasterDetail),
                new { id = taskId });
        }

        static IContainer CellStyle(IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold())
                .PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Black);

        static IContainer CellStyleContent(IContainer container) =>
            container.PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3);

        public async Task<IActionResult> ExportToExcel()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Enclosure)
                .Include(t => t.TaskType)
                .Include(t => t.TaskStatus)
                .Include(t => t.Expenses!)
                .ThenInclude(e => e.Price)
                .OrderByDescending(t => t.TimeStart)
                .AsNoTracking()
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                foreach (var task in tasks)
                {
                    string sheetName = $"Task #{task.TaskID}";
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    worksheet.Cell(1, 1).Value = "Task Details";
                    worksheet.Range(1, 1, 1, 4).Merge()
                            .Style.Font.SetBold().Font.SetFontSize(14);

                    worksheet.Cell(3, 1).Value = "Task ID:";
                    worksheet.Cell(3, 2).Value = task.TaskID;

                    worksheet.Cell(4, 1).Value = "Start:";
                    worksheet.Cell(4, 2).Value = task.TimeStart.HasValue ? task.TimeStart.Value.ToString("yyyy-MM-dd") : "N/A";

                    worksheet.Cell(5, 1).Value = "End:";
                    worksheet.Cell(5, 2).Value = task.TimeEnd.HasValue ? task.TimeEnd.Value.ToString("yyyy-MM-dd") : "N/A";

                    worksheet.Cell(6, 1).Value = "Enclosure:";
                    worksheet.Cell(6, 2).Value = task.Enclosure?.Name ?? "-";

                    worksheet.Cell(7, 1).Value = "Type:";
                    worksheet.Cell(7, 2).Value = task.TaskType?.Name ?? "-";

                    worksheet.Cell(8, 1).Value = "Status:";
                    worksheet.Cell(8, 2).Value = task.TaskStatus?.Status ?? "-";

                    worksheet.Range(3, 1, 8, 1).Style.Font.SetBold();

                    int row = 10;

                    worksheet.Cell(row, 1).Value = "ID";
                    worksheet.Cell(row, 2).Value = "Type";
                    worksheet.Cell(row, 3).Value = "Description";
                    worksheet.Cell(row, 4).Value = "Price";

                    var header = worksheet.Range(row, 1, row, 4);
                    header.Style.Font.SetBold();
                    header.Style.Fill.BackgroundColor = XLColor.LightGray;
                    header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                    row++;

                    if (task.Expenses != null && task.Expenses.Any())
                    {
                        foreach (var e in task.Expenses)
                        {
                            worksheet.Cell(row, 1).Value = e.Id;
                            worksheet.Cell(row, 2).Value = e.Type ?? "-";
                            worksheet.Cell(row, 3).Value = e.Description ?? "-";
                            worksheet.Cell(row, 4).Value = e.Price?.ItemName ?? "-";
                            row++;
                        }
                    }
                    else
                    {
                        worksheet.Cell(row, 1).Value = "No expenses recorded.";
                        worksheet.Range(row, 1, row, 4).Merge()
                                .Style.Font.SetItalic();
                    }

                    worksheet.Columns().AdjustToContents();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Tasks_MasterDetail.xlsx");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExpensesBatch(TaskDetailsViewModel model)
        {
            var task = await _context.Tasks.FindAsync(model.Task.TaskID);
            if (task == null)
                return NotFound();

            // MASTER updates
            task.TimeStart = model.Task.TimeStart;
            task.TimeEnd   = model.Task.TimeEnd;
            task.Notes     = model.Task.Notes;

            if (model.Task.EnclosureID > 0)
                task.EnclosureID = model.Task.EnclosureID;

            if (model.Task.TypeID > 0)
                task.TypeID = model.Task.TypeID;

            if (model.Task.StatusID > 0)
                task.StatusID = model.Task.StatusID;

            if (model?.Expenses == null)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MasterDetail), new { id = model.Task.TaskID });
            }

            foreach (var item in model.Expenses)
            {
                _logger.LogInformation("Processing expense {ExpenseId}, IsDeleted={IsDeleted}", 
                    item.ExpenseID, item.IsDeleted);
                
                if (item.IsDeleted)
                {
                    var expenseToDelete = await _context.Expenses.FindAsync(item.ExpenseID);
                    if (expenseToDelete != null)
                    {
                        _context.Expenses.Remove(expenseToDelete);
                        _logger.LogWarning(
                            "Expense {ExpenseId} deleted from Task {TaskId}.",
                            item.ExpenseID,
                            model.Task.TaskID
                        );
                    }
                    continue;
                }

                var dbItem = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == item.ExpenseID);

                _logger.LogInformation("Found dbItem: {Found}, ExpenseId: {DbId}", 
                    dbItem != null, dbItem?.Id);
                
                if (dbItem == null)
                {
                    _logger.LogWarning("Expense {ExpenseId} not found in database", item.ExpenseID);
                    continue;
                }

                _logger.LogInformation("Updating - Old: Type={OldType}, Desc={OldDesc} | New: Type={NewType}, Desc={NewDesc}", 
                    dbItem.Type, dbItem.Description, item.Type, item.Description);

                dbItem.Type = item.Type;
                dbItem.Description = item.Description;
                dbItem.ArrivalDate = item.ArrivalDate;

                if (!string.IsNullOrWhiteSpace(item.PriceName))
                {
                    var price = await _context.Prices
                        .FirstOrDefaultAsync(p => p.ItemName == item.PriceName);

                    if (price != null)
                    {
                        _logger.LogInformation("Updating PriceId from {OldPrice} to {NewPrice}", 
                            dbItem.PriceId, price.PriceID);
                        dbItem.PriceId = price.PriceID;
                    }
                }
            }

            _logger.LogInformation("About to save changes...");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Changes saved successfully");

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} and related expenses updated.",
                model.Task.TaskID
            );

            return RedirectToAction(nameof(MasterDetail),
                new { id = model.Task.TaskID });
        }




        public IActionResult MasterDetail(int id)
        {
            try
            {
                var model = _context.Tasks
                    .AsNoTracking()
                    .Where(t => t.TaskID == id)
                    .Select(t => new TaskDetailsViewModel
                    {
                        Task = new TaskViewModel
                        {
                            TaskID = t.TaskID,
                            TimeStart = t.TimeStart,
                            TimeEnd = t.TimeEnd,
                            Notes = t.Notes,
                            EnclosureName = t.Enclosure.Name,
                            TypeName = t.TaskType.Name,
                            StatusName = t.TaskStatus.Status
                        },
                        Expenses = t.Expenses.Select(e => new ExpenseViewModel
                        {
                            ExpenseID = e.Id,
                            TaskId = t.TaskID,
                            Type = e.Type,
                            ArrivalDate = e.ArrivalDate,
                            Description = e.Description,
                            PriceId = e.PriceId,
                            PriceName = e.Price != null ? e.Price.ItemName : null
                        }).ToList()
                    })
                    .FirstOrDefault();

                if (model == null) return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Task master-detail.");
                TempData["Message"] = "Error loading task details.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }


        
    }
}
