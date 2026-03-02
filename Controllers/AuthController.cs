using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timebox.Models;

namespace Timebox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return the created user with their new ID
            return user;
        }
    }
}
