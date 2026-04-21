using ByteBite.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByteBite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading roles: {ex.Message}";
                return View(new List<IdentityRole>());
            }
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Get users in this role
                var usersInRole = await _context.UserRoles
                    .Where(ur => ur.RoleId == role.Id)
                    .Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => new { u.Id, u.Email, u.UserName })
                    .ToListAsync();

                ViewBag.UsersInRole = usersInRole;
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading role: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] IdentityRole role)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrWhiteSpace(role.Name))
                    {
                        ModelState.AddModelError("Name", "Role name is required.");
                        return View(role);
                    }

                    // Check if role already exists
                    var existingRole = await _roleManager.FindByNameAsync(role.Name);
                    if (existingRole != null)
                    {
                        ModelState.AddModelError("Name", "This role already exists.");
                        return View(role);
                    }

                    var result = await _roleManager.CreateAsync(new IdentityRole(role.Name));
                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Role '{role.Name}' created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating role: {ex.Message}";
                return View(role);
            }
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading role: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name")] IdentityRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrWhiteSpace(role.Name))
                    {
                        ModelState.AddModelError("Name", "Role name is required.");
                        return View(role);
                    }

                    var existingRole = await _roleManager.FindByIdAsync(id);
                    if (existingRole == null)
                    {
                        return NotFound();
                    }

                    // Check if another role with this name exists
                    var duplicateRole = await _roleManager.FindByNameAsync(role.Name);
                    if (duplicateRole != null && duplicateRole.Id != role.Id)
                    {
                        ModelState.AddModelError("Name", "A role with this name already exists.");
                        return View(role);
                    }

                    existingRole.Name = role.Name;
                    var result = await _roleManager.UpdateAsync(existingRole);

                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Role updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating role: {ex.Message}";
                return View(role);
            }
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Count users in this role
                var usersInRole = await _context.UserRoles
                    .CountAsync(ur => ur.RoleId == role.Id);

                ViewBag.UsersInRole = usersInRole;
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading role: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Check if role has users
                var usersInRole = await _context.UserRoles
                    .AnyAsync(ur => ur.RoleId == role.Id);

                if (usersInRole)
                {
                    TempData["Error"] = "Cannot delete a role that has users assigned to it.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role deleted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting role: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
