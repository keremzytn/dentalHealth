using System;
using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class DentalHealthRecord
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string? Notes { get; set; }

        public bool IsCompleted { get; set; }

        public string? ImagePath { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}