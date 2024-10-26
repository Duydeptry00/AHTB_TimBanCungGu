using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesApiController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public MoviesApiController(DBAHTBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            var movies = await _context.Phim.Include(p => p.TheLoai)
                .Select(p => new
                {
                    p.IDPhim,
                    p.TenPhim,
                    p.HinhAnh,
                    TheLoai = p.TheLoai.TenTheLoai
                })
                .ToListAsync();

            return Ok(movies);
        }

        [HttpPost] 
        public async Task<IActionResult> AddPhim(Phim model)
        {
            if (ModelState.IsValid)
            {
                _context.Phim.Add(model);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMovies), new { id = model.IDPhim }, model); 
            }

            return BadRequest(ModelState); 
        }
    }
}
