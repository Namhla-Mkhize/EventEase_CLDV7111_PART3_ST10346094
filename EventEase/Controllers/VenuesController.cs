using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public VenuesController(ApplicationDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        public IActionResult Index(string searchString, bool? availability)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v =>
                    (v.VenueName != null && v.VenueName.Contains(searchString)) ||
                    (v.Location != null && v.Location.Contains(searchString)));
            }

            if (availability.HasValue)
            {
                venues = venues.Where(v => v.Availability == availability.Value);
            }

            ViewBag.SearchString = searchString;
            ViewBag.Availability = availability;
            return View(venues.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                if (venue.ImageFile != null)
                {
                    venue.ImageURL = await _blobStorageService.UploadImageAsync(venue.ImageFile);
                }
                else
                {
                    venue.ImageURL = "";
                }
                _context.Venues.Add(venue);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(venue);
        }

        public IActionResult Edit(int id)
        {
            var venue = _context.Venues.Find(id);
            return View(venue);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Venue venue)
        {
            if (ModelState.IsValid)
            {
                if (venue.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(venue.ImageURL))
                    {
                        await _blobStorageService.DeleteImageAsync(venue.ImageURL);
                    }
                    venue.ImageURL = await _blobStorageService.UploadImageAsync(venue.ImageFile);
                }
                _context.Venues.Update(venue);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            bool hasBookings = _context.Bookings.Any(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
                return RedirectToAction("Index");
            }

            var venue = _context.Venues.Find(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var venue = _context.Venues.Find(id);
            return View(venue);
        }

        public IActionResult Details(int id)
        {
            var venue = _context.Venues.Find(id);
            return View(venue);
        }
    }
}