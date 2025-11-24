using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class BooksController : Controller
    {
        private readonly MarketplaceDbContext _db;

        public BooksController(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Books.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(b => EF.Functions.Like(b.Title, $"%{q}%") || EF.Functions.Like(b.Author, $"%{q}%"));
            }
            var books = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            ViewData["Query"] = q;
            return View(books);
        }

        public IActionResult Create()
        {
            return View(new Book());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.Books.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        public async Task<IActionResult> Buy(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            ViewData["Book"] = book;
            return View(new PurchaseRequest { BookId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(PurchaseRequest request)
        {
            var book = await _db.Books.FindAsync(request.BookId);
            if (book == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Book"] = book;
                return View(request);
            }

            _db.PurchaseRequests.Add(request);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Your request has been sent. Admin will contact you soon.";
            return RedirectToAction(nameof(Details), new { id = request.BookId });
        }
    }
}

