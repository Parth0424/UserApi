using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserApi.Model;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(string firstName = null, string lastName = null, string email = null)
        {
            var query = _context.users.AsQueryable();

            if (!string.IsNullOrEmpty(firstName))
            {
                query = query.Where(u => u.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(u => u.LastName.Contains(lastName));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            var users = await query.ToListAsync();

            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (UserExistsByEmail(user.Email))
            {
                return Conflict("A user with this email already exists.");
            }

            if (UserExistsByUsername(user.UserName))
            {
                return Conflict("A user with this username already exists.");
            }

            _context.users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw; // Rethrow the exception if there is a problem saving the user
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (UserExistsByEmail(user.Email, id))
            {
                return Conflict("A user with this email already exists.");
            }

            if (UserExistsByUsername(user.UserName, id))
            {
                return Conflict("A user with this username already exists.");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               
                    throw; // Rethrow the exception if there is a concurrency problem
                
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExistsByEmail(string email, int? excludedUserId = null)
        {
            if (excludedUserId != null)
            {
                return _context.users.Any(u => u.Email == email && u.Id != excludedUserId);
            }
            else
            {
                return _context.users.Any(u => u.Email == email);
            }
        }

        private bool UserExistsByUsername(string username, int? excludedUserId = null)
        {
            if (excludedUserId != null)
            {
                return _context.users.Any(u => u.UserName == username && u.Id != excludedUserId);
            }
            else
            {
                return _context.users.Any(u => u.UserName == username);
            }
        }
    }

}
