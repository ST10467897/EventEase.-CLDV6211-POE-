using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseLocal.Models;

namespace EventEaseLocal.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(string? searchString, int? venueId, DateTime? dateFilter)
        {
            var bookings = _context.Bookings.Include(b => b.Event).Include(b => b.Venue).AsQueryable();
            if (!string.IsNullOrEmpty(searchString)) { bookings = bookings.Where(b => b.Event!.EventName.Contains(searchString) || b.Venue!.VenueName.Contains(searchString)); }
            if (venueId.HasValue) { bookings = bookings.Where(b => b.VenueId == venueId.Value); }
            if (dateFilter.HasValue) { bookings = bookings.Where(b => b.EventDate.Date == dateFilter.Value.Date); }
            ViewData["CurrentFilter"] = searchString; ViewData["VenueFilter"] = venueId; ViewData["DateFilter"] = dateFilter?.ToString("yyyy-MM-dd");
            ViewData["Venues"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", venueId);
            return View(await bookings.OrderBy(b => b.EventDate).ThenBy(b => b.StartTime).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Event).Include(b => b.Venue).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound(); return View(booking);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName"); return View();
        }

        [HttpPost] [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,VenueId,EventDate,StartTime,EndTime")] Booking booking)
        {
            if (booking.EndTime <= booking.StartTime) { ModelState.AddModelError("EndTime", "End time must be after start time."); }
            if (ModelState.IsValid)
            {
                var conflict = await _context.Bookings.AnyAsync(b => b.VenueId == booking.VenueId && b.EventDate.Date == booking.EventDate.Date && b.StartTime < booking.EndTime && b.EndTime > booking.StartTime);
                if (conflict) { ModelState.AddModelError(string.Empty, "This venue is already booked during the selected time slot. Please choose a different time or venue."); }
            }
            if (ModelState.IsValid) { _context.Add(booking); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Booking created successfully."; return RedirectToAction(nameof(Index)); }
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId); return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.FindAsync(id); if (booking == null) return NotFound();
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId); return View(booking);
        }

        [HttpPost] [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,EventDate,StartTime,EndTime")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();
            if (booking.EndTime <= booking.StartTime) { ModelState.AddModelError("EndTime", "End time must be after start time."); }
            if (ModelState.IsValid)
            {
                var conflict = await _context.Bookings.AnyAsync(b => b.BookingId != booking.BookingId && b.VenueId == booking.VenueId && b.EventDate.Date == booking.EventDate.Date && b.StartTime < booking.EndTime && b.EndTime > booking.StartTime);
                if (conflict) { ModelState.AddModelError(string.Empty, "This venue is already booked during the selected time slot."); }
            }
            if (ModelState.IsValid)
            {
                try { _context.Update(booking); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Booking updated successfully."; }
                catch (DbUpdateConcurrencyException) { if (!_context.Bookings.Any(b => b.BookingId == booking.BookingId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId); return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Event).Include(b => b.Venue).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound(); return View(booking);
        }

        [HttpPost, ActionName("Delete")] [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null) { _context.Bookings.Remove(booking); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Booking deleted successfully."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
