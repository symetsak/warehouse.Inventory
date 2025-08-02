using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Entities;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db) => _db = db;

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var list = await _db.Products
                .Include(p => p.Warehouse)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Unit = p.Unit,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    TotalValue = p.TotalValue,
                    WarehouseId = p.WarehouseId,
                    WarehouseName = p.Warehouse!.Name

                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var p = await _db.Products
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            var dto = new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Unit = p.Unit,
                Quantity = p.Quantity,
                Price = p.Price,
                TotalValue = p.TotalValue,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse!.Name

            };

            return Ok(dto);
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
        {
            var entity = new Product
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Unit = dto.Unit,
                Quantity = dto.Quantity,
                Price = dto.Price,
                TotalValue = dto.Quantity * dto.Price,
                WarehouseId = dto.WarehouseId
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();

            // Map to DTO for response
            var response = new ProductDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                Unit = entity.Unit,
                Quantity = entity.Quantity,
                Price = entity.Price,
                TotalValue = entity.TotalValue,
                WarehouseId = entity.WarehouseId,
                WarehouseName = (await _db.Warehouses.FindAsync(entity.WarehouseId))!.Name
            };

            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }

        // PUT: api/Products/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateProductDto dto)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity == null) return NotFound();

            // Update fields
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Unit = dto.Unit;
            entity.Quantity = dto.Quantity;
            entity.Price = dto.Price;
            entity.TotalValue = dto.Quantity * dto.Price;
            entity.WarehouseId = dto.WarehouseId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // εκτελείς πρώτα το await
                var exists = await _db.Products.AnyAsync(e => e.Id == id);
                if (!exists)
                    return NotFound();

                // αν υπάρχει, ξαναρίχνεις το exception
                throw;
            }


            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity == null) return NotFound();

            _db.Products.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
