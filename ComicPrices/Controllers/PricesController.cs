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
                .Include(price => price.Comic)
                .Include(price => price.Seller)
                .OrderBy(price => price.SellerId).ThenBy(price => price.Amount)
                .Where(price => price.Comic.Name.ToLower() == comicName.ToLower())
                .ToListAsync();

            if (!prices.Any())
            {
                return NotFound();
            }
    
            var latestDate = prices.Max(price => price.DateRecorded.Date);

            var results = prices.Where(price => price.DateRecorded.Date == latestDate)
                .GroupBy(price => price.SellerId)
                .Select(group => group.First())
                .Select(price => new
                {
                    ComicName = price.Comic.Name,
                    ComicId = price.Comic.Id,
                    SellerName = price.Seller.Name,
                    SellerId = price.Seller.Id,
                    Price = price.Amount,
                    DateRecorded = price.DateRecorded
                })
                .OrderBy(result => result.Price)
                .ToList();

            return Ok(results);
        }
        [HttpGet("history/{comicId}/{sellerId}")]
        public async Task<IActionResult> GetPriceHistory(int comicId, int sellerId)
        {
            var prices = await _context.Prices
                .Where(price => price.Comic.Id == comicId && price.Seller.Id == sellerId)
                .Include(price => price.Comic)
                .Include(price => price.Seller)
                .OrderBy(price => price.DateRecorded)
                .ToListAsync();

            if (!prices.Any())
            {
                return NotFound();
            }

            var priceHistory = new
            {
                Comic = prices.First().Comic,
                Seller = prices.First().Seller,
                PriceDifference = prices.Last().Amount - prices.First().Amount,
                Data = prices.Select(price => new 
                {
                    Price = price.Amount,
                    DateRecorded = price.DateRecorded
                }).ToList()
            };
        
            return Ok(priceHistory);
        }
    }
}