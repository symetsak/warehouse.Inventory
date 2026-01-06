using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Entities;
using Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public InventoryController(ApplicationDbContext db) => _db = db;

        // GET: api/Inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAll()
        {
            var list = await _db.Inventories
                .Include(i => i.Warehouse)
                .Include(i => i.User)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ScanCode = i.ScanCode,
                    Code = i.Code,
                    Action = i.Action,
                    Quantity = i.Quantity,
                    Timestamp = i.Timestamp,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    UserId = i.UserId,
                    UserName = i.User.FullName
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Inventory/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryDto>> Get(int id)
        {
            var i = await _db.Inventories
                .Include(x => x.Warehouse)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (i == null) return NotFound();

            return Ok(new InventoryDto
            {
                Id = i.Id,
                ScanCode = i.ScanCode,
                Code = i.Code,
                Action = i.Action,
                Quantity = i.Quantity,
                Timestamp = i.Timestamp,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                UserId = i.UserId,
                UserName = i.User.FullName
            });
        }

        // POST: api/Inventory
        [HttpPost]
        public async Task<ActionResult<InventoryDto>> Create(CreateInventoryDto dto)
        {
            if (!await _db.Warehouses.AnyAsync(w => w.Id == dto.WarehouseId))
                return BadRequest($"Invalid warehouseId: {dto.WarehouseId}");

            if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest($"Invalid userId: {dto.UserId}");

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Code == dto.Code);

            if (product == null)
                return BadRequest("Invalid product code");

            if (dto.Action == "Output")
            {
                if (product.Quantity <= 0 || dto.Quantity > product.Quantity)
                {
                    return BadRequest(new
                    {
                        message = "Το απόθεμα δεν επαρκεί για αυτή την ενέργεια"
                    });
                }
            }

            // Δημιουργία Inventory
            var entity = new Inventory
            {
                ScanCode = dto.ScanCode,
                Code = dto.Code,
                Action = dto.Action,
                Quantity = dto.Quantity,
                WarehouseId = dto.WarehouseId,
                UserId = dto.UserId,
                Timestamp = DateTime.UtcNow
            };

            _db.Inventories.Add(entity);

            // Ενημέρωση προϊόντος
            product.Quantity += dto.Action == "Input"
                ? dto.Quantity
                : -dto.Quantity;

            product.TotalValue = product.Quantity * product.Price;

            await _db.SaveChangesAsync();

            var response = new InventoryDto
            {
                Id = entity.Id,
                ScanCode = entity.ScanCode,
                Code = entity.Code,
                Action = entity.Action,
                Quantity = entity.Quantity,
                Timestamp = entity.Timestamp,
                WarehouseId = entity.WarehouseId,
                WarehouseName = (await _db.Warehouses.FindAsync(entity.WarehouseId))!.Name,
                UserId = entity.UserId,
                UserName = (await _db.Users.FindAsync(entity.UserId))!.FullName
            };

            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }

        // PUT: api/Inventory/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateInventoryDto dto)
        {
            var entity = await _db.Inventories.FindAsync(id);
            if (entity == null) return NotFound();

            if (!await _db.Warehouses.AnyAsync(w => w.Id == dto.WarehouseId))
                return BadRequest($"Invalid warehouseId: {dto.WarehouseId}");

            if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest($"Invalid userId: {dto.UserId}");

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Code == dto.Code);

            if (product == null)
                return BadRequest("Invalid product code");

            int oldDelta = entity.Action == "Input"
                ? entity.Quantity
                : -entity.Quantity;

            int newDelta = dto.Action == "Input"
                ? dto.Quantity
                : -dto.Quantity;

            int diff = newDelta - oldDelta;

            if (product.Quantity + diff < 0)
            {
                return BadRequest(new
                {
                    message = "Το απόθεμα δεν επαρκεί για αυτή την ενέργεια"
                });
            }

            entity.ScanCode = dto.ScanCode;
            entity.Code = dto.Code;
            entity.Action = dto.Action;
            entity.Quantity = dto.Quantity;
            entity.WarehouseId = dto.WarehouseId;
            entity.UserId = dto.UserId;

            product.Quantity += diff;
            product.TotalValue = product.Quantity * product.Price;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Inventory/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Inventories.FindAsync(id);
            if (entity == null) return NotFound();

            _db.Inventories.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
