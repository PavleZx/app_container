using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for 'Affects' items (Incident details).
    /// </summary>
    [ApiController]
    [Route("api/individualAnimals")]
    public class IndividualAnimalsApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;

        public IndividualAnimalsApiController(Rppp08Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Returns all 'IndividualAnimals' records.
        /// </summary>
        [HttpGet(Name = "GetIndividualAnimals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IndividualAnimals>>> GetAll()
        {
            return await ctx.IndividualAnimals
                .Include(a => a.Enclosure)
                .Include(a => a.AnimalName)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a specific 'IndividualAnimals' record by ID.
        /// </summary>
        [HttpGet("{id}", Name = "GetIndividualAnimalsById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IndividualAnimals>> Get(int id)
        {
            var item = await ctx.IndividualAnimals
                .Include(a => a.Enclosure)
                .Include(a => a.AnimalName)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnimalID == id);

            if (item == null)
            {
                return Problem(statusCode: 404, detail: $"Individual animal item with ID {id} not found.");
            }
            return item;
        }

        /// <summary>
        /// Creates a new 'IndividualAnimals' record.
        /// </summary>
        /// <remarks>
        /// EnclosureID is required. NameID optional (nullable).
        /// </remarks>
        [HttpPost(Name = "CreateIndividualAnimals")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IndividualAnimals>> Post([FromBody] IndividualAnimals model)
        {
            var enclosureExists = await ctx.Enclosures.AnyAsync(r => r.EnclosureID == model.EnclosureID);
            if (!enclosureExists)
            {
                return Problem(statusCode: 400, detail: $"Enclosure with ID {model.EnclosureID} does not exist.");
            }

            if (model.NameID.HasValue)
            {
                if (!await ctx.AnimalNames.AnyAsync(e => e.NameID == model.NameID))
                    return Problem(statusCode: 400, detail: $"Name with ID {model.NameID} does not exist.");
            }

            if (await ctx.IndividualAnimals.AnyAsync())
            {
                model.AnimalID = await ctx.IndividualAnimals.MaxAsync(a => a.AnimalID) + 1;
            }
            else
            {
                model.AnimalID = 1;
            }

            ctx.IndividualAnimals.Add(model);
            await ctx.SaveChangesAsync();

            return CreatedAtRoute("GetIndividualAnimalsById", new { id = model.AnimalID }, model);
        }

        /// <summary>
        /// Updates an existing 'IndividualAnimal' record.
        /// </summary>
        [HttpPut("{id}", Name = "UpdateIndividualAnimals")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] IndividualAnimals model)
        {
            if (id != model.AnimalID)
            {
                return Problem(statusCode: 400, detail: "ID mismatch.");
            }

            var dbItem = await ctx.IndividualAnimals.FindAsync(id);
            if (dbItem == null)
            {
                return Problem(statusCode: 404, detail: $"Item with ID {id} not found.");
            }

            if (!await ctx.Enclosures.AnyAsync(r => r.EnclosureID == model.EnclosureID))
                return Problem(statusCode: 400, detail: "Invalid EnclosureID.");

            if (model.NameID>0 && !await ctx.AnimalNames.AnyAsync(e => e.NameID == model.NameID))
                return Problem(statusCode: 400, detail: "Invalid NameID.");

            if (model.AnimalID>0 && !await ctx.IndividualAnimals.AnyAsync(a => a.AnimalID == model.AnimalID))
                return Problem(statusCode: 400, detail: "Invalid AnimalID.");

            dbItem.AnimalID = model.AnimalID;
            dbItem.SerialNum = model.SerialNum;
            dbItem.Acquisition = model.Acquisition;
            dbItem.Nickname = model.Nickname;
            dbItem.Status = model.Status;
            dbItem.EnclosureID = model.EnclosureID;
            dbItem.NameID = model.NameID;

            await ctx.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an 'IndividualAnimal' record.
        /// </summary>
        [HttpDelete("{id}", Name = "DeleteIndividualAnimals")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await ctx.IndividualAnimals.FindAsync(id);
            if (item == null)
            {
                return Problem(statusCode: 404, detail: "Item not found.");
            }

            ctx.IndividualAnimals.Remove(item);
            await ctx.SaveChangesAsync();

            return NoContent();
        }
    }
}