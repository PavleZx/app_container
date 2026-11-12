using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for 'Affects' items (Incident details).
    /// </summary>
    [ApiController]
    [Route("api/affects")]
    public class AffectsApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;

        public AffectsApiController(Rppp08Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Returns all 'Affects' records.
        /// </summary>
        [HttpGet(Name = "GetAffects")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Affects>>> GetAll()
        {
            return await ctx.Affects
                .Include(a => a.Enclosure)
                .Include(a => a.IndividualAnimal)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a specific 'Affects' record by ID.
        /// </summary>
        [HttpGet("{id}", Name = "GetAffectsById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Affects>> Get(int id)
        {
            var item = await ctx.Affects
                .Include(a => a.Enclosure)
                .Include(a => a.IndividualAnimal)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.affectsID == id);

            if (item == null)
            {
                return Problem(statusCode: 404, detail: $"Affects item with ID {id} not found.");
            }
            return item;
        }

        /// <summary>
        /// Creates a new 'Affects' record.
        /// </summary>
        /// <remarks>
        /// ReportID is required. EnclosureID and AnimalID are optional (nullable).
        /// </remarks>
        [HttpPost(Name = "CreateAffects")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Affects>> Post([FromBody] Affects model)
        {
            var reportExists = await ctx.IncidentReports.AnyAsync(r => r.ReportID == model.reportID);
            if (!reportExists)
            {
                return Problem(statusCode: 400, detail: $"Report with ID {model.reportID} does not exist.");
            }

            if (model.enclosureID.HasValue)
            {
                if (!await ctx.Enclosures.AnyAsync(e => e.EnclosureID == model.enclosureID))
                    return Problem(statusCode: 400, detail: $"Enclosure with ID {model.enclosureID} does not exist.");
            }
            if (model.animalID.HasValue)
            {
                if (!await ctx.IndividualAnimals.AnyAsync(a => a.AnimalID == model.animalID))
                    return Problem(statusCode: 400, detail: $"Animal with ID {model.animalID} does not exist.");
            }

            if (await ctx.Affects.AnyAsync())
            {
                model.affectsID = await ctx.Affects.MaxAsync(a => a.affectsID) + 1;
            }
            else
            {
                model.affectsID = 1;
            }

            ctx.Affects.Add(model);
            await ctx.SaveChangesAsync();

            return CreatedAtRoute("GetAffectsById", new { id = model.affectsID }, model);
        }

        /// <summary>
        /// Updates an existing 'Affects' record.
        /// </summary>
        [HttpPut("{id}", Name = "UpdateAffects")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] Affects model)
        {
            if (id != model.affectsID)
            {
                return Problem(statusCode: 400, detail: "ID mismatch.");
            }

            var dbItem = await ctx.Affects.FindAsync(id);
            if (dbItem == null)
            {
                return Problem(statusCode: 404, detail: $"Item with ID {id} not found.");
            }

            if (!await ctx.IncidentReports.AnyAsync(r => r.ReportID == model.reportID))
                return Problem(statusCode: 400, detail: "Invalid ReportID.");

            if (model.enclosureID.HasValue && !await ctx.Enclosures.AnyAsync(e => e.EnclosureID == model.enclosureID))
                return Problem(statusCode: 400, detail: "Invalid EnclosureID.");

            if (model.animalID.HasValue && !await ctx.IndividualAnimals.AnyAsync(a => a.AnimalID == model.animalID))
                return Problem(statusCode: 400, detail: "Invalid AnimalID.");

            dbItem.reportID = model.reportID;
            dbItem.description = model.description;
            dbItem.enclosureID = model.enclosureID;
            dbItem.animalID = model.animalID;
            dbItem.groupID = model.groupID;

            await ctx.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an 'Affects' record.
        /// </summary>
        [HttpDelete("{id}", Name = "DeleteAffects")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await ctx.Affects.FindAsync(id);
            if (item == null)
            {
                return Problem(statusCode: 404, detail: "Item not found.");
            }

            ctx.Affects.Remove(item);
            await ctx.SaveChangesAsync();

            return NoContent();
        }
    }
}