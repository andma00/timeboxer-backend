using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeboxTask = Timebox.Models.Task;
using Timebox.Models;
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();
            var tasks = await _context.Tasks
                            .Where(t => t.UserId == userId)
                            .Include(t => t.Goals)
                                .ToListAsync();
            return tasks;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Timebox.Models.Task>> GetTaskById(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();
            TimeboxTask task = await _context.Tasks.Include(t => t.Goals).FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
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
        public async Task<ActionResult<TimeboxTask>> CreateTask(TaskCreateDto taskDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var task = new TimeboxTask
            {
                UserId = userId,
                Description = taskDto.Description,
                Duration = taskDto.Duration,
                Goals = taskDto.Goals
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }
        //PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(string id, TaskUpdateDto updateTaskDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            //Handling goals update
            var incomingGoalIds = updateTaskDto.Goals.Select(g => g.Id).ToList();
            var goalsToRemove = task.Goals.Where(g => !incomingGoalIds.Contains(g.Id)).ToList();
            _context.Goals.RemoveRange(goalsToRemove);

            foreach (var incomingGoal in updateTaskDto.Goals)
            {
                var existingGoal = task.Goals.FirstOrDefault(g => g.Id == incomingGoal.Id);
                if (existingGoal != null)
                {
                    existingGoal.Description = incomingGoal.Description;
                    existingGoal.IsCompleted = incomingGoal.IsCompleted;
                }
                else
                {
                    incomingGoal.TaskId = task.Id;
                    task.Goals.Add(incomingGoal);
                }
            }

            task.Description = updateTaskDto.Description;
            task.Duration = updateTaskDto.Duration;
            task.StartedAt = updateTaskDto.StartedAt;
            task.CompletedAt = updateTaskDto.CompletedAt;

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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
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

