using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ComicPrices.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class PricesController : ControllerBase
    {
        // DbContext that provides us access to database.
        private readonly Data.ComicsDbContext _context;
        
        // Constructor receives DbContext instance through dependency injection
        public PricesController(Data.ComicsDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Price>> GetById(int id)
        {
            var price = await _context.Prices.FindAsync(id);

            if (price == null)
            {
                return NotFound();
            }

            return Ok(price);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            IQueryable<Models.Price> query = _context.Prices;

            int totalItems = await query.CountAsync();

            // apply the pagination
            var prices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // always a good practice to return the total count and the page size for clients to handle on their end.
            return Ok(new { TotalItems = totalItems, PageSize = pageSize, Items = prices });
        }
        
        [HttpGet("search/{comicName}")]
        public async Task<IActionResult> GetByComicName(string comicName)
        {
            var prices = await _context.Prices
                // Eager Loading
                .Include(price => price.Comic)
                .Include(price => price.Seller)
                // Order by SellerId and Amount
                .OrderBy(price => price.SellerId).ThenBy(price => price.Amount)
                // Filter
                .Where(price => price.Comic.Name.ToLower() == comicName.ToLower())
                .ToListAsync();  // Make sure to call ToListAsync() before AsEnumerable()

            var latestDate = prices.Max(p => p.DateRecorded.Date);

            var results = prices.AsEnumerable()
                .Where(price => price.DateRecorded.Date == latestDate)
                // Group by SellerId
                .GroupBy(price => price.SellerId)
                .Select(group => group.First())
                // Projection
                .Select(price => new
                {
                    ComicName = price.Comic.Name,
                    SellerName = price.Seller.Name,
                    SellerId = price.SellerId,
                    Price = price.Amount,
                    DateRecorded = price.DateRecorded
                })
                // Order the results by Price in ascending order
                .OrderBy(result => result.Price)
                .ToList();

            if (!results.Any())
            {
                return NotFound();
            }

            return Ok(results);
        }
    }
}