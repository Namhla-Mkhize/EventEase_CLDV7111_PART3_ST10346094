using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? availability)
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .ThenInclude(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bool isNumeric = int.TryParse(searchString, out int bookingId);
                bookings = bookings.Where(b =>
                    (isNumeric && b.BookingId == bookingId) ||
                    (b.Event != null && b.Event.EventName != null && b.Event.EventName.Contains(searchString)) ||
                    (b.Venue != null && b.Venue.VenueName != null && b.Venue.VenueName.Contains(searchString)));
            }

            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event != null && b.Event.EventTypeId == eventTypeId.Value);
            }

            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate <= endDate.Value);
            }

            if (availability.HasValue)
            {
                bookings = bookings.Where(b => b.Venue != null && b.Venue.Availability == availability.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.EventTypes = _context.EventTypes.ToList();
            ViewBag.SelectedEventType = eventTypeId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Availability = availability;

            return View(bookings.ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                bool doubleBooking = _context.Bookings.Any(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate.Date == booking.BookingDate.Date);

                if (doubleBooking)
                {
                    ModelState.AddModelError("",
                        "This venue is already booked on the selected date.");
                    ViewBag.Venues = _context.Venues.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View(booking);
                }

                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(booking);
        }

        public IActionResult Edit(int id)
        {
            var booking = _context.Bookings.Find(id);
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(booking);
        }

        [HttpPost]
        public IActionResult Edit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Bookings.Update(booking);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(booking);
        }

        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefault(b => b.BookingId == id);
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}