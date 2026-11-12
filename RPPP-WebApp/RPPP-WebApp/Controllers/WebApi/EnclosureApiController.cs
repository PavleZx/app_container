using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for enclosuresss.
    /// </summary>
    [ApiController]
    [Route("api/enclosures")]
    public class EnclosureApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;

        public EnclosureApiController(Rppp08Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Returns a list of all enclosures.
        /// </summary>
        /// <response code="200">Returns the list of enclosures.</response>
        [HttpGet(Name = "GetEnclosures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Enclosure>>> GetAll()
        {
            return await ctx.Enclosures
                .Include(r => r.AnimalType)
                .Include(r => r.Location)
                .Include(r => r.IndividualAnimals)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a specific enclosure by ID.
        /// </summary>
        /// <param name="id">Enclosure id.</param>
        /// <response code="200">Returns the enclosure.</response>
        /// <response code="404">Enclosure is not found.</response>
        [HttpGet("{id}", Name = "GetEnclosureById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Enclosure>> Get(int id)
        {
            var enclosure = await ctx.Enclosures
                .Include(r => r.AnimalType)
                .Include(r => r.Location)
                .Include(r => r.IndividualAnimals)
                    .ThenInclude(a => a.AnimalName)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.EnclosureID == id);

            if (enclosure == null)
            {
                return Problem(statusCode: 404, detail: $"No enclosure found with ID {id}");
            }

            return enclosure;
        }

        /// <summary>
        /// Creates a new enclosure.
        /// </summary>
        /// <param name="model">Enclosure.</param>
        /// <response code="201">Created successfully.</response>
        /// <response code="400">Invalid data.</response>
        [HttpPost(Name = "CreateEnclosure")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Enclosure>> Post([FromBody] Enclosure model)
        {
            var typeExists = await ctx.AnimalTypes.AnyAsync(t => t.AnimalTypeID == model.AnimalTypeID);
            if (!typeExists)
            {
                return Problem(statusCode: 400, detail: $"Animal Name with ID {model.AnimalTypeID} does not exist.");
            }
            var locationExists = await ctx.Locations.AnyAsync(t => t.LocationID == model.LocationID);
            if (!locationExists)
            {
                return Problem(statusCode: 400, detail: $"Location with ID {model.LocationID} does not exist.");
            }
            if (await ctx.Enclosures.AnyAsync())
            {
                model.EnclosureID = await ctx.Enclosures.MaxAsync(r => r.EnclosureID) + 1;
            }
            else
            {
                model.EnclosureID = 1;
            }

            ctx.Enclosures.Add(model);
            await ctx.SaveChangesAsync();

            return CreatedAtRoute("GetEnclosureById", new { id = model.EnclosureID }, model);
        }

        /// <summary>
        /// Updates an existing enclosure.
        /// </summary>
        /// <param name="id">Enclosure ID.</param>
        /// <param name="model">Updated enclosure object.</param>
        /// <response code="204">Update successful.</response>
        /// <response code="400">ID mismatch or invalid data.</response>
        /// <response code="404">Enclosure not found.</response>
        [HttpPut("{id}", Name = "UpdateEnclosure")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] Enclosure model)
        {
            if (id != model.EnclosureID)
            {
                return Problem(statusCode: 400, detail: "ID in URL and body do not match.");
            }

            var dbEnc = await ctx.Enclosures.FindAsync(id);
            if (dbEnc == null)
            {
                return Problem(statusCode: 404, detail: $"Report with ID {id} not found.");
            }

            var typeExists = await ctx.AnimalTypes.AnyAsync(t => t.AnimalTypeID == model.AnimalTypeID);
            if (!typeExists)
            {
                return Problem(statusCode: 400, detail: $"Name with ID {model.AnimalTypeID} does not exist.");
            }

            var locationExists = await ctx.Locations.AnyAsync(t => t.LocationID == model.LocationID);
            if (!locationExists)
            {
                return Problem(statusCode: 400, detail: $"Location with ID {model.LocationID} does not exist.");
            }
            dbEnc.Dimensions = model.Dimensions;
            dbEnc.Name = model.Name;
            dbEnc.Change = model.Change;
            dbEnc.Lighting = model.Lighting;
            dbEnc.Shading = model.Shading;
            dbEnc.AnimalTypeID = model.AnimalTypeID;
            dbEnc.LocationID = model.LocationID;

            await ctx.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an enclosure.
        /// </summary>
        /// <param name="id">Enclosure ID.</param>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="404">Not found.</response>
        [HttpDelete("{id}", Name = "DeleteEnclosure")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var enclosure = await ctx.Enclosures.FindAsync(id);
            if (enclosure == null)
            {
                return Problem(statusCode: 404, detail: $"Enclosure with ID {id} not found.");
            }
            try 
            {
                ctx.Enclosures.Remove(enclosure);
                await ctx.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return Problem(statusCode: 500, detail: "Could not delete enclosure. Usually caused by existing related data (IndividualAnimals). " + ex.Message);
            }

            return NoContent();
        }
    }
}