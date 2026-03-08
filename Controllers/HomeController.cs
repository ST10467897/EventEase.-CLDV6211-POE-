using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseLocal.Models;

namespace EventEaseLocal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["VenueCount"] = await _context.Venues.CountAsync();
            ViewData["EventCount"] = await _context.Events.CountAsync();
            ViewData["BookingCount"] = await _context.Bookings.CountAsync();

            var upcomingBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Where(b => b.EventDate >= DateTime.Today)
                .OrderBy(b => b.EventDate)
                .ThenBy(b => b.StartTime)
                .Take(5)
                .ToListAsync();

            return View(upcomingBookings);
        }
    }
}
