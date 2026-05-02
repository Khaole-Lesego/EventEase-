using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.ViewModels;

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
            var bookings = BuildBookingSearchQuery(searchString);
            var bookingsList = await bookings.ToListAsync();
            ViewBag.SearchString = searchString;
            return View(bookingsList);
        }

        // GET: /Booking/List
        public async Task<IActionResult> List(string searchString)
        {
            var model = new LuxuryListPageViewModel<Booking>
            {
                EntityNamePlural = "Bookings",
                HeaderKicker = "Reservations Ledger",
                HeaderTitle = "Prestige Booking Registry",
                SearchPlaceholder = "Search by ID, event, venue, or YYYY-MM-DD",
                EmptyMessage = "No Bookings Found",
                SearchString = searchString ?? string.Empty,
                ActiveTab = "bookings",
                Items = await BuildBookingSearchQuery(searchString)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync()
            };

            return View(model);
        }

        private IQueryable<Booking> BuildBookingSearchQuery(string? searchString)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(searchString))
            {
                return bookings;
            }

            var term = searchString.Trim().ToLower();
            DateTime parsedDate;
            bool hasDate = DateTime.TryParse(searchString, out parsedDate);

            bookings = bookings.Where(b =>
                b.BookingID.ToString().Contains(term) ||
                (b.Event != null && b.Event.EventName.ToLower().Contains(term)) ||
                (b.Venue != null && b.Venue.VenueName.ToLower().Contains(term)) ||
                (hasDate && (b.StartDate.Date == parsedDate.Date || b.EndDate.Date == parsedDate.Date))
            );

            return bookings;
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