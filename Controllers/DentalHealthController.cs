using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DentalHealthTracker.Models;
using DentalHealthTracker.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DentalHealthTracker.Controllers
{
    [Authorize]
    public class DentalHealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DentalHealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return RedirectToAction("Login", "Account");
            
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var viewModel = new DentalHealthViewModel
            {
                Goals = await _context.Goals
                    .Where(g => g.UserId == userId)
                    .ToListAsync(),
                RecentStatuses = await _context.GoalStatuses
                    .Include(gs => gs.Goal)
                    .Where(gs => gs.UserId == userId && gs.Date >= sevenDaysAgo)
                    .OrderByDescending(gs => gs.Date)
                    .ToListAsync(),
                CurrentRecommendation = await GetRandomRecommendation()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddGoal(Goal goal)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return RedirectToAction("Login", "Account");
                
                goal.UserId = userId;
                goal.CreatedDate = DateTime.UtcNow;
                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return RedirectToAction("Login", "Account");

            var goal = await _context.Goals.FindAsync(id);
            if (goal != null && goal.UserId == userId)
            {
                var hasStatus = await _context.GoalStatuses.AnyAsync(gs => gs.GoalId == id);
                if (hasStatus)
                {
                    TempData["Error"] = "Bu hedefin durum kayıtları olduğu için silinemez.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddStatus(GoalStatus status, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return RedirectToAction("Login", "Account");
                
                status.UserId = userId;
                status.Date = DateTime.UtcNow;

                if (image != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine("wwwroot", "uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    status.ImagePath = $"/uploads/{fileName}";
                }

                _context.GoalStatuses.Add(status);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetRandomRecommendation()
        {
            var recommendations = await _context.Recommendations
                .Where(r => r.IsActive)
                .ToListAsync();
            
            if (!recommendations.Any())
                return "Henüz öneri bulunmamaktadır.";

            var random = new Random();
            var index = random.Next(0, recommendations.Count);
            return recommendations[index].Content;
        }
    }
} 