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
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace RPPP_WebApp.Controllers
{
    public class EnclosureController : Controller
    {
        private readonly Rppp08Context _context;
        private readonly ILogger<EnclosureController> _logger;
        private readonly AppSettings _appSettings;

        public EnclosureController(Rppp08Context context, IOptionsSnapshot<AppSettings> options, ILogger<EnclosureController> logger)
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

                var query = _context.Enclosures
                    .Include(c => c.IndividualAnimals) 
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

                var enclosures = query.Select(c => new EnclosuresViewModel
                {
                    EnclosureID = c.EnclosureID,
                    Name = c.Name,
                    Dimensions = c.Dimensions,
                    Change = c.Change,
                    Lighting = c.Lighting,
                    Shading = c.Shading,
                    LocationName = c.Location.LocationName,
                    AnimalTypes = c.AnimalType.AnimalTypes 
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                var model = new EnclosureViewModel
                {
                    Enclosures = enclosures,
                    PagingInfo = pagingInfo
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the enclosure index page.");
                TempData["Message"] = "An error occurred while retrieving the data. Please try again later.";
                TempData["MessageType"] = "error";

                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }
        }

        public IActionResult Create(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationName");
                ViewData["AnimalTypeID"] = new SelectList(_context.AnimalTypes, "AnimalTypeID", "AnimalTypes");
                ViewData["page"] = page;
                ViewData["sort"] = sort;
                ViewData["ascending"] = ascending;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while preparing the enclosure creation form.");
                TempData["Message"] = "An error occurred while preparing the form. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LocationID,AnimalTypeID,Name,Dimensions,Change,Lighting,Shading")] Enclosure enclosure, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {

                _context.Add(enclosure);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Created entity '{enclosure.Name}'.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });


            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while creating a new enclosure.");
                TempData["Message"] = "An error occurred while saving the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Create), new { page, sort, ascending });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new enclosure.");
                TempData["Message"] = "An error occurred while saving the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Create), new { page, sort, ascending });
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
                var enclosure = await _context.Enclosures.FindAsync(id);
                if (enclosure == null)
                {
                    return NotFound();
                }

                ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationName", enclosure.LocationID);
                ViewData["AnimalTypeID"] = new SelectList(_context.AnimalTypes, "AnimalTypeID", "AnimalTypes", enclosure.AnimalTypeID);
                ViewData["page"] = page;
                ViewData["sort"] = sort;
                ViewData["ascending"] = ascending;
                return View(enclosure);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the enclosure edit page.");
                TempData["Message"] = "An error occurred while preparing the edit form. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnclosureID,LocationID,AnimalTypeID,Name,Dimensions,Change,Lighting,Shading")] Enclosure enclosure, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                if (id != enclosure.EnclosureID)
                {
                    return NotFound();
                }

                _context.Update(enclosure);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Edited entity '{enclosure.Name}'.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });

            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                _logger.LogError(dbEx, "A database concurrency error occurred while editing the enclosure.");
                TempData["Message"] = "A concurrency error occurred while updating the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Edit), new { id, page, sort, ascending });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing the enclosure.");
                TempData["Message"] = "An error occurred while editing the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Edit), new { id, page, sort, ascending });
            }
        }
        private bool EnclosureExists(int id)
        {
            try
            {
                return _context.Enclosures.Any(e => e.EnclosureID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if the enclosure exists.");
                TempData["Message"] = "An error occurred while verifying the enclosure's existence. Please try again later.";
                TempData["MessageType"] = "error";
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var enclosure = await _context.Enclosures.FindAsync(id);
                if (enclosure == null)
                {
                    return NotFound();
                }

                _context.Enclosures.Remove(enclosure);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Deleted entity '{enclosure.Name}'.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while deleting the enclosure.");
                TempData["Message"] = "An error occurred while deleting the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the enclosure.");
                TempData["Message"] = "An error occurred while deleting the enclosure. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index), new { page, sort, ascending });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> removeAnimal(int animalId, int enclosureId){
            try
    {
        if (animalId == 0)
        {
            return RedirectToAction("MasterDetail", new { id = enclosureId });
        }

        var animal = await _context.IndividualAnimals
                                   .FirstOrDefaultAsync(a => a.AnimalID == animalId);

        if (animal == null)
        {
            _logger.LogWarning("Attempted to delete animal {AnimalId}, but it was not found.", animalId);
            return RedirectToAction("MasterDetail", new { id = enclosureId });
        }

        _context.IndividualAnimals.Remove(animal);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Animal removed successfully.";
        TempData["MessageType"] = "success";

        return RedirectToAction("MasterDetail", new { id = enclosureId });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while deleting an animal.");
        TempData["Message"] = "Error deleting animal.";
        TempData["MessageType"] = "error";

        return RedirectToAction("MasterDetail", new { id = enclosureId });
    }
        }
        [HttpGet]
        public async Task<IActionResult> MasterDetail(int id)
        {
                var enclosures = await _context.Enclosures.FindAsync(id);
    if (enclosures == null)
        return NotFound();
             var model = new EnclosureDetailViewModel
    {
        Enclosure = _context.Enclosures
            .Where(p => p.EnclosureID == id)
            .Select(p => new EnclosuresViewModel
            {
                 EnclosureID = p.EnclosureID,
                    Name = p.Name,
                    Dimensions =  p.Dimensions,
                    Change = p.Change,         
                    Lighting = p.Lighting,
                    Shading = p.Shading,
                    LocationName = p.Location.LocationName,
                    AnimalTypes = p.AnimalType.AnimalTypes
            }).FirstOrDefault(),

        IndividualAnimals = _context.IndividualAnimals
            .Where(p => p.EnclosureID == id)
            .Select(p => new IndividualAnimalsViewModel
            {

                        AnimalID = p.AnimalID,
                        SerialNum = p.SerialNum,
                        Nickname = p.Nickname,
                        Acquisition = p.Acquisition,
                        Status = p.Status,
                        AnimalName = p.AnimalName.CroatianName
            }).ToList() ?? new List<IndividualAnimalsViewModel>(),

        NewAnimals = new List<IndividualAnimalsViewModel>()
    };

    return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addAnimal(int enclosureId){
            try
            {
                    var newAnimal = new IndividualAnimals
    {
        EnclosureID = enclosureId,
        SerialNum = 0,
        Nickname = "",
        Acquisition = "",
        Status = "",
        NameID = null
    };

    _context.IndividualAnimals.Add(newAnimal);
    await _context.SaveChangesAsync(); 
            var model = new EnclosureDetailViewModel
            {
            Enclosure = _context.Enclosures.Where(a => a.EnclosureID == enclosureId).Select(p => new EnclosuresViewModel
            {
                 EnclosureID = p.EnclosureID,
                    Name = p.Name,
                    Dimensions =  p.Dimensions,
                    Change = p.Change,         
                    Lighting = p.Lighting,
                    Shading = p.Shading,
                    LocationName = p.Location.LocationName,
                    AnimalTypes = p.AnimalType.AnimalTypes
            }).FirstOrDefault(),
            IndividualAnimals = _context.IndividualAnimals
            .Where(a => a.EnclosureID == enclosureId)
            .Select(p => new IndividualAnimalsViewModel {
                AnimalID = p.AnimalID,
                        SerialNum = p.SerialNum,
                        Nickname = p.Nickname,
                        Acquisition = p.Acquisition,
                        Status = p.Status,
                        AnimalName = p.AnimalName.CroatianName})
            .ToList()
            };
            return View("MasterDetail", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding empty item");
                TempData["Message"] = "Error adding record: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction(nameof(MasterDetail), new {id = enclosureId});
    }
            [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> saveAnimal(int id, EnclosureDetailViewModel model){
        try
            {
                    if (model?.IndividualAnimals == null)
    {
        TempData["Message"] = "No animals to save.";
        TempData["MessageType"] = "error";
        return RedirectToAction(nameof(MasterDetail), new { id = model?.Enclosure?.EnclosureID ?? 0 });
    }
                foreach (var item in model.IndividualAnimals)
    {
        var dbItem = await _context.IndividualAnimals.Include(a => a.AnimalName)
        .FirstOrDefaultAsync(a => a.AnimalID == item.AnimalID);
        if (dbItem == null) continue;
        dbItem.SerialNum = item.SerialNum;
        dbItem.Nickname = item.Nickname;
        dbItem.Acquisition = item.Acquisition;
        dbItem.Status = item.Status;
            if (!string.IsNullOrWhiteSpace(item.AnimalName))
            {
                AnimalName nameEntity;

                if (dbItem.NameID.HasValue)
                {
                    nameEntity = await _context.AnimalNames.FindAsync(dbItem.NameID.Value);
                    if (nameEntity != null)
                    {
                        nameEntity.CroatianName = item.AnimalName;
                    }
                    else
                    {
                        nameEntity = new AnimalName
                        {
                            CroatianName = item.AnimalName,
                            LatinName = item.AnimalName
                        };
                        _context.AnimalNames.Add(nameEntity);
                        await _context.SaveChangesAsync();
                        dbItem.NameID = nameEntity.NameID;
                    }
                }
                else
                {
                    nameEntity = await _context.AnimalNames
                        .FirstOrDefaultAsync(n => n.CroatianName == item.AnimalName);

                    if (nameEntity == null)
                    {
                        nameEntity = new AnimalName
                        {
                            CroatianName = item.AnimalName,
                            LatinName = item.AnimalName
                        };
                        _context.AnimalNames.Add(nameEntity);
                        await _context.SaveChangesAsync();
                    }

                    dbItem.NameID = nameEntity.NameID;
                }
            }
    }
    await _context.SaveChangesAsync();
    TempData["Message"] = "Success.";
    TempData["MessageType"] = "success";
    return RedirectToAction(nameof(MasterDetail), new { id = model.Enclosure.EnclosureID });}
            catch (DbUpdateConcurrencyException dbEx)
            {
                    _logger.LogError(dbEx, "Concurrency error while updating individual animals.");
                    TempData["Message"] = "Concurrency error. Please try again later.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction(nameof(MasterDetail), new {id = model.Enclosure.EnclosureID});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while editing the individual animals.");
                TempData["Message"] = "An unexpected error occurred. Please try again later.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(MasterDetail), new {id = model.Enclosure.EnclosureID});
            }
}

public async Task<IActionResult> ExportAllReportsPdf()
        {
            var enclosures = await _context.Enclosures
                .Include(r => r.AnimalType)
                .Include(r => r.Location)
                .Include(r => r.IndividualAnimals)
                    .ThenInclude(a => a.AnimalName)
                .OrderByDescending(r => r.Name)
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
                        .Text("Full Enclosure Log")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        foreach (var enclosure in enclosures)
                        {
                            column.Item().ShowEntire().Column(reportSection => 
                            {
                                reportSection.Item().Text(text =>
                                {
                                    text.Span($"Enclosure #{enclosure.EnclosureID}").Bold().FontSize(16);
                                    text.Span($" | Name: {enclosure.Name}").FontSize(14);
                                });
                                var typeName = enclosure.AnimalType?.AnimalTypes ?? "Unknown Type"; 
                                reportSection.Item().PaddingBottom(5).Text($"Type: {typeName} | Dimensions: {enclosure.Dimensions}")
                                    .FontColor(Colors.Grey.Darken2);
                                if (enclosure.IndividualAnimals != null && enclosure.IndividualAnimals.Any())
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
                                            header.Cell().Element(CellStyle).Text("Nickname");
                                            header.Cell().Element(CellStyle).Text("Enclosure");
                                            header.Cell().Element(CellStyle).Text("AnimalName");
                                        });
                                        foreach (var item in enclosure.IndividualAnimals)
                                        {
                                            table.Cell().Element(CellStyleContent).Text(item.AnimalID.ToString());
                                            table.Cell().Element(CellStyleContent).Text(item.Nickname ?? "-");
                                            var enclosureName = item.Enclosure?.Name ?? "---";
                                            table.Cell().Element(CellStyleContent).Text(enclosureName);

                                            var animalName = item.AnimalName?.CroatianName ?? "---";
                                            table.Cell().Element(CellStyleContent).Text(animalName);
                                        }
                                    });
                                }
                                else
                                {
                                    reportSection.Item().PaddingBottom(20).Text("No individual animals items listed.").Italic().FontColor(Colors.Grey.Medium);
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
            return File(stream, "application/pdf", "All_Enclosures.pdf");
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

        public async Task<IActionResult> ExportToExcel()
{
            var enclosures = await _context.Enclosures
                .Include(r => r.AnimalType)
                .Include(r => r.Location)
                .Include(r => r.IndividualAnimals)
                    .ThenInclude(a => a.AnimalName)
                .OrderByDescending(r => r.Name)
                .AsNoTracking()
                .ToListAsync();

    using (var workbook = new XLWorkbook())
    {
        foreach (var enclosure in enclosures)
        {
            string sheetName = $"Enclosure #{enclosure.EnclosureID}";
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).Value = "Enclosure Details";
            worksheet.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.SetFontSize(14);

            worksheet.Cell(3, 1).Value = "Enclosure ID:";
            worksheet.Cell(3, 2).Value = enclosure.EnclosureID;

            worksheet.Cell(4, 1).Value = "Name:";
            worksheet.Cell(4, 2).Value = enclosure.Name;

            worksheet.Cell(5, 1).Value = "Dimensions:";
            worksheet.Cell(5, 2).Value = enclosure.Dimensions;

            worksheet.Cell(6, 1).Value = "Animal Type:";
            worksheet.Cell(6, 2).Value = enclosure.AnimalType?.AnimalTypes ?? "Unknown";
            worksheet.Range(3, 1, 6, 1).Style.Font.SetBold();

            worksheet.Cell(7, 1).Value = "Location:";
            worksheet.Cell(7, 2).Value = enclosure.Location?.LocationName ?? "Unknown";
            worksheet.Range(3, 1, 7, 1).Style.Font.SetBold();

            int row = 9;
            worksheet.Cell(row, 1).Value = "ID";
            worksheet.Cell(row, 2).Value = "Nickname";
            worksheet.Cell(row, 3).Value = "Enclosure";
            worksheet.Cell(row, 4).Value = "AnimalName";
            
            var headerRange = worksheet.Range(row, 1, row, 4);
            headerRange.Style.Font.SetBold();
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            row++;
            if (enclosure.IndividualAnimals != null && enclosure.IndividualAnimals.Any())
            {
                foreach (var item in enclosure.IndividualAnimals)
                {
                    worksheet.Cell(row, 1).Value = item.AnimalID;
                    worksheet.Cell(row, 2).Value = item.Nickname ?? "-";
                    string enclosureName = item.Enclosure?.Name ?? "---";
                    worksheet.Cell(row, 3).Value = enclosureName;

                    string animalName = item.AnimalName?.CroatianName ?? "---";
                    worksheet.Cell(row, 4).Value = animalName;

                    row++;
                }
            }
            else
            {
                worksheet.Cell(row, 1).Value = "No items recorded.";
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
                "Enclosure_MasterDetail.xlsx");
        }
    }
}
    }
}
