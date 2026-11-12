using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

/// <summary>
/// Provides CRUD operations for workers.
/// </summary>
/// <remarks>
/// This controller exposes a RESTful Web API for managing workers.
/// It supports standard HTTP verbs and returns JSON responses.
/// </remarks>
[ApiController]
[Route("api/workers")]
public class WorkerApiController : ControllerBase
{
    private readonly Rppp08Context ctx;

    public WorkerApiController(Rppp08Context ctx)
    {
        this.ctx = ctx;
    }

    /// <summary>
    /// Returns a list of all workers.
    /// </summary>
    /// <remarks>
    /// Retrieves all workers from the system.
    /// The result is returned as a JSON array.
    /// </remarks>
    /// <returns>
    /// A list of workers.
    /// </returns>
    /// <response code="200">Workers successfully returned.</response>
    [HttpGet(Name = "GetWorkers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<WorkerViewModel>> GetAll()
    {
        return await ctx.Workers
            .AsNoTracking()
            .Select(w => new WorkerViewModel
            {
                WorkerId = w.WorkerId,
                Name = w.Name,
                Surname = w.Surname,
                Email = w.Email,
                Phone = w.Phone,
                AvailabilityId = w.AvailabilityId
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns a worker by identifier.
    /// </summary>
    /// <remarks>
    /// Retrieves a single worker using its unique identifier.
    /// </remarks>
    /// <param name="id">Worker identifier.</param>
    /// <returns>
    /// The worker with the specified identifier.
    /// </returns>
    /// <response code="200">Worker successfully returned.</response>
    /// <response code="404">Worker with the given identifier does not exist.</response>
    [HttpGet("{id}", Name = "GetWorkerById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerViewModel>> Get(int id)
    {
        var worker = await ctx.Workers
            .Where(w => w.WorkerId == id)
            .Select(w => new WorkerViewModel
            {
                WorkerId = w.WorkerId,
                Name = w.Name,
                Surname = w.Surname,
                Email = w.Email,
                Phone = w.Phone,
                AvailabilityId = w.AvailabilityId
            })
            .FirstOrDefaultAsync();

        if (worker == null)
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {id} does not exist");

        return worker;
    }

    /// <summary>
    /// Creates a new worker.
    /// </summary>
    /// <remarks>
    /// Creates a new worker using the provided data.
    /// If successful, returns HTTP 201 with the created worker and its URI.
    /// </remarks>
    /// <param name="model">Worker data.</param>
    /// <returns>
    /// The created worker.
    /// </returns>
    /// <response code="201">Worker successfully created.</response>
    /// <response code="400">Invalid worker data.</response>
    [HttpPost(Name = "CreateWorker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkerViewModel>> Post([FromBody] WorkerViewModel model)
    {
        if (model.AvailabilityId != null)
        {
            var avaibilityId = await ctx.Availabilities.FindAsync(model.AvailabilityId);
            if (avaibilityId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"Avaibility with id = {model.AvailabilityId} does not exist");
            }
        }

        var worker = new Worker
        {
            Name = model.Name,
            Surname = model.Surname,
            Email = model.Email,
            Phone = model.Phone,
            AvailabilityId = model.AvailabilityId
        };

        ctx.Workers.Add(worker);
        await ctx.SaveChangesAsync();

        model.WorkerId = worker.WorkerId;

        return CreatedAtRoute(
            "GetWorkerById",
            new { id = worker.WorkerId },
            model
        );
    }

    /// <summary>
    /// Updates an existing worker.
    /// </summary>
    /// <remarks>
    /// Updates all properties of an existing worker.
    /// The identifier in the URL must match the identifier in the request body.
    /// </remarks>
    /// <param name="id">Worker identifier.</param>
    /// <param name="model">Updated worker data.</param>
    /// <returns>
    /// No content if update is successful.
    /// </returns>
    /// <response code="204">Worker successfully updated.</response>
    /// <response code="400">Invalid request or identifier mismatch.</response>
    /// <response code="404">Worker with the given identifier does not exist.</response>
    [HttpPut("{id}", Name = "UpdateWorker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, [FromBody] WorkerViewModel model)
    {
        if (id != model.WorkerId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Route id and model WorkerId must match.");
        }

        var worker = await ctx.Workers.FindAsync(id);
        if (worker == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {id} does not exist");
        }

        if (model.AvailabilityId != null) { 
            var avaibilityId = await ctx.Availabilities.FindAsync(model.AvailabilityId);
            if (avaibilityId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"Avaibility with id = {model.AvailabilityId} does not exist");
            }
        }

        worker.Name = model.Name;
        worker.Surname = model.Surname;
        worker.Email = model.Email;
        worker.Phone = model.Phone;
        worker.AvailabilityId = model.AvailabilityId;

        await ctx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a worker.
    /// </summary>
    /// <remarks>
    /// Deletes a worker identified by the given identifier.
    /// </remarks>
    /// <param name="id">Worker identifier.</param>
    /// <returns>
    /// No content if deletion is successful.
    /// </returns>
    /// <response code="204">Worker successfully deleted.</response>
    /// <response code="404">Worker with the given identifier does not exist.</response>
    [HttpDelete("{id}", Name = "DeleteWorker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var worker = await ctx.Workers.FindAsync(id);
        if (worker == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {id} does not exist");
        }

        ctx.Workers.Remove(worker);
        await ctx.SaveChangesAsync();

        return NoContent();
    }

}
