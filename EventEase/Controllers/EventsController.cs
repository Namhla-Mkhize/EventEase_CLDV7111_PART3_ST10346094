using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e =>
                    (e.EventName != null && e.EventName.Contains(searchString)) ||
                    (e.Venue != null && e.Venue.VenueName != null && e.Venue.VenueName.Contains(searchString)));
            }

            if (eventTypeId.HasValue)
            {
                events = events.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            if (startDate.HasValue)
            {
                events = events.Where(e => e.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.EventDate <= endDate.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.EventTypes = _context.EventTypes.ToList();
            ViewBag.SelectedEventType = eventTypeId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(events.ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Event newEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(newEvent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View(newEvent);
        }

        public IActionResult Edit(int id)
        {
            var newEvent = _context.Events.Find(id);
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View(newEvent);
        }

        [HttpPost]
        public IActionResult Edit(Event newEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Update(newEvent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Venues = _context.Venues.ToList();
            ViewBag.EventTypes = _context.EventTypes.ToList();
            return View(newEvent);
        }

        public IActionResult Delete(int id)
        {
            var newEvent = _context.Events.Find(id);
            return View(newEvent);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            bool hasBookings = _context.Bookings.Any(b => b.EventId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings.";
                return RedirectToAction("Index");
            }

            var newEvent = _context.Events.Find(id);
            if (newEvent != null)
            {
                _context.Events.Remove(newEvent);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}