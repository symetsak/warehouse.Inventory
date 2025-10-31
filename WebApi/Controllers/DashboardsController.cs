using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Shared.DTOs;

namespace WebApi.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        // GET: api/Dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            // Χρησιμοποιούμε UTC γιατί το Inventory.Timestamp είναι UTC (GETUTCDATE()).
            var now = DateTime.UtcNow;

            // Σήμερα (UTC)
            var todayStart = now.Date;
            var tomorrowStart = todayStart.AddDays(1);

            // Εβδομάδα (Δευτέρα–Κυριακή)
            // DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
            int dow = (int)todayStart.DayOfWeek;
            var weekStart = todayStart.AddDays(dow == 0 ? -6 : -(dow - 1)); // Δευτέρα
            var weekEnd = weekStart.AddDays(7);

            // Μήνας
            var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            // Μετρήσεις με range filters στο Timestamp
            var todayCount = await _db.Inventories
                .AsNoTracking()
                .CountAsync(i => i.Timestamp >= todayStart && i.Timestamp < tomorrowStart);

            var weekCount = await _db.Inventories
                .AsNoTracking()
                .CountAsync(i => i.Timestamp >= weekStart && i.Timestamp < weekEnd);

            var monthCount = await _db.Inventories
                .AsNoTracking()
                .CountAsync(i => i.Timestamp >= monthStart && i.Timestamp < nextMonthStart);

            var dto = new DashboardStatsDto
            {
                TodayInventories = todayCount,
                WeekInventories = weekCount,
                MonthInventories = monthCount
            };

            return Ok(dto);
        }

        // GET: api/Dashboard/warehouse-stats
        [HttpGet("warehouse-stats")]
        public async Task<ActionResult<IEnumerable<WarehouseStatsDto>>> GetWarehouseStats()
        {
            // Ομαδοποίηση προϊόντων ανά αποθήκη
            var stats = await _db.Products
                .AsNoTracking()
                .GroupBy(p => new { p.WarehouseId, p.Warehouse!.Name })
                .Select(g => new WarehouseStatsDto
                {
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = g.Key.Name,
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(x => x.TotalValue) 
                })
                .OrderBy(s => s.WarehouseName)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
