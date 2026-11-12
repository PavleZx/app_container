using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using TaskEntity = RPPP_WebApp.Model.Task;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for tasks.
    /// </summary>
    [ApiController]
    [Route("api/tasks")]
    public class TaskApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;
        private readonly ILogger<TaskApiController> _logger;

        public TaskApiController(
            Rppp08Context ctx,
            ILogger<TaskApiController> logger)
        {
            this.ctx = ctx;
            _logger = logger;
        }

        /// <summary>
        /// Returns all tasks.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaskEntity>>> GetAll()
        {
            return await ctx.Tasks
                .Include(t => t.Enclosure)
                .Include(t => t.TaskType)
                .Include(t => t.TaskStatus)
                .Include(t => t.Expenses)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a task by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskEntity>> Get(int id)
        {
            var task = await ctx.Tasks
                .Include(t => t.Enclosure)
                .Include(t => t.TaskType)
                .Include(t => t.TaskStatus)
                .Include(t => t.Expenses)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskID == id);

            if (task == null)
            {
                return Problem(statusCode: 404,
                    detail: $"Task with ID {id} not found.");
            }

            return task;
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskEntity>> Post([FromBody] TaskEntity model)
        {
            if (!await ctx.Enclosures.AnyAsync(e => e.EnclosureID == model.EnclosureID))
                return Problem(statusCode:400, detail: "Invalid EnclosureID.");

            if (!await ctx.TaskTypes.AnyAsync(t => t.TaskTypeID == model.TypeID))
                return Problem(statusCode:400, detail: "Invalid TypeID.");

            if (!await ctx.TaskStatuses.AnyAsync(s => s.TaskStatusID == model.StatusID))
                return Problem(statusCode:400, detail: "Invalid StatusID.");

            ctx.Tasks.Add(model);
            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                "Task {TaskId} created via API.",
                model.TaskID
            );

            return CreatedAtAction(nameof(Get),
                new { id = model.TaskID }, model);
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] TaskEntity model)
        {
            if (id != model.TaskID)
                return Problem(statusCode:400, detail: "ID mismatch.");

            var dbTask = await ctx.Tasks.FindAsync(id);
            if (dbTask == null)
                return Problem(statusCode:404, detail: $"Task {id} not found.");

            dbTask.TimeStart = model.TimeStart;
            dbTask.TimeEnd = model.TimeEnd;
            dbTask.Notes = model.Notes;
            dbTask.EnclosureID = model.EnclosureID;
            dbTask.TypeID = model.TypeID;
            dbTask.StatusID = model.StatusID;

            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                "Task {TaskId} updated via API.",
                id
            );
            return NoContent();
        }

        /// <summary>
        /// Deletes a task.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await ctx.Tasks.FindAsync(id);
            if (task == null)
                return Problem(statusCode:404, detail: $"Task {id} not found.");

            try
            {
                ctx.Tasks.Remove(task);
                await ctx.SaveChangesAsync();
                _logger.LogWarning(
                    "Task {TaskId} deleted via API.",
                    id
                );
            }
            catch (Exception ex)
            {
                return Problem(statusCode:500,
                    detail: "Cannot delete task (existing expenses). " + ex.Message);
            }

            return NoContent();
        }
    }
}
