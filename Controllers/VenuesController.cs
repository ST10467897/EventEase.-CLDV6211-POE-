using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseLocal.Models;

namespace EventEaseLocal.Controllers
{
    [Authorize]
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var venues = _context.Venues.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString) || v.Location.Contains(searchString));
            }
            ViewData["CurrentFilter"] = searchString;
            return View(await venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.Include(v => v.Events).Include(v => v.Bookings).FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        public IActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue)
        {
            if (id != venue.VenueId) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(venue); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" updated successfully."; }
                catch (DbUpdateConcurrencyException) { if (!_context.Venues.Any(e => e.VenueId == venue.VenueId)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();
            ViewData["HasDependencies"] = await _context.Bookings.AnyAsync(b => b.VenueId == id) || await _context.Events.AnyAsync(e => e.VenueId == id);
            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _context.Bookings.AnyAsync(b => b.VenueId == id) || await _context.Events.AnyAsync(e => e.VenueId == id))
            { TempData["ErrorMessage"] = "Cannot delete this venue — it has associated events or bookings."; return RedirectToAction(nameof(Index)); }
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null) { _context.Venues.Remove(venue); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" deleted successfully."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
