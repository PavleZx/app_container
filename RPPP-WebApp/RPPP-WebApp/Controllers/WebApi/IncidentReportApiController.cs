using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.Controllers.WebApi
{
    /// <summary>
    /// Provides CRUD operations for incident reports.
    /// </summary>
    [ApiController]
    [Route("api/incidentreports")]
    public class IncidentReportApiController : ControllerBase
    {
        private readonly Rppp08Context ctx;

        public IncidentReportApiController(Rppp08Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Returns a list of all incident reports.
        /// </summary>
        /// <response code="200">Returns the list of reports.</response>
        [HttpGet(Name = "GetIncidentReports")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IncidentReport>>> GetAll()
        {
            return await ctx.IncidentReports
                .Include(r => r.IncidentType)
                .Include(r => r.Affects)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a specific incident report by ID.
        /// </summary>
        /// <param name="id">Report identifier.</param>
        /// <response code="200">Returns the report.</response>
        /// <response code="404">If the report is not found.</response>
        [HttpGet("{id}", Name = "GetIncidentReportById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentReport>> Get(int id)
        {
            var report = await ctx.IncidentReports
                .Include(r => r.IncidentType)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.Enclosure)
                .Include(r => r.Affects)
                    .ThenInclude(a => a.IndividualAnimal)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null)
            {
                return Problem(statusCode: 404, detail: $"No report found with ID {id}");
            }

            return report;
        }

        /// <summary>
        /// Creates a new incident report.
        /// </summary>
        /// <param name="model">Report object.</param>
        /// <response code="201">Created successfully.</response>
        /// <response code="400">Invalid data.</response>
        [HttpPost(Name = "CreateIncidentReport")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IncidentReport>> Post([FromBody] IncidentReport model)
        {
            var typeExists = await ctx.IncidentTypes.AnyAsync(t => t.IncidentTypeID == model.TypeID);
            if (!typeExists)
            {
                return Problem(statusCode: 400, detail: $"Incident Type with ID {model.TypeID} does not exist.");
            }

            if (await ctx.IncidentReports.AnyAsync())
            {
                model.ReportID = await ctx.IncidentReports.MaxAsync(r => r.ReportID) + 1;
            }
            else
            {
                model.ReportID = 1;
            }

            ctx.IncidentReports.Add(model);
            await ctx.SaveChangesAsync();

            return CreatedAtRoute("GetIncidentReportById", new { id = model.ReportID }, model);
        }

        /// <summary>
        /// Updates an existing incident report.
        /// </summary>
        /// <param name="id">Report ID.</param>
        /// <param name="model">Updated report object.</param>
        /// <response code="204">Update successful.</response>
        /// <response code="400">ID mismatch or invalid data.</response>
        /// <response code="404">Report not found.</response>
        [HttpPut("{id}", Name = "UpdateIncidentReport")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(int id, [FromBody] IncidentReport model)
        {
            if (id != model.ReportID)
            {
                return Problem(statusCode: 400, detail: "ID in URL and body do not match.");
            }

            var dbReport = await ctx.IncidentReports.FindAsync(id);
            if (dbReport == null)
            {
                return Problem(statusCode: 404, detail: $"Report with ID {id} not found.");
            }

            var typeExists = await ctx.IncidentTypes.AnyAsync(t => t.IncidentTypeID == model.TypeID);
            if (!typeExists)
            {
                return Problem(statusCode: 400, detail: $"Incident Type with ID {model.TypeID} does not exist.");
            }

            dbReport.DateReport = model.DateReport;
            dbReport.DateSolved = model.DateSolved;
            dbReport.Status = model.Status;
            dbReport.TypeID = model.TypeID;

            await ctx.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an incident report.
        /// </summary>
        /// <param name="id">Report ID.</param>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="404">Not found.</response>
        [HttpDelete("{id}", Name = "DeleteIncidentReport")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await ctx.IncidentReports.FindAsync(id);
            if (report == null)
            {
                return Problem(statusCode: 404, detail: $"Report with ID {id} not found.");
            }
            try 
            {
                ctx.IncidentReports.Remove(report);
                await ctx.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return Problem(statusCode: 500, detail: "Could not delete report. Usually caused by existing related data (Affects). " + ex.Message);
            }

            return NoContent();
        }
    }
}