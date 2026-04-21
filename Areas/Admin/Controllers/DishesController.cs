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
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DishesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            try
            {
                var applicationDbContext = _context.Dishes.Include(d => d.Category);
                return View(await applicationDbContext.ToListAsync());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading dishes: {ex.Message}";
                return View(new List<Dish>());
            }
        }

        // GET: Dishes/Details/5
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
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (dish == null)
                {
                    return NotFound();
                }

                return View(dish);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading dish: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Dishes/Create
        public IActionResult Create()
        {
            try
            {
                var categories = _context.Categories.ToList();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");

                if (categories.Count == 0)
                {
                    ViewBag.WarningMessage = "No categories available. Please create a category first.";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading form: {ex.Message}";
                ViewData["CategoryId"] = new SelectList(new List<Category>(), "Id", "Name");
                return View();
            }
        }

        // POST: Dishes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dish dish, IFormFile imageFile)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dish.Title))
                {
                    ModelState.AddModelError("Title", "Dish title is required.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                if (dish.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than 0.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                if (dish.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Please select a category.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                // Check if category exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dish.CategoryId);
                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Selected category does not exist.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Validate file size (5MB max)
                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("imageFile", "File size must be less than 5MB");
                            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                            return View(dish);
                        }

                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("imageFile", "Only image files are allowed (jpg, jpeg, png, gif, webp)");
                            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                            return View(dish);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "dishes");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        dish.ImagePath = "/images/dishes/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("imageFile", $"Error uploading image: {ex.Message}");
                        ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                        return View(dish);
                    }
                }
                else
                {
                    dish.ImagePath = "/images/placeholder.jpg";
                }

                // Save to database
                _context.Add(dish);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Dish '{dish.Title}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Database error: {innerException}");
                ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                return View(dish);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                return View(dish);
            }
        }

        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var dish = await _context.Dishes.FindAsync(id);
                if (dish == null)
                {
                    return NotFound();
                }
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", dish.CategoryId);
                return View(dish);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading edit form: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dish dish, IFormFile imageFile)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dish.Title))
                {
                    ModelState.AddModelError("Title", "Dish title is required.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                if (dish.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than 0.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                    return View(dish);
                }

                // Handle image upload if new image provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(dish.ImagePath) && dish.ImagePath != "/images/placeholder.jpg")
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, dish.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Validate file size
                        if (imageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("imageFile", "File size must be less than 5MB");
                            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                            return View(dish);
                        }

                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("imageFile", "Only image files are allowed");
                            ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                            return View(dish);
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "dishes");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        dish.ImagePath = "/images/dishes/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("imageFile", $"Error uploading image: {ex.Message}");
                        ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                        return View(dish);
                    }
                }

                _context.Update(dish);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dish updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(dish.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewData["CategoryId"] = new SelectList(_context.Categories.ToList(), "Id", "Name", dish.CategoryId);
                return View(dish);
            }
        }

        // GET: Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var dish = await _context.Dishes
                    .Include(d => d.Category)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (dish == null)
                {
                    return NotFound();
                }

                return View(dish);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var dish = await _context.Dishes.FindAsync(id);
                if (dish != null)
                {
                    // Delete image if it exists
                    if (!string.IsNullOrEmpty(dish.ImagePath) && dish.ImagePath != "/images/placeholder.jpg")
                    {
                        string imagePath = Path.Combine(_hostEnvironment.WebRootPath, dish.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.Dishes.Remove(dish);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Dish deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting dish: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Get all dishes as JSON
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var dishes = await _context.Dishes
                    .Include(d => d.Category)
                    .ToListAsync();

                return Json(dishes.Select(d => new
                {
                    id = d.Id,
                    title = d.Title,
                    description = d.Description,
                    price = d.Price,
                    imagePath = d.ImagePath ?? "/images/placeholder.jpg",
                    category = d.Category?.Name
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}
