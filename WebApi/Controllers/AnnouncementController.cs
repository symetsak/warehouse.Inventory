using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using System;

namespace WebApi.Controllers
{
    // Api/Controllers/AnnouncementsController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AnnouncementsController(ApplicationDbContext db) => _db = db;

        // Όλοι οι authenticated χρήστες βλέπουν
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<AnnouncementDto>> GetAll()
        {
            return await _db.Announcements
                .OrderByDescending(a => a.IsPinned)            
                .ThenByDescending(a => a.Date)                 
                .Select(a => new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Body = a.Body,
                    Date = a.Date,
                    PublisherFullName = a.PublisherFullName,
                    IsPinned = a.IsPinned,                      
                    PinnedAt = a.PinnedAt
                })
                .ToListAsync();
        }

        // Admin: Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AnnouncementDto>> Create(AnnouncementDto dto)
        {
            var entity = new Announcement
            {
                Title = dto.Title,
                Body = dto.Body,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                PublisherFullName = dto.PublisherFullName
            };
            _db.Announcements.Add(entity);
            await _db.SaveChangesAsync();
            dto.Id = entity.Id;
            return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, dto);
        }

        // Admin: Delete
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Announcements.FindAsync(id);
            if (entity is null) return NotFound();
            _db.Announcements.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Admin: Pin / Unpin
        [HttpPatch("{id}/pin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pin(int id)
        {
            var a = await _db.Announcements.FindAsync(id);
            if (a is null) return NotFound();
            a.IsPinned = true;
            a.PinnedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/unpin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unpin(int id)
        {
            var a = await _db.Announcements.FindAsync(id);
            if (a is null) return NotFound();
            a.IsPinned = false;
            a.PinnedAt = null;
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }

}
