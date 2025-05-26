using System;
using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class GoalViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hedef tarihi zorunludur")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }
    }
} 