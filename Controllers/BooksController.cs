using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.ViewModels;
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

        public async Task<IActionResult> Index(string? q, string? sort, string? category, int page = 1, int pageSize = 12)
        {
            sort ??= "newest";
            var baseQuery = _db.Books.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                baseQuery = baseQuery.Where(b => EF.Functions.Like(b.Title, $"%{q}%") || EF.Functions.Like(b.Author, $"%{q}%"));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                baseQuery = baseQuery.Where(b => b.Category == category);
            }
            baseQuery = sort switch
            {
                "price_asc" => baseQuery.OrderBy(b => b.Price),
                "price_desc" => baseQuery.OrderByDescending(b => b.Price),
                _ => baseQuery.OrderByDescending(b => b.CreatedAt)
            };

            var total = await baseQuery.CountAsync();
            var items = await baseQuery.Include(b => b.Images).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var vm = new BooksIndexViewModel
            {
                Items = items,
                Page = page,
                TotalPages = (int)System.Math.Ceiling(total / (double)pageSize),
                Query = q,
                Category = category,
                Sort = sort
            };
            return View(vm);
        }

        public IActionResult Create()
        {
            return View(new BookCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var seller = new Seller { Contact = model.SellerContact };
            _db.Sellers.Add(seller);
            await _db.SaveChangesAsync();

            // Calculate condition rating from yes/no questions
            var conditionRating = CalculateConditionRating(model);

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Version = model.Version,
                Price = model.Price,
                Condition = conditionRating,
                Category = model.Category,
                Location = model.Location,
                Description = model.Description,
                SellerId = seller.Id,
                Status = BookStatus.Pending  // Force pending status for WhatsApp verification
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            if (model.ImageUrls != null)
            {
                if (model.ImageUrls.Count == 1 && model.ImageUrls[0].Contains('\n'))
                {
                    var lines = model.ImageUrls[0].Split('\n');
                    model.ImageUrls = lines.ToList();
                }
                foreach (var url in model.ImageUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        _db.BookImages.Add(new BookImage { BookId = book.Id, Url = url });
                    }
                }
                await _db.SaveChangesAsync();
            }

            // Redirect to listing success page with WhatsApp instructions
            return RedirectToAction(nameof(ListingSuccess), new { id = book.Id });
        }

        private BookCondition CalculateConditionRating(BookCreateViewModel model)
        {
            // Count issues (weighted by severity)
            int issues = 0;
            if (model.IsPagesMissing) issues += 2;  // Most severe
            if (model.IsWaterDamaged) issues += 2;  // Most severe
            if (model.IsBindingBroken) issues += 1;
            if (model.IsCoverTorn) issues += 1;
            if (model.HasHighlighting) issues += 1;

            // Map to rating (fewer issues = better condition)
            return issues switch
            {
                0 => BookCondition.New,
                1 => BookCondition.LikeNew,
                2 or 3 => BookCondition.VeryGood,
                4 or 5 => BookCondition.Good,
                _ => BookCondition.Acceptable
            };
        }

        public async Task<IActionResult> ListingSuccess(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _db.Books
                .Include(b => b.Seller).ThenInclude(s => s.Contact)
                .Include(b => b.Images)
                .Include(b => b.Bids).ThenInclude(bid => bid.Bidder).ThenInclude(b => b!.Contact)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        public async Task<IActionResult> Buy(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            ViewData["Book"] = book;
            return View(new PurchaseRequestViewModel { BookId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(PurchaseRequestViewModel request)
        {
            var book = await _db.Books.FindAsync(request.BookId);
            if (book == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Book"] = book;
                return View(request);
            }

            var buyer = new Buyer { Contact = request.BuyerContact };
            _db.Buyers.Add(buyer);
            await _db.SaveChangesAsync();

            var pr = new PurchaseRequest
            {
                BookId = request.BookId,
                BuyerId = buyer.Id,
                OfferPrice = request.OfferPrice,
                Status = PurchaseRequestStatus.New
            };
            _db.PurchaseRequests.Add(pr);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Your request has been sent. Admin will contact you soon.";
            return RedirectToAction(nameof(Details), new { id = request.BookId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _db.Books.Include(b => b.Seller).ThenInclude(s => s.Contact).Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            var vm = new BookCreateViewModel
            {
                Title = book.Title,
                Author = book.Author,
                Version = book.Version,
                Price = book.Price,
                Condition = book.Condition,
                Category = book.Category,
                Location = book.Location,
                Description = book.Description,
                SellerContact = book.Seller?.Contact ?? new ContactInfo(),
                ImageUrls = book.Images.Select(i => i.Url).ToList()
            };
            ViewData["BookId"] = book.Id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCreateViewModel model)
        {
            var book = await _db.Books.Include(b => b.Seller).ThenInclude(s => s.Contact).Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            if (!ModelState.IsValid) { ViewData["BookId"] = id; return View(model); }

            book.Title = model.Title;
            book.Author = model.Author;
            book.Version = model.Version;
            book.Price = model.Price;
            book.Condition = model.Condition;
            book.Category = model.Category;
            book.Location = model.Location;
            book.Description = model.Description;
            if (book.Seller != null) book.Seller.Contact = model.SellerContact;

            _db.BookImages.RemoveRange(book.Images);
            if (model.ImageUrls != null)
            {
                if (model.ImageUrls.Count == 1 && model.ImageUrls[0].Contains('\n'))
                {
                    var lines = model.ImageUrls[0].Split('\n');
                    model.ImageUrls = lines.ToList();
                }
                foreach (var url in model.ImageUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        _db.BookImages.Add(new BookImage { BookId = book.Id, Url = url });
                }
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Listing updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, BookStatus status)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();
            book.Status = status;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Status updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
