using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class Feedback
    {
        // Primary key with auto-generated identity value
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FeedbackId { get; set; }

        // Foreign key to the User who gave the feedback
        [Required, ForeignKey("FromUser")]
        public long FromUserId { get; set; }

        // Foreign key to the DonatorProfile receiving the feedback
        [Required, ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        // Rating given by the user, must be between 1 and 5
        [Required, Range(1, 5)]
        public int Rating { get; set; }

        // Optional textual comment accompanying the rating
        // Maximum length 1000 characters
        [MaxLength(1000)]
        public string? Comment { get; set; }

        // Timestamp when the feedback was created
        // Automatically set to UTC now when inserted
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the User who gave the feedback
        public User? FromUser { get; set; }

        // Navigation property to the DonatorProfile being reviewed
        public DonatorProfile? DonatorProfile { get; set; }
    }
}
