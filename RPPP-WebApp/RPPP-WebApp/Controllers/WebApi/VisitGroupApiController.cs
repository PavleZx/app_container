using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

/// <summary>
/// Provides CRUD operations for visit groups.
/// </summary>
[ApiController]
[Route("api/visitgroups")]
public class VisitGroupApiController : ControllerBase
{
    private readonly Rppp08Context ctx;

    public VisitGroupApiController(Rppp08Context ctx)
    {
        this.ctx = ctx;
    }

    /// <summary>
    /// Returns a list of all visit groups.
    /// </summary>
    [HttpGet(Name = "GetVisitGroups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<VisitGroupViewModel>> GetAll()
    {
        return await ctx.VisitGroups
            .AsNoTracking()
            .Select(v => new VisitGroupViewModel
            {
                VisitGroupId = v.VisitGroupId,
                GroupName = v.GroupName,
                DateOfVisiting = v.DateOfVisiting,
                Approval = v.Approval,
                VisitorCount = v.VisitorCount,
                TourId = v.TourId
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns a visit group by identifier.
    /// </summary>
    [HttpGet("{id}", Name = "GetVisitGroupById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitGroupViewModel>> Get(int id)
    {
        var visitGroup = await ctx.VisitGroups
            .Where(v => v.VisitGroupId == id)
            .Select(v => new VisitGroupViewModel
            {
                VisitGroupId = v.VisitGroupId,
                GroupName = v.GroupName,
                DateOfVisiting = v.DateOfVisiting,
                Approval = v.Approval,
                VisitorCount = v.VisitorCount,
                TourId = v.TourId
            })
            .FirstOrDefaultAsync();

        if (visitGroup == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"VisitGroup with id = {id} does not exist");
        }

        return visitGroup;
    }

    /// <summary>
    /// Creates a new visit group.
    /// </summary>
    [HttpPost(Name = "CreateVisitGroup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitGroupViewModel>> Post([FromBody] VisitGroupViewModel model)
    {
        var tour = await ctx.TourTypes.FindAsync(model.TourId);
        if (tour == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"TourType with id = {model.TourId} does not exist");
        }

        var visitGroup = new VisitGroup
        {
            GroupName = model.GroupName,
            DateOfVisiting = model.DateOfVisiting,
            Approval = model.Approval,
            VisitorCount = model.VisitorCount,
            TourId = model.TourId
        };

        ctx.VisitGroups.Add(visitGroup);
        await ctx.SaveChangesAsync();

        model.VisitGroupId = visitGroup.VisitGroupId;

        return CreatedAtRoute(
            "GetVisitGroupById",
            new { id = visitGroup.VisitGroupId },
            model
        );
    }

    /// <summary>
    /// Updates an existing visit group.
    /// </summary>
    [HttpPut("{id}", Name = "UpdateVisitGroup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, [FromBody] VisitGroupViewModel model)
    {
        if (id != model.VisitGroupId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Route id and model VisitGroupId must match.");
        }

        var visitGroup = await ctx.VisitGroups.FindAsync(id);
        if (visitGroup == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"VisitGroup with id = {id} does not exist");
        }

        var tour = await ctx.TourTypes.FindAsync(model.TourId);
        if (tour == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"TourType with id = {model.TourId} does not exist");
        }

        visitGroup.GroupName = model.GroupName;
        visitGroup.DateOfVisiting = model.DateOfVisiting;
        visitGroup.Approval = model.Approval;
        visitGroup.VisitorCount = model.VisitorCount;
        visitGroup.TourId = model.TourId;

        await ctx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a visit group.
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteVisitGroup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var visitGroup = await ctx.VisitGroups.FindAsync(id);
        if (visitGroup == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"VisitGroup with id = {id} does not exist");
        }

        ctx.VisitGroups.Remove(visitGroup);
        await ctx.SaveChangesAsync();

        return NoContent();
    }
}
