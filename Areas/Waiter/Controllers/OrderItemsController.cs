using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ByteBite.Data;
using ByteBite.Models;

namespace ByteBite.Areas.Waiter.Controllers
{
    [Area("Waiter")]
    [Authorize(Roles = "Admin,Waiter")]
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderItems.Include(o => o.Dish).Include(o => o.Order);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Dish)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Title");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,DishId,Quantity")] OrderItem orderItem)
        {
            // We do not bind 'Price' from the form to prevent malicious tampering.
            // Instead, we fetch it securely from the database.
            if (ModelState.IsValid)
            {
                // 1. Fetch the dish from the database to get its current price
                var dish = await _context.Dishes.FindAsync(orderItem.DishId);

                if (dish != null)
                {
                    // 2. Snapshot the price at the time of the order
                    orderItem.Price = dish.Price;
                }

                // 3. Save item to database
                _context.Add(orderItem);
                await _context.SaveChangesAsync();

                // 4. Automatically recalculate the total of the associated order
                await UpdateOrderTotal(orderItem.OrderId);

                return RedirectToAction(nameof(Index));
            }

            // Repopulate ViewDatas if ModelState is invalid
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Title", orderItem.DishId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Title", orderItem.DishId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,DishId,Quantity")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the snapshot price gets updated in case they edited the Dish type
                    var dish = await _context.Dishes.FindAsync(orderItem.DishId);
                    if (dish != null)
                    {
                        orderItem.Price = dish.Price;
                    }

                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();

                    // Automatically recalculate total after editing
                    await UpdateOrderTotal(orderItem.OrderId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DishId"] = new SelectList(_context.Dishes, "Id", "Title", orderItem.DishId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Dish)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                // CRUCIAL: Store OrderId before we delete the item
                int associatedOrderId = orderItem.OrderId;

                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();

                // Automatically recalculate total after deleting an item
                await UpdateOrderTotal(associatedOrderId);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }

        // Helper Method for automatic sum calculation
        private async Task UpdateOrderTotal(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Dish) // Includes Dish to access its Price
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                // Automatically compute sum: (Quantity * Dish.Price) for all items
                order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Dish.Price);

                _context.Update(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}