using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseLocal.Models;
using EventEaseLocal.Services;

namespace EventEaseLocal.Controllers
{
    [Authorize]
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorage;

        public VenuesController(ApplicationDbContext context, IBlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
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
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity")] Venue venue, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    var imageUrl = await _blobStorage.UploadVenueImageAsync(imageFile);
                    if (imageUrl == null)
                    {
                        ModelState.AddModelError("", "Invalid image. Please upload a JPG or PNG file under 5MB.");
                        return View(venue);
                    }
                    venue.ImageUrl = imageUrl;
                }
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
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    if (imageFile != null)
                    {
                        var imageUrl = await _blobStorage.UploadVenueImageAsync(imageFile);
                        if (imageUrl == null)
                        {
                            ModelState.AddModelError("", "Invalid image. Please upload a JPG or PNG file under 5MB.");
                            return View(venue);
                        }
                        await _blobStorage.DeleteVenueImageAsync(existing?.ImageUrl);
                        venue.ImageUrl = imageUrl;
                    }
                    else
                    {
                        venue.ImageUrl = existing?.ImageUrl;
                    }
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venues.Any(e => e.VenueId == venue.VenueId)) return NotFound();
                    else throw;
                }
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
            if (venue != null)
            {
                await _blobStorage.DeleteVenueImageAsync(venue.ImageUrl);
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
