using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Entities;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public WarehousesController(ApplicationDbContext db) => _db = db;

        // GET: api/Warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll()
        {
            var list = await _db.Warehouses
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Address = w.Address
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Warehouses/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<WarehouseDto>> Get(int id)
        {
            var w = await _db.Warehouses.FindAsync(id);
            if (w == null) return NotFound();

            return Ok(new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Address = w.Address
            });
        }

        // POST: api/Warehouses
        [HttpPost]
        public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto dto)
        {
            var entity = new Warehouse
            {
                Name = dto.Name,
                Address = dto.Address
            };

            _db.Warehouses.Add(entity);
            await _db.SaveChangesAsync();

            var result = new WarehouseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address
            };

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        // PUT: api/Warehouses/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateWarehouseDto dto)
        {
            var entity = await _db.Warehouses.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Name = dto.Name;
            entity.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Warehouses/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Warehouses.FindAsync(id);
            if (entity == null) return NotFound();

            _db.Warehouses.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
