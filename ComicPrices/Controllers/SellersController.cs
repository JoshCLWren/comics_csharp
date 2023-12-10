using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicPrices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SellersController : ControllerBase
    {
        // DbContext that provides us access to database.
        private readonly Data.ComicsDbContext _context;
        
        // Constructor receives DbContext instance through dependency injection
        public SellersController(Data.ComicsDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Seller>> GetById(int id)
        {
            var seller = await _context.Sellers.FindAsync(id);

            if (seller == null)
            {
                return NotFound();
            }

            return Ok(seller);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            IQueryable<Models.Seller> query = _context.Sellers;

            int totalItems = await query.CountAsync();

            // apply the pagination
            var sellers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // always a good practice to return the total count and the page size for clients to handle on their end.
            return Ok(new { TotalItems = totalItems, PageSize = pageSize, Items = sellers });
        }
        [HttpGet("{id}/inventory")]
        public async Task<IActionResult> GetInventory(int id)
        {
            var prices = await _context.Prices
                .Include(price => price.Comic)
                .Where(price => price.SellerId == id)
                .GroupBy(p => p.ComicId)
                .Select(gp => gp.OrderByDescending(p => p.DateRecorded).First())
                .ToListAsync();

            if (!prices.Any())
            {
                return NotFound();
            }

            var inventory = prices.Select(price => new
            {
                Comic = price.Comic.Name,
                ComicId = price.ComicId,
                Price = price.Amount,
                DateRecorded = price.DateRecorded
            }).ToList();

            var totalComics = inventory.Count;
            var totalPrice = inventory.Sum(item => item.Price);

            return Ok(new { TotalComics = totalComics, TotalPrice = totalPrice, Comics = inventory });
        }
    }
}