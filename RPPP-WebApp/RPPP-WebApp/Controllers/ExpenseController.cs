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


namespace RPPP_WebApp.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly Rppp08Context _context;
        private readonly ILogger<ExpenseController> _logger;


        public ExpenseController(
            Rppp08Context ctx,
            ILogger<ExpenseController> logger)
        {
            this._context = ctx;
            _logger = logger;
        }

       private void PrepareExpenseDropdowns(int? taskId = null, int? priceId = null)    
       {
            // Get tasks
            var tasks = _context.Tasks
                .AsNoTracking()
                .Select(t => new { 
                    t.TaskID, 
                    DisplayText = "Task " + t.TaskID + " (" + 
                        (t.TimeStart.HasValue ? t.TimeStart.Value.ToString("yyyy-MM-dd") : "N/A") + " - " + 
                        (t.TimeEnd.HasValue ? t.TimeEnd.Value.ToString("yyyy-MM-dd") : "N/A") + ")"
                })
                .ToList();

            // Get prices - use correct property names: PriceID and ItemName
            var prices = _context.Prices
                .AsNoTracking()
                .Select(p => new {
                    p.PriceID,
                    ItemName = p.ItemName.Trim()  // Remove trailing spaces
                })
                .ToList();

            ViewData["TaskId"] = new SelectList(tasks, "TaskID", "DisplayText", taskId);
            ViewData["PriceId"] = new SelectList(prices, "PriceID", "ItemName", priceId);
        }

        public IActionResult Index(int taskId, int page = 1, int sort = 0, bool ascending = true)
        {
            var expenses = _context.Expenses
                .AsNoTracking()
                .Where(e => e.TaskId == taskId)
                .Include(e => e.Price)  
                .Select(e => new ExpenseViewModel
                {
                    ExpenseID = e.Id,
                    Type = e.Type!,
                    ArrivalDate = e.ArrivalDate,
                    Description = e.Description,
                    PriceAmount = e.Price != null ? (decimal)e.Price.PriceValue : 0m
                })
                .ToList();

            var model = new ExpenseIndexViewModel
            {
                TaskId = taskId,
                Expenses = expenses,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    Sort = sort,
                    Ascending = ascending
                }
            };

            return View(model);
        }



        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (!ModelState.IsValid)
            {
                PrepareExpenseDropdowns(expense.TaskId, expense.PriceId);
                return View(expense);
            }

            _context.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { taskId = expense.TaskId });
        }


        // GET
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _context.Expenses
                .AsNoTracking()  // Don't track related entities
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (expense == null) return NotFound();

            PrepareExpenseDropdowns(expense.TaskId, expense.PriceId);
            return View(expense);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PrepareExpenseDropdowns(expense.TaskId, expense.PriceId);
                return View(expense);
            }

            try
            {
                // Only update the Expense, ignore navigation properties
                _context.Entry(expense).State = EntityState.Modified;
                
                // Tell EF to NOT touch the navigation properties
                _context.Entry(expense).Reference(e => e.Task).IsModified = false;
                _context.Entry(expense).Reference(e => e.Price).IsModified = false;
                
                await _context.SaveChangesAsync();
                
                TempData["Message"] = "Expense updated successfully.";
                TempData["MessageType"] = "success";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense {Id}", id);
                ModelState.AddModelError("", "Unable to save changes.");
                PrepareExpenseDropdowns(expense.TaskId, expense.PriceId);
                return View(expense);
            }
        }

        [HttpGet]
        public IActionResult Create(int? taskId = null, int? priceId = null)
        {
            PrepareExpenseDropdowns(taskId, priceId);
            return View(new Expense { TaskId = taskId, PriceId = priceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int taskId, int page = 1, int sort = 0, bool ascending = true)
        {
            var expense = await _context.Expenses.FindAsync(id);
            
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { 
                taskId = taskId,
                page = page,
                sort = sort,
                ascending = ascending
            });
        }

        // SINGLE DETAIL
        public IActionResult Details(int id)
        {
            var expense = _context.Expenses
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new ExpenseDetailsViewModel
                {
                    ExpenseID = e.Id,
                    Type = e.Type!,
                    ArrivalDate = e.ArrivalDate,
                    Description = e.Description,
                })
                .FirstOrDefault();

            if (expense == null)
                return NotFound();

            return View(expense);
        }
    }
}