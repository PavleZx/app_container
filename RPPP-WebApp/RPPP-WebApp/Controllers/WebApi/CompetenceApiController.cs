using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

/// <summary>
/// Provides CRUD operations for competences.
/// </summary>
/// <remarks>
/// This controller exposes a RESTful Web API for managing competences.
/// It supports standard HTTP verbs and returns JSON responses.
/// </remarks>
[ApiController]
[Route("api/competences")]
public class CompetenceApiController : ControllerBase
{
    private readonly Rppp08Context ctx;

    public CompetenceApiController(Rppp08Context ctx)
    {
        this.ctx = ctx;
    }

    /// <summary>
    /// Returns a list of all competences.
    /// </summary>
    /// <remarks>
    /// Retrieves all competences from the system.
    /// The result is returned as a JSON array.
    /// </remarks>
    /// <returns>
    /// A list of competences.
    /// </returns>
    /// <response code="200">Competences successfully returned.</response>
    [HttpGet(Name = "GetCompetences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<CompetenceViewModel>> GetAll()
    {
        return await ctx.Competences
            .AsNoTracking()
            .Select(c => new CompetenceViewModel
            {
                CompetenceId = c.CompetenceId,
                Name = c.Name,
                Description = c.Description,
                ExpiryDate = c.ExpiryDate,
                WorkerId = c.WorkerId,
                CertificateId = c.CertificateId
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns a competence by identifier.
    /// </summary>
    /// <param name="id">Competence identifier.</param>
    /// <returns>
    /// The competence with the specified identifier.
    /// </returns>
    /// <response code="200">Competence successfully returned.</response>
    /// <response code="404">Competence with the given identifier does not exist.</response>
    [HttpGet("{id}", Name = "GetCompetenceById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompetenceViewModel>> Get(int id)
    {
        var competence = await ctx.Competences
            .Where(c => c.CompetenceId == id)
            .Select(c => new CompetenceViewModel
            {
                CompetenceId = c.CompetenceId,
                Name = c.Name,
                Description = c.Description,
                ExpiryDate = c.ExpiryDate,
                WorkerId = c.WorkerId,
                CertificateId = c.CertificateId
            })
            .FirstOrDefaultAsync();

        if (competence == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Competence with id = {id} does not exist");
        }

        return competence;
    }

    /// <summary>
    /// Creates a new competence.
    /// </summary>
    /// <param name="model">Competence data.</param>
    /// <returns>
    /// The created competence.
    /// </returns>
    /// <response code="201">Competence successfully created.</response>
    /// <response code="400">Invalid competence data.</response>
    /// <response code="404">Related worker or certificate does not exist.</response>
    [HttpPost(Name = "CreateCompetence")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompetenceViewModel>> Post([FromBody] CompetenceViewModel model)
    {
        var worker = await ctx.Workers.FindAsync(model.WorkerId);
        if (worker == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {model.WorkerId} does not exist");
        }

        if (model.CertificateId != null)
        {
            var certificate = await ctx.Certificates.FindAsync(model.CertificateId);
            if (certificate == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"Certificate with id = {model.CertificateId} does not exist");
            }
        }

        var competence = new Competence
        {
            Name = model.Name,
            Description = model.Description,
            ExpiryDate = model.ExpiryDate,
            WorkerId = model.WorkerId,
            CertificateId = model.CertificateId
        };

        ctx.Competences.Add(competence);
        await ctx.SaveChangesAsync();

        model.CompetenceId = competence.CompetenceId;

        return CreatedAtRoute(
            "GetCompetenceById",
            new { id = competence.CompetenceId },
            model
        );
    }

    /// <summary>
    /// Updates an existing competence.
    /// </summary>
    /// <param name="id">Competence identifier.</param>
    /// <param name="model">Updated competence data.</param>
    /// <returns>
    /// No content if update is successful.
    /// </returns>
    /// <response code="204">Competence successfully updated.</response>
    /// <response code="400">Invalid request or identifier mismatch.</response>
    /// <response code="404">Competence, worker or certificate does not exist.</response>
    [HttpPut("{id}", Name = "UpdateCompetence")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, [FromBody] CompetenceViewModel model)
    {
        if (id != model.CompetenceId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Route id and model CompetenceId must match.");
        }

        var competence = await ctx.Competences.FindAsync(id);
        if (competence == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Competence with id = {id} does not exist");
        }

        var worker = await ctx.Workers.FindAsync(model.WorkerId);
        if (worker == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {model.WorkerId} does not exist");
        }

        if (model.CertificateId != null)
        {
            var certificate = await ctx.Certificates.FindAsync(model.CertificateId);
            if (certificate == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"Certificate with id = {model.CertificateId} does not exist");
            }
        }

        competence.Name = model.Name;
        competence.Description = model.Description;
        competence.ExpiryDate = model.ExpiryDate;
        competence.WorkerId = model.WorkerId;
        competence.CertificateId = model.CertificateId;

        await ctx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a competence.
    /// </summary>
    /// <param name="id">Competence identifier.</param>
    /// <returns>
    /// No content if deletion is successful.
    /// </returns>
    /// <response code="204">Competence successfully deleted.</response>
    /// <response code="404">Competence with the given identifier does not exist.</response>
    [HttpDelete("{id}", Name = "DeleteCompetence")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var competence = await ctx.Competences.FindAsync(id);
        if (competence == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Competence with id = {id} does not exist");
        }

        ctx.Competences.Remove(competence);
        await ctx.SaveChangesAsync();

        return NoContent();
    }
}
