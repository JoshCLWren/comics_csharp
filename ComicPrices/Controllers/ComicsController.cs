using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ComicPrices.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class ComicsController : ControllerBase
    {
        // DbContext that provides us access to database.
        private readonly Data.ComicsDbContext _context;
        
        // Constructor receives DbContext instance through dependency injection
        public ComicsController(Data.ComicsDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Comic>> GetById(int id)
        {
            var comic = await _context.Comics.FindAsync(id);

            if (comic == null)
            {
                return NotFound();
            }

            return Ok(comic);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string name = "")
        {
            IQueryable<Models.Comic> query = _context.Comics;

            // apply the name filtering
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(comic => comic.Name != null && comic.Name.Contains(name));
            }

            int totalItems = await query.CountAsync();

            // apply the pagination
            var comics = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // always a good practice to return the total count and the page size for clients to handle on their end.
            return Ok(new { TotalItems = totalItems, PageSize = pageSize, Items = comics });
        }
    }
}