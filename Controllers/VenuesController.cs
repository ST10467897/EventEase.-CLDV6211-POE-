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
        private readonly IWebHostEnvironment _env;

        public VenuesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                    var imageUrl = await SaveImage(imageFile);
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
                        var imageUrl = await SaveImage(imageFile);
                        if (imageUrl == null)
                        {
                            ModelState.AddModelError("", "Invalid image. Please upload a JPG or PNG file under 5MB.");
                            return View(venue);
                        }
                        DeleteLocalImage(existing?.ImageUrl);
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
                DeleteLocalImage(venue.ImageUrl);
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Venue \"{venue.VenueName}\" deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImage(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext) || file.Length > 5 * 1024 * 1024)
                return null;

            var filename = $"{Guid.NewGuid()}{ext}";
            var folder = Path.Combine(_env.WebRootPath, "images", "venues");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, filename);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/venues/{filename}";
        }

        private void DeleteLocalImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http"))
                return;
            var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
