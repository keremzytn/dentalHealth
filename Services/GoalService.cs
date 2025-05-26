using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DentalHealthTracker.Data;
using DentalHealthTracker.Models;

namespace DentalHealthTracker.Services
{
    public class GoalService : IGoalService
    {
        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> CreateGoalAsync(GoalViewModel model)
        {
            var goal = new Goal
            {
                UserId = model.UserId,
                Title = model.Title,
                Description = model.Description,
                CreatedDate = DateTime.SpecifyKind(model.TargetDate, DateTimeKind.Utc),
                Status = GoalStatusType.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Goals.AddAsync(goal);
            await _context.SaveChangesAsync();

            return ServiceResult.Success();
        }
    }
} 