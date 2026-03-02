using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeboxTask = Timebox.Models.Task;
using Microsoft.AspNetCore.Authorization;


namespace Timebox.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        //GET
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeboxTask>>> GetAllTasks()
        {
            List<TimeboxTask> tasks = await _context.Tasks.Include(t => t.Goals).ToListAsync();
            return tasks;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Timebox.Models.Task>> GetTaskById(string id)
        {
            TimeboxTask task = await _context.Tasks.Include(t => t.Goals).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            else
            {
                return task;
            }
        }
        //POST
        //
        [HttpPost]
        public async Task<ActionResult<TimeboxTask>> CreateTask(TimeboxTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }
        //PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(string id, TimeboxTask updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Description = updatedTask.Description;
            task.Duration = updatedTask.Duration;
            task.StartedAt = updatedTask.StartedAt;
            task.CompletedAt = updatedTask.CompletedAt;

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(t => t.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }
        //DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

