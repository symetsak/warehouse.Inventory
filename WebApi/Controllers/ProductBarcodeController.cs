using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductBarcodesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ProductBarcodesController(ApplicationDbContext db) => _db = db;

    // POST: api/ProductBarcodes
    [HttpPost]
    public async Task<ActionResult> Create(ProductBarcodeCreateDto dto)
    {
        var code = (dto.Code ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        var productExists = await _db.Products.AnyAsync(p => p.Id == dto.ProductId);
        if (!productExists) return NotFound("Product not found.");

        var codeExists = await _db.ProductBarcodes.AnyAsync(x => x.Code == code);
        if (codeExists) return Conflict("Το barcode υπάρχει ήδη.");

        // αν έρχεται ως primary, κάνε τα υπόλοιπα μη-primary
        if (dto.IsPrimary)
        {
            var primaries = await _db.ProductBarcodes
                .Where(x => x.ProductId == dto.ProductId && x.IsPrimary)
                .ToListAsync();
            primaries.ForEach(x => x.IsPrimary = false);
        }

        _db.ProductBarcodes.Add(new Core.Entities.ProductBarcode
        {
            ProductId = dto.ProductId,
            Code = code,
            Type = dto.Type,
            IsPrimary = dto.IsPrimary
        });

        await _db.SaveChangesAsync();
        return Created($"/api/productbarcodes/{code}", null);
    }

    // (προαιρετικό) DELETE: api/ProductBarcodes/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        code = (code ?? string.Empty).Trim();
        var entity = await _db.ProductBarcodes.FirstOrDefaultAsync(x => x.Code == code);
        if (entity is null) return NotFound();

        _db.ProductBarcodes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
