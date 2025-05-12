using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;

namespace EventEase.Controllers
{
    public class NewBookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public NewBookingsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: NewBookings
        [HttpGet]
        public async Task<IActionResult> Index(string searchQuery)
        {
            //
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchQuery) ||
                    b.Event.EventName.Contains(searchQuery));
            }

            return View(await bookings.ToListAsync());
        }
    }
}
