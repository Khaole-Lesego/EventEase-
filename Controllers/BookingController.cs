using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Booking
        // Displays all bookings with their related Event and Venue data.
        // Added search functionality: search by BookingID (exact) or Event Name (partial).
        public async Task<IActionResult> Index(string searchString)
        {
            // Start with all bookings including related Event and Venue
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            // Apply search if a term is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // If search string can be parsed as an integer, search by BookingID
                if (int.TryParse(searchString, out int bookingId))
                {
                    bookings = bookings.Where(b => b.BookingID == bookingId);
                }
                else
                {
                    // Otherwise search by Event Name (case‑insensitive partial match)
                    bookings = bookings.Where(b => b.Event.EventName.Contains(searchString));
                }
            }

            var bookingsList = await bookings.ToListAsync();
            ViewBag.SearchString = searchString; // Keep search term in the input box
            return View(bookingsList);
        }




        // GET: /Booking/Create
        public IActionResult Create()
        {
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }




        // POST: /Booking/Create
        // ADD "ImageChoice" to [Bind]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,VenueID,StartDate,EndDate,ImageChoice")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool conflict = await _context.Booking
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                                   b.StartDate < booking.EndDate &&
                                   b.EndDate > booking.StartDate);

                if (conflict)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected date range.");
                }
                else
                {
                    booking.BookingDate = DateTime.Now;
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View(booking);
        }

        // GET: /Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();


          // Pass full objects so the view's JS can read ImageUrl for preview
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        // POST: /Booking/Edit/5
        // ADD "ImageChoice" to [Bind]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,EventID,VenueID,StartDate,EndDate,ImageChoice")] Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            if (ModelState.IsValid)
            {
                bool conflict = await _context.Booking
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                                   b.BookingID != booking.BookingID &&
                                   b.StartDate < booking.EndDate &&
                                   b.EndDate > booking.StartDate);

                if (conflict)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected date range.");
                }
                else
                {
                    try
                    {
                        var original = await _context.Booking.AsNoTracking()
                            .FirstOrDefaultAsync(b => b.BookingID == id);
                        if (original != null)
                            booking.BookingDate = original.BookingDate;

                        _context.Update(booking);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Booking updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BookingExists(booking.BookingID)) return NotFound();
                        else throw;
                    }
                }
            }

            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        // GET: /Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: /Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }
    }
}