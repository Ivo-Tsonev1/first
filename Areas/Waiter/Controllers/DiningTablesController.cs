using ByteBite.Data;
using ByteBite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByteBite.Areas.Waiter.Controllers
{
    [Area("Waiter")]
    [Authorize(Roles = "Admin,Waiter")]
    public class DiningTablesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DiningTablesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: DiningTables
        public async Task<IActionResult> Index()
        {
            try
            {
                var tables = await _context.DiningTables.ToListAsync();
                return View(tables);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(new List<DiningTable>());
            }
        }

        // GET: DiningTables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var diningTable = await _context.DiningTables.FirstOrDefaultAsync(m => m.Id == id);
                if (diningTable == null)
                {
                    return NotFound();
                }

                return View(diningTable);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: DiningTables/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DiningTables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TableNumber,Capacity,IsOccupied")] DiningTable diningTable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(diningTable);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Table created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Error: {ex.Message}";
                }
            }
            return View(diningTable);
        }

        // GET: DiningTables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var diningTable = await _context.DiningTables.FindAsync(id);
                if (diningTable == null)
                {
                    return NotFound();
                }
                return View(diningTable);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DiningTables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TableNumber,Capacity,IsOccupied")] DiningTable diningTable)
        {
            if (id != diningTable.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(diningTable);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Table updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiningTableExists(diningTable.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(diningTable);
        }

        // GET: DiningTables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var diningTable = await _context.DiningTables.FirstOrDefaultAsync(m => m.Id == id);
                if (diningTable == null)
                {
                    return NotFound();
                }

                return View(diningTable);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DiningTables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var diningTable = await _context.DiningTables.FindAsync(id);
                if (diningTable != null)
                {
                    // Delete associated orders first
                    var orders = await _context.Orders.Where(o => o.DiningTableId == id).ToListAsync();
                    foreach (var order in orders)
                    {
                        var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
                        _context.OrderItems.RemoveRange(orderItems);
                        _context.Orders.Remove(order);
                    }

                    _context.DiningTables.Remove(diningTable);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Table deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool DiningTableExists(int id)
        {
            return _context.DiningTables.Any(e => e.Id == id);
        }

        // ===================== API ENDPOINTS =====================

        [HttpGet]
        public async Task<IActionResult> GetOrders(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                    .FirstOrDefaultAsync(o => o.DiningTableId == id && !o.IsPaid);

                var items = new List<object>();
                decimal total = 0;

                if (order != null)
                {
                    foreach (var item in order.Items)
                    {
                        items.Add(new
                        {
                            itemId = item.Id,
                            dishName = item.Dish.Title,
                            quantity = item.Quantity,
                            price = item.Dish.Price,
                            subtotal = item.Dish.Price * item.Quantity
                        });
                        total += item.Dish.Price * item.Quantity;
                    }
                }

                return Json(new { items, total, success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, items = new List<object>(), total = 0, success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
        {
            try
            {
                if (request == null || request.TableId <= 0 || request.DishId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request parameters" });
                }

                var table = await _context.DiningTables.FindAsync(request.TableId);
                if (table == null)
                {
                    return NotFound(new { success = false, message = "Table not found" });
                }

                var dish = await _context.Dishes.FindAsync(request.DishId);
                if (dish == null)
                {
                    return NotFound(new { success = false, message = "Dish not found" });
                }

                var user = await _userManager.GetUserAsync(User);

                // Get or create order
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.DiningTableId == request.TableId && !o.IsPaid);

                if (order == null)
                {
                    order = new Order
                    {
                        DiningTableId = request.TableId,
                        OrderTime = DateTime.Now,
                        WaiterId = user?.Id,
                        IsPaid = false,
                        TotalPrice = 0
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Mark table as occupied
                    table.IsOccupied = true;
                    _context.Update(table);
                    await _context.SaveChangesAsync();
                }

                // Check if item already exists
                var existingItem = order.Items.FirstOrDefault(i => i.DishId == request.DishId);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                    _context.Update(existingItem);
                }
                else
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        DishId = request.DishId,
                        Quantity = request.Quantity
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // Update order total
                await _context.SaveChangesAsync();
                order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                order.TotalPrice = order.Items.Sum(i => i.Quantity * i.Dish.Price);
                _context.Update(order);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Item added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveItemRequest request)
        {
            try
            {
                if (request == null || request.ItemId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request" });
                }

                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .FirstOrDefaultAsync(oi => oi.Id == request.ItemId);

                if (orderItem == null)
                {
                    return NotFound(new { success = false, message = "Item not found" });
                }

                var order = orderItem.Order;
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();

                var remainingItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == order.Id)
                    .ToListAsync();

                if (remainingItems.Count == 0)
                {
                    _context.Orders.Remove(order);

                    if (order.DiningTableId > 0)
                    {
                        var table = await _context.DiningTables.FindAsync(order.DiningTableId);
                        if (table != null)
                        {
                            table.IsOccupied = false;
                            _context.Update(table);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    order = await _context.Orders
                        .Include(o => o.Items)
                        .ThenInclude(i => i.Dish)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);

                    order.TotalPrice = order.Items.Sum(i => i.Quantity * i.Dish.Price);
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Item removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBill(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Dish)
                    .FirstOrDefaultAsync(o => o.DiningTableId == id && !o.IsPaid);

                if (order == null)
                {
                    return NotFound(new { error = "No active order found" });
                }

                var items = order.Items.Select(i => new
                {
                    dishName = i.Dish.Title,
                    quantity = i.Quantity,
                    price = i.Dish.Price,
                    subtotal = i.Dish.Price * i.Quantity
                }).ToList();

                return Json(new
                {
                    orderId = order.Id,
                    tableId = order.DiningTableId,
                    items = items,
                    total = order.TotalPrice,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (request == null || request.OrderId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request" });
                }

                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                order.IsPaid = true;
                _context.Update(order);

                var table = await _context.DiningTables.FindAsync(request.TableId);
                if (table != null)
                {
                    table.IsOccupied = false;
                    _context.Update(table);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReleaseTable([FromBody] ReleaseTableRequest request)
        {
            try
            {
                if (request == null || request.TableId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid request" });
                }

                var table = await _context.DiningTables.FindAsync(request.TableId);
                if (table == null)
                {
                    return NotFound(new { success = false, message = "Table not found" });
                }

                table.IsOccupied = false;
                _context.Update(table);

                var unpaidOrders = await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.DiningTableId == request.TableId && !o.IsPaid)
                    .ToListAsync();

                foreach (var order in unpaidOrders)
                {
                    foreach (var item in order.Items)
                    {
                        _context.OrderItems.Remove(item);
                    }
                    _context.Orders.Remove(order);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Table released successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDishes()
        {
            try
            {
                var dishes = await _context.Dishes
                    .Select(d => new
                    {
                        id = d.Id,
                        title = d.Title,
                        description = d.Description,
                        price = d.Price,
                        imagePath = d.ImagePath
                    })
                    .ToListAsync();

                return Json(dishes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request Classes
    public class AddItemRequest
    {
        public int TableId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class RemoveItemRequest
    {
        public int ItemId { get; set; }
        public int TableId { get; set; }
    }

    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
    }

    public class ReleaseTableRequest
    {
        public int TableId { get; set; }
    }
}
