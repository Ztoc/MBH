using MBH.Data;
using MBH.Dtos;
using MBH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MBH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduledTasksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ScheduledTasksController(AppDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScheduledTaskCreateDto dto, CancellationToken ct)
        {
            if (dto.ScheduledTimeUtc.Kind != DateTimeKind.Utc)
                return BadRequest("ScheduledTimeUtc must be UTC (DateTime.Kind == Utc).");

            var entity = new ScheduledTask
            {
                Title = dto.Title,
                Description = dto.Description,
                ScheduledTime = dto.ScheduledTimeUtc
            };

            TryValidateModel(entity);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            _db.ScheduledTasks.Add(entity);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduledTask>>> List([FromQuery] bool? executed, CancellationToken ct)
        {
            var q = _db.ScheduledTasks.AsQueryable();
            if (executed.HasValue) q = q.Where(x => x.IsExecuted == executed.Value);
            var items = await q.OrderBy(x => x.IsExecuted).ThenBy(x => x.ScheduledTime).ToListAsync(ct);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var item = await _db.ScheduledTasks.FindAsync(new object?[] { id }, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ScheduledTaskUpdateDto dto, CancellationToken ct)
        {
            var item = await _db.ScheduledTasks.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (item is null) return NotFound();

            if (item.IsExecuted)
                return Conflict("Executed tasks cannot be rescheduled or modified.");

            if (dto.ScheduledTimeUtc <= DateTime.UtcNow)
                return BadRequest("ScheduledTimeUtc must be in the future (UTC).");

            item.Title = dto.Title;
            item.Description = dto.Description;
            item.ScheduledTime = dto.ScheduledTimeUtc;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var item = await _db.ScheduledTasks.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (item is null) return NotFound();

            if (item.IsExecuted)
                return Conflict("Executed tasks cannot be deleted.");

            _db.ScheduledTasks.Remove(item);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
