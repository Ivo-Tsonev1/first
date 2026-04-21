using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ByteBite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // Показва списъка с всички потребители
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Метод за задаване на роля Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Премахваме текущите роли, за да няма дублиране
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Добавяме ролята Admin
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction(nameof(Index));
        }

        // Метод за задаване на роля Waiter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignWaiter(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                await _userManager.AddToRoleAsync(user, "Waiter");
            }
            return RedirectToAction(nameof(Index));
        }

        // Метод за изтриване на потребител
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}