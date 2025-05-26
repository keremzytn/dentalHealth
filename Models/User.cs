using System;
using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public DateTime BirthDate { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
        
        // Navigation properties
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
} 