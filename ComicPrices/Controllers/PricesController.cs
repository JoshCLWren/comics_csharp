using ComicPrices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ComicPrices.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class PricesController(Data.ComicsDbContext context) : ControllerBase
    {
        // DbContext that provides us access to database.

        // Constructor receives DbContext instance through dependency injection
        [HttpGet("{id}")]
        public async Task<ActionResult<Price>> GetById(int id)
        {
            var price = await context.Prices.FindAsync(id);

            if (price == null)
            {
                return NotFound();
            }

            return Ok(price);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            IQueryable<Price> query = context.Prices;

            int totalItems = await query.CountAsync();

            // apply the pagination
            var prices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // always a good practice to return the total count and the page size for clients to handle on their end.
            return Ok(new { TotalItems = totalItems, PageSize = pageSize, Items = prices });
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> GetByComicName([FromQuery] string comicName)
        {
            const int matchThreshold = 80;

            var prices = await context.Prices
                .Include(price => price.Comic)
                .Include(price => price.Seller)
                .OrderBy(price => price.SellerId).ThenBy(price => price.Amount)
                .ToListAsync();

            var directMatches = prices
                .Where(price => price.Comic?.Name?.ToLower() == comicName.ToLower())
                .ToList();

            bool isFuzzySearchUsed = false;
            int fuzzyMatchScore = 0;

            List<Price> matches;

            if (directMatches.Any())
            {
                matches = directMatches;
            }
            else
            {
                matches = prices
                    .Where(price =>
                    {
                        fuzzyMatchScore = FuzzySharp.Fuzz.PartialRatio(price.Comic?.Name?.ToLower(), comicName.ToLower());
                        return fuzzyMatchScore > matchThreshold;
                    })
                    .ToList();
                isFuzzySearchUsed = true;
            }

            if (!matches.Any())
            {
                return NotFound();
            }

            var latestDate = matches.Max(price => price.DateRecorded.Date);
    
            var results = matches.Where(price => price.DateRecorded.Date == latestDate)
                .GroupBy(price => price.SellerId)
                .Select(group => group.First())
                .Select(price => new
                {
                    ComicName = price.Comic?.Name,
                    ComicId = price.Comic?.Id,
                    SellerName = price.Seller?.Name,
                    SellerId = price.Seller?.Id,
                    Price = price.Amount,
                    price.DateRecorded,
                    IsFuzzySearchUsed = isFuzzySearchUsed,
                    FuzzyMatchScore = isFuzzySearchUsed ? fuzzyMatchScore : 0
                })
                .OrderBy(result => result.Price)
                .ToList();

            return Ok(results);
        }
        [HttpGet("history/comics/{comicId}/sellers/{sellerId}")]
        public async Task<IActionResult> GetPriceHistory(int comicId, int sellerId)
        {
            var prices = await context.Prices
                .Where(price => price.Seller != null && price.Comic != null && price.Comic.Id == comicId && price.Seller.Id == sellerId)
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
                prices.First().Comic,
                prices.First().Seller,
                PriceDifference = prices.Last().Amount - prices.First().Amount,
                Data = prices.Select(price => new 
                {
                    Price = price.Amount, price.DateRecorded
                }).ToList()
            };
        
            return Ok(priceHistory);
        }
    }
}