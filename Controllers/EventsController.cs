using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseLocal.Models;

namespace EventEaseLocal.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventsController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(string? searchString, int? venueId, int? eventTypeId,
            DateTime? dateFrom, DateTime? dateTo, bool availableOnly = false)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                events = events.Where(e => e.EventName.Contains(searchString)
                    || (e.Description != null && e.Description.Contains(searchString)));

            if (venueId.HasValue)
                events = events.Where(e => e.VenueId == venueId.Value);

            if (eventTypeId.HasValue)
                events = events.Where(e => e.EventTypeId == eventTypeId.Value);

            if (availableOnly && (dateFrom.HasValue || dateTo.HasValue))
            {
                var busyVenueIds = _context.Bookings
                    .Where(b => (!dateFrom.HasValue || b.EventDate >= dateFrom.Value)
                             && (!dateTo.HasValue || b.EventDate <= dateTo.Value))
                    .Select(b => b.VenueId);
                events = events.Where(e => !busyVenueIds.Contains(e.VenueId));
            }
            else if (dateFrom.HasValue || dateTo.HasValue)
            {
                events = events.Where(e => e.Bookings.Any(b =>
                    (!dateFrom.HasValue || b.EventDate >= dateFrom.Value) &&
                    (!dateTo.HasValue || b.EventDate <= dateTo.Value)));
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["VenueFilter"] = venueId;
            ViewData["EventTypeFilter"] = eventTypeId;
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");
            ViewData["AvailableOnly"] = availableOnly;
            ViewData["Venues"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", venueId);
            ViewData["EventTypes"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", eventTypeId);
            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.Include(e => e.Venue).Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventId == id);
            if (evt == null) return NotFound();
            return View(evt);
        }

        public async Task<IActionResult> Create()
        { ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName"); ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name"); return View(); }

        [HttpPost] [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,Description,VenueId,EventTypeId")] Event evt)
        {
            if (ModelState.IsValid) { _context.Add(evt); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"Event \"{evt.EventName}\" created successfully."; return RedirectToAction(nameof(Index)); }
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", evt.VenueId); ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", evt.EventTypeId); return View(evt);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.FindAsync(id); if (evt == null) return NotFound();
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", evt.VenueId); ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", evt.EventTypeId); return View(evt);
        }

        [HttpPost] [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Description,VenueId,EventTypeId")] Event evt)
        {
            if (id != evt.EventId) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(evt); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"Event \"{evt.EventName}\" updated successfully."; }
                catch (DbUpdateConcurrencyException) { if (!_context.Events.Any(e => e.EventId == evt.EventId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", evt.VenueId); ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", evt.EventTypeId); return View(evt);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventId == id); if (evt == null) return NotFound();
            ViewData["HasDependencies"] = await _context.Bookings.AnyAsync(b => b.EventId == id); return View(evt);
        }

        [HttpPost, ActionName("Delete")] [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _context.Bookings.AnyAsync(b => b.EventId == id)) { TempData["ErrorMessage"] = "Cannot delete this event — it has associated bookings."; return RedirectToAction(nameof(Index)); }
            var evt = await _context.Events.FindAsync(id);
            if (evt != null) { _context.Events.Remove(evt); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"Event \"{evt.EventName}\" deleted successfully."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
