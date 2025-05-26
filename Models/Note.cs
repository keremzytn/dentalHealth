using System;
using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class Note
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
        
        [Required]
        public string? Content { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
} 