using ByteBite.Data;
using ByteBite.Data;
using ByteBite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Controllers
{
    public class MenusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Menus (displays dishes)
        public async Task<IActionResult> Index()
        {
            try
            {
                var dishes = _context.Dishes.Include(d => d.Category);
                return View(await dishes.ToListAsync());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading dishes: {ex.Message}";
                return View(new List<Dish>());
            }
        }

        // GET: Menus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var dish = await _context.Dishes
                    .Include(d => d.Category)
                    .FirstOrDefaultAsync(d => d.Id == id);
                if (dish == null)
                {
                    return NotFound();
                }

                return View(dish);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading dish details: {ex.Message}";
                return NotFound();
            }
        }
    }
}
