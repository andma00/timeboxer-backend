
namespace Timebox.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Timebox.Models;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using BCrypt.Net;

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
            user.PasswordHash = BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            if (BCrypt.Verify(request.Password, user.PasswordHash))
            {
                //Claims are pieces of information about the user that are encoded in the JWT token. They can include things like the user's ID, email, roles, and other relevant data. These claims can be used by the server to identify the user and determine their permissions when they make requests with the token.
                var claims = new List<Claim>
                {
                                                new Claim(ClaimTypes.NameIdentifier, user.Id),
                                                new Claim(ClaimTypes.Email, user.Email)
                };

                // TODO: Create security key (for now stored in environment variable, but should be stored in a secure vault in production)
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY", EnvironmentVariableTarget.User)!)
                                                                );

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                // Create token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(7),
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
            return Unauthorized("Invalid email or password");
        }

    }


}
