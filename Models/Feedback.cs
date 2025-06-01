using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FeedbackId { get; set; }

        [Required, ForeignKey("FromUser")]
        public long FromUserId { get; set; }

        [Required, ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? FromUser { get; set; }
        public DonatorProfile? DonatorProfile { get; set; }
    }
}