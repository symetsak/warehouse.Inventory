using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Entities;
using Shared.DTOs;
using Infrastructure.Security;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public UsersController(ApplicationDbContext db) => _db = db;

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var list = await _db.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Mobile = u.Mobile,
                    Email = u.Email,
                    Username = u.Username,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Users/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> Get(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();

            return Ok(new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Mobile = u.Mobile,
                Email = u.Email,
                Username = u.Username,
                Role = u.Role
            });
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
        {
            // (εδώ μπορείς να προσθέσεις validation ή hashing του password)
            var entity = new User
            {
                FullName = dto.FullName,
                Mobile = dto.Mobile,
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Role = dto.Role
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync();

            var result = new UserDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Mobile = entity.Mobile,
                Email = entity.Email,
                Username = entity.Username,
                Role = entity.Role
            };

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        // PUT: api/Users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateUserDto dto)
        {
            var entity = await _db.Users.FindAsync(id);
            if (entity == null) return NotFound();

            entity.FullName = dto.FullName;
            entity.Mobile = dto.Mobile;
            entity.Email = dto.Email;
            entity.Username = dto.Username;
            entity.PasswordHash = dto.Password; // ή μην αλλάζεις password εδώ
            entity.Role = dto.Role;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Users.FindAsync(id);
            if (entity == null) return NotFound();

            _db.Users.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
