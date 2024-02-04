using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using TaskCoreApi.EFDbContext;
using TaskCoreApi.Model;

namespace TaskCoreApi.Controllers
{
    //[Route("api/[controller]/[actionname]")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;

        private IConfiguration _config;


        public UsersController(UserDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/Users
        [HttpGet("GetAllUsers")]
        [Authorize(Roles="Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("GetUserById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetUser(string id)
        {

            // Check if userId is a valid UUID
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid userId. It should be a valid UUID." });
            }

            // Retrieve the user by userId
            var user = await _context.Users.FindAsync(id);

            // Check if the user exists
            if (user == null)
            {
                return NotFound(new { message = $"User with id {id} not found." });
            }


            return Ok(user);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("UpdateUser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            // Check if userId is a valid UUID
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid userId. It should be a valid UUID." });
            }


            _context.Entry(user).State = EntityState.Modified;

            try
            {
                // Check if the user was found and updated
                if (UserExists(id))
                {
                    await _context.SaveChangesAsync();
                    // Status code 200
                }
                else
                {
                    return NotFound(new { message = $"User with id {id} not found." });
                }

                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("CreateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> PostUser([FromBody]User user)
        {
         
           
            try
            {
                if (string.IsNullOrEmpty(user.Id))
                {
                    user.Id = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(user.Id))
                    {
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }

                }
              
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("LoginUser")]
        public async Task<ActionResult<User>> LoginUser([FromBody]UserLogin userLogin)
        {
           
            var userData = await _context.Users.FirstOrDefaultAsync(u => u.Username== userLogin.Username && u.Password== userLogin.Password);
            // Check if the user was not found 
            if (userData == null)
            {
                return NotFound(new { message = $"User not found with Username {userLogin.Username} && Password {userLogin.Password}" });
            }
            else
            {
                string role = string.Empty;
                if (userData.IsAdmin==true)
                {
                     role = "Admin";
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userData.Username),  // Replace with the actual username
                    new Claim(ClaimTypes.Role, role),     // Replace with the actual role
                    // Add more claims as needed
                };

                var Sectoken = new JwtSecurityToken
                    (
                        _config["Jwt:Issuer"],
                        _config["Jwt:Audience"],
                        claims,
                        expires: DateTime.Now.AddMinutes(120),
                        signingCredentials: credentials
                    );


                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);
                return Ok(token);
            }

           
           

        }

        // DELETE: api/Users/5
        [HttpDelete("DeleteUser/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {

            // Check if userId is a valid UUID
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid userId. It should be a valid UUID." });
            }

            var user = await _context.Users.FindAsync(id);
            // Check if the user was not found 
            if (user == null)
            {
                return NotFound(new { message = $"User with id {id} not found." });
            }

            // Check if the user was found and deleted
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();


            return NoContent();

        }

        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
