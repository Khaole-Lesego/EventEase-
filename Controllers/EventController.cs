using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public EventController(ApplicationDbContext context,BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: /Event
        public async Task<IActionResult> Index()
        {
            return View(await _context.Event.ToListAsync());
        }

        // GET: /Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // ADD "ImageFile" to [Bind] and upload logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,Description,ImageFile")] Event @event)
        {
            if (ModelState.IsValid)
            {
                if (@event.ImageFile != null && @event.ImageFile.Length > 0)
                {
                    var imageUrl = await _blobStorageService.UploadImageAsync(@event.ImageFile);
                    @event.ImageUrl = imageUrl;
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: /Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: /Event/Edit/5
        // ADD "ImageFile" to [Bind] and upload logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,Description,ImageFile")] Event @event)
        {
            if (id != @event.EventID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (@event.ImageFile != null && @event.ImageFile.Length > 0)
                    {
                        var newImageUrl = await _blobStorageService.UploadImageAsync(@event.ImageFile);
                        @event.ImageUrl = newImageUrl;
                    }
                    else
                    {
                        // Preserve existing image URL when no new file is uploaded
                        var existing = await _context.Event.AsNoTracking()
                            .FirstOrDefaultAsync(e => e.EventID == id);
                        if (existing != null)
                            @event.ImageUrl = existing.ImageUrl;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: /Event/Delete/5
        // Shows confirmation only if the event has no bookings; otherwise redirect with error.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null) return NotFound();

            // CRITICAL: check if any booking uses this event.
            bool isBooked = await _context.Booking.AnyAsync(b => b.EventID == id);
            if (isBooked)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has existing bookings.";
                return RedirectToAction(nameof(Index));   // Do NOT show delete confirmation.
            }

            return View(@event);
        }

        // POST: /Event/Delete/5
        // Performs the actual deletion (only reached if no bookings exist).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);
            if (@event != null)
            {
                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}