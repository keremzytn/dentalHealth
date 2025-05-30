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
using System.Text.Json;
using Serilog;

namespace DentalHealthTracker.Controllers
{
    [Authorize]
    public class DentalHealthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DentalHealthController> _logger;

        public DentalHealthController(ApplicationDbContext context, ILogger<DentalHealthController> logger)
        {
            _context = context;
            _logger = logger;
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
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Model hatası: {errors}";
                    return RedirectToAction(nameof(Index));
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                goal.UserId = userId;
                goal.CreatedDate = DateTime.UtcNow;
                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Hedef başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hedef eklenirken bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" İç hata: {ex.InnerException.Message}";
                }
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
        public async Task<IActionResult> AddStatus([FromForm] GoalStatus status, IFormFile image)
        {
            try
            {
                _logger.LogInformation("AddStatus metodu başlatıldı. Form verileri: {@FormData}",
                    Request.Form.ToDictionary(f => f.Key, f => f.Value.ToString()));

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning("Model hatası: {Errors}", errors);
                    TempData["Error"] = $"Durum kaydedilirken bir hata oluştu: {errors}";
                    return RedirectToAction(nameof(Index));
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return RedirectToAction("Login", "Account");

                status.UserId = userId;
                status.Date = DateTime.UtcNow;
                status.IsCompleted = Request.Form["IsCompleted"].ToString().ToLower() == "true";
                _logger.LogDebug("Status oluşturuldu: {@Status}", status);

                if (image != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine("wwwroot", "uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    status.ImagePath = $"/uploads/{fileName}";
                    _logger.LogInformation("Resim yüklendi: {ImagePath}", status.ImagePath);
                }

                _context.GoalStatuses.Add(status);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Status başarıyla kaydedildi. ID: {StatusId}", status.Id);
                TempData["Success"] = "Durum başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Durum kaydedilirken hata oluştu");
                TempData["Error"] = $"Durum kaydedilirken bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" İç hata: {ex.InnerException.Message}";
                }
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