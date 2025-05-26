using System;
using System.ComponentModel.DataAnnotations;

namespace DentalHealthTracker.Models
{
    public class DentalHealthRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public string? Notes { get; set; }
        
        public bool BrushedTeeth { get; set; }
        
        public bool Flossed { get; set; }
        
        public bool UsedMouthwash { get; set; }
        
        public int BrushingDuration { get; set; }
    }
} 