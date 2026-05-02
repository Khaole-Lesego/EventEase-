using EventEase.Models;
using EventEase.Services;   // <-- add this
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        // Inject both DbContext and BlobStorageService
        public VenueController(ApplicationDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: /Venue
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }



        // GET: /Venue/List
        public async Task<IActionResult> List(string searchString)
        {
            var query = _context.Venue.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.Trim().ToLower();
                query = query.Where(v =>
                    v.VenueID.ToString().Contains(term) ||
                    v.VenueName.ToLower().Contains(term) ||
                    v.Location.ToLower().Contains(term));
            }

            var model = new LuxuryListPageViewModel<Venue>
            {
                EntityNamePlural = "Venues",
                HeaderKicker = "Curated Venue Register",
                HeaderTitle = "Grand Venue Collection",
                SearchPlaceholder = "Search by ID, name, or location",
                EmptyMessage = "No Venues Found",
                SearchString = searchString ?? string.Empty,
                ActiveTab = "venues",
                Items = await query.OrderBy(v => v.VenueName).ToListAsync()
            };

            return View(model);
        }

        // GET: /Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: "ImageFile" must be included in the [Bind] list.
        // Without it, ASP.NET Core's model binder skips the ImageFile property entirely,
        // so venue.ImageFile is always null and the upload block never executes.
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,ImageFile")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                // --- IMAGE UPLOAD ---
                if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                {
                    var imageUrl = await _blobStorageService.UploadImageAsync(venue.ImageFile);
                    venue.ImageUrl = imageUrl;
                }
                // --- END IMAGE UPLOAD ---

                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: /Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: "ImageFile" must also be included in the Edit [Bind] list for the same reason.
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,Capacity,ImageFile")] Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // --- IMAGE UPLOAD (if a new file is provided) ---
                    if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                    {
                        // Upload new image and update the URL
                        var newImageUrl = await _blobStorageService.UploadImageAsync(venue.ImageFile);
                        venue.ImageUrl = newImageUrl;
                    }
                    else
                    {
                        // Keep the existing image URL if no new file was uploaded
                        var existingVenue = await _context.Venue.AsNoTracking()
                            .FirstOrDefaultAsync(v => v.VenueID == id);
                        if (existingVenue != null)
                        {
                            venue.ImageUrl = existingVenue.ImageUrl;
                        }
                    }
                    // --- END IMAGE UPLOAD ---

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            bool hasBookings = await _context.Booking.AnyAsync(b => b.VenueID == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // POST: /Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue != null)
            {
                _context.Venue.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}