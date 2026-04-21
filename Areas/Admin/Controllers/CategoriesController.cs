using ByteBite.Data;
using ByteBite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByteBite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                var applicationDbContext = _context.Categories.Include(c => c.Menu);
                return View(await applicationDbContext.ToListAsync());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading categories: {ex.Message}";
                return View(new List<Category>());
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Menu)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            try
            {
                // Load all menus from database
                var menus = _context.Menus.ToList();

                ViewData["MenuId"] = new SelectList(menus, "Id", "Name");

                if (menus.Count == 0)
                {
                    ViewBag.WarningMessage = "No menus available. Please create a menu first.";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading form: {ex.Message}";
                ViewData["MenuId"] = new SelectList(new List<Menu>(), "Id", "Name");
                return View();
            }
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    ModelState.AddModelError("Name", "Category name is required.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name");
                    return View(category);
                }

                if (category.MenuId <= 0)
                {
                    ModelState.AddModelError("MenuId", "Please select a valid menu.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name");
                    return View(category);
                }

                // Check if menu exists
                var menuExists = await _context.Menus.AnyAsync(m => m.Id == category.MenuId);
                if (!menuExists)
                {
                    ModelState.AddModelError("MenuId", "Selected menu does not exist.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name");
                    return View(category);
                }

                // Check for duplicate
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.MenuId == category.MenuId);

                if (categoryExists)
                {
                    ModelState.AddModelError("Name", "This category already exists in the selected menu.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name");
                    return View(category);
                }

                // Save to database
                _context.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Database error: {innerException}");
                ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", category.MenuId);
                return View(category);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", category.MenuId);
                return View(category);
            }
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                ViewData["MenuId"] = new SelectList(_context.Menus, "Id", "Name", category.MenuId);
                return View(category);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading edit form: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    ModelState.AddModelError("Name", "Category name is required.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", category.MenuId);
                    return View(category);
                }

                if (category.MenuId <= 0)
                {
                    ModelState.AddModelError("MenuId", "Please select a valid menu.");
                    ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", category.MenuId);
                    return View(category);
                }

                _context.Update(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewData["MenuId"] = new SelectList(_context.Menus.ToList(), "Id", "Name", category.MenuId);
                return View(category);
            }
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Menu)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
