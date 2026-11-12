using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for expenses.
    /// </summary>
    [ApiController]
    [Route("api/expenses")]
    public class ExpenseApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;
        private readonly ILogger<ExpenseApiController> _logger;

        public ExpenseApiController(
            Rppp08Context ctx,
            ILogger<ExpenseApiController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }
        /// <summary>
        /// Returns all expenses.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Expense>>> GetAll()
        {
            return await ctx.Expenses
                .Include(e => e.Task)
                .Include(e => e.Price)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns an expense by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Expense>> Get(int id)
        {
            var expense = await ctx.Expenses
                .Include(e => e.Task)
                .Include(e => e.Price)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return Problem(
                    statusCode: 404,
                    detail: $"Expense with ID {id} not found.");
            }

            return expense;
        }

        /// <summary>
        /// Creates a new expense.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Expense>> Post([FromBody] Expense model)
        {
            if (!await ctx.Tasks.AnyAsync(t => t.TaskID == model.TaskId))
            {
                return Problem(
                    statusCode: 400,
                    detail: $"Task with ID {model.TaskId} does not exist.");
            }

            if (model.PriceId.HasValue &&
                !await ctx.Prices.AnyAsync(p => p.PriceID == model.PriceId))
            {
                return Problem(
                    statusCode: 400,
                    detail: $"Price with ID {model.PriceId} does not exist.");
            }

            ctx.Expenses.Add(model);
            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                "Expense {ExpenseId} created via API for Task {TaskId}.",
                model.Id,
                model.TaskId
            );

            return CreatedAtAction(
                nameof(Get),
                new { id = model.Id },
                model);
        }

        /// <summary>
        /// Updates an existing expense.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] Expense model)
        {
            if (id != model.Id)
            {
                return Problem(
                    statusCode: 400,
                    detail: "ID mismatch.");
            }

            var dbExpense = await ctx.Expenses.FindAsync(id);
            if (dbExpense == null)
            {
                return Problem(
                    statusCode: 404,
                    detail: $"Expense with ID {id} not found.");
            }

            if (!await ctx.Tasks.AnyAsync(t => t.TaskID == model.TaskId))
            {
                return Problem(
                    statusCode: 400,
                    detail: $"Task with ID {model.TaskId} does not exist.");
            }

            if (model.PriceId.HasValue &&
                !await ctx.Prices.AnyAsync(p => p.PriceID == model.PriceId))
            {
                return Problem(
                    statusCode: 400,
                    detail: $"Price with ID {model.PriceId} does not exist.");
            }

            dbExpense.Type = model.Type;
            dbExpense.Description = model.Description;
            dbExpense.ArrivalDate = model.ArrivalDate;
            dbExpense.TaskId = model.TaskId;
            dbExpense.PriceId = model.PriceId;

            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                "Expense {ExpenseId} updated via API.",
                id
            );
            return NoContent();
        }

        /// <summary>
        /// Deletes an expense.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await ctx.Expenses.FindAsync(id);
            if (expense == null)
            {
                return Problem(
                    statusCode: 404,
                    detail: $"Expense with ID {id} not found.");
            }

            ctx.Expenses.Remove(expense);
            await ctx.SaveChangesAsync();
            _logger.LogWarning(
                "Expense {ExpenseId} deleted via API.",
                id
            );

            return NoContent();
        }
    }
}
