using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

/// <summary>
/// Provides CRUD operations for group guides.
/// </summary>
[ApiController]
[Route("api/groupguides")]
public class GroupGuideApiController : ControllerBase
{
    private readonly Rppp08Context ctx;

    public GroupGuideApiController(Rppp08Context ctx)
    {
        this.ctx = ctx;
    }

    /// <summary>
    /// Returns a list of all group guides.
    /// </summary>
    [HttpGet(Name = "GetGroupGuides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<GroupGuideViewModel>> GetAll()
    {
        return await ctx.GroupGuides
            .AsNoTracking()
            .Select(g => new GroupGuideViewModel
            {
                GroupGuideId = g.GroupGuideId,
                VisitGroupId = g.VisitGroupId,
                WorkerId = g.WorkerId,
                GuideRole = g.GuideRole,
                Activity = g.Activity
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns a group guide by identifier.
    /// </summary>
    [HttpGet("{id}", Name = "GetGroupGuideById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupGuideViewModel>> Get(int id)
    {
        var groupGuide = await ctx.GroupGuides
            .Where(g => g.GroupGuideId == id)
            .Select(g => new GroupGuideViewModel
            {
                GroupGuideId = g.GroupGuideId,
                VisitGroupId = g.VisitGroupId,
                WorkerId = g.WorkerId,
                GuideRole = g.GuideRole,
                Activity = g.Activity
            })
            .FirstOrDefaultAsync();

        if (groupGuide == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"GroupGuide with id = {id} does not exist");
        }

        return groupGuide;
    }

    /// <summary>
    /// Creates a new group guide.
    /// </summary>
    [HttpPost(Name = "CreateGroupGuide")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupGuideViewModel>> Post([FromBody] GroupGuideViewModel model)
    {
        if (await ctx.VisitGroups.FindAsync(model.VisitGroupId) == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"VisitGroup with id = {model.VisitGroupId} does not exist");
        }

        if (await ctx.Workers.FindAsync(model.WorkerId) == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Worker with id = {model.WorkerId} does not exist");
        }

        var groupGuide = new GroupGuide
        {
            VisitGroupId = model.VisitGroupId,
            WorkerId = model.WorkerId,
            GuideRole = model.GuideRole,
            Activity = model.Activity
        };

        ctx.GroupGuides.Add(groupGuide);
        await ctx.SaveChangesAsync();

        model.GroupGuideId = groupGuide.GroupGuideId;

        return CreatedAtRoute(
            "GetGroupGuideById",
            new { id = groupGuide.GroupGuideId },
            model
        );
    }

    /// <summary>
    /// Updates an existing group guide.
    /// </summary>
    [HttpPut("{id}", Name = "UpdateGroupGuide")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, [FromBody] GroupGuideViewModel model)
    {
        if (id != model.GroupGuideId)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Route id and model GroupGuideId must match.");
        }

        var groupGuide = await ctx.GroupGuides.FindAsync(id);
        if (groupGuide == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"GroupGuide with id = {id} does not exist");
        }

        if (await ctx.VisitGroups.FindAsync(model.VisitGroupId) == null ||
            await ctx.Workers.FindAsync(model.WorkerId) == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: "Related VisitGroup or Worker does not exist");
        }

        groupGuide.VisitGroupId = model.VisitGroupId;
        groupGuide.WorkerId = model.WorkerId;
        groupGuide.GuideRole = model.GuideRole;
        groupGuide.Activity = model.Activity;

        await ctx.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a group guide.
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteGroupGuide")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var groupGuide = await ctx.GroupGuides.FindAsync(id);
        if (groupGuide == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"GroupGuide with id = {id} does not exist");
        }

        ctx.GroupGuides.Remove(groupGuide);
        await ctx.SaveChangesAsync();

        return NoContent();
    }
}
