using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Views
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly string _blobConnectionString;
        private readonly string _blobContainerName;

        public VenuesController(EventEaseContext context, IConfiguration configuration)
        {
            _context = context;
            _blobConnectionString = configuration.GetValue<string>("AzureBlobStorage:ConnectionString");
            _blobContainerName = configuration.GetValue<string>("AzureBlobStorage:ContainerName");
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Validation to prevent duplicate Venues
                bool duplicateExists = await _context.Venue
                    .AnyAsync(v => v.VenueName.ToLower() == venue.VenueName.ToLower() &&
                                   v.Location.ToLower() == venue.Location.ToLower());

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "A venue with the same name and location already exists.");
                    return View(venue);
                }

                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!imageFile.ContentType.StartsWith("image/"))
                        {
                            ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
                            return View(venue);
                        }

                        string blobUrl = await UploadImageToBlob(imageFile);
                        venue.ImageURL = blobUrl;
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageURL")] Venue venue, IFormFile imageFile, bool deleteImage)
        {
            if (id != venue.VenueId) return NotFound();

            var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
            if (existingVenue == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Validation to prevent duplicate Venues
                bool duplicateExists = await _context.Venue
                    .AnyAsync(v => v.VenueId != venue.VenueId &&
                                   v.VenueName.ToLower() == venue.VenueName.ToLower() &&
                                   v.Location.ToLower() == venue.Location.ToLower());

                if (duplicateExists)
                {
                    ModelState.AddModelError(string.Empty, "A venue with the same name and location already exists.");
                    return View(venue);
                }

                try
                {
                    if (deleteImage && !string.IsNullOrEmpty(existingVenue.ImageURL))
                    {
                        await DeleteImageFromBlob(existingVenue.ImageURL);
                        venue.ImageURL = null;
                    }
                    else
                    {
                        venue.ImageURL = existingVenue.ImageURL;
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingVenue.ImageURL))
                        {
                            await DeleteImageFromBlob(existingVenue.ImageURL);
                        }

                        string newImageUrl = await UploadImageToBlob(imageFile);
                        venue.ImageURL = newImageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId)) return NotFound();
                    else throw;
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FirstOrDefaultAsync(e => e.VenueId == id);
            if (venue == null) return NotFound();

            bool hasActiveBookings = await _context.Booking
                .AnyAsync(b => b.VenueId == id && b.BookingDate > DateTime.Now);

            if (hasActiveBookings)
            {
                ModelState.AddModelError(string.Empty, "This Venue cannot be deleted because there are active bookings.");
                return View(venue);
            }

            if (!string.IsNullOrEmpty(venue.ImageURL))
            {
                await DeleteImageFromBlob(venue.ImageURL);
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }

        private async Task<string> UploadImageToBlob(IFormFile imageFile)
        {
            //Method to upload image to Blob Storage
            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        private async Task DeleteImageFromBlob(string imageUrl)
        {
            //Method to delete image to Blob Storage
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_blobContainerName);

            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}