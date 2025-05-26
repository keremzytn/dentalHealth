using System;
using System.Collections.Generic;

namespace DentalHealthTracker.Models
{
    public class DentalHealthViewModel
    {
        public List<Goal> Goals { get; set; } = new();
        public List<GoalStatus> RecentStatuses { get; set; } = new();
        public string CurrentRecommendation { get; set; } = string.Empty;
    }
} 