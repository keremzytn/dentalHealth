using System.Threading.Tasks;
using DentalHealthTracker.Models;

namespace DentalHealthTracker.Services
{
    public interface IGoalService
    {
        Task<ServiceResult> CreateGoalAsync(GoalViewModel model);
    }
} 