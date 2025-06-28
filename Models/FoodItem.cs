using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class FoodItem
    {
        // Primary key with auto-generated identity value.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FoodId { get; set; }

        // Foreign key to the DonatorProfile who owns this food item.
        [ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        // Name of the food item, required, max length 100 characters.
        [Required, MaxLength(100)]
        public required string FoodName { get; set; }

        // Optional description of the food item, max length 500 characters.
        [MaxLength(500)]
        public string? Description { get; set; }

        // Indicates if the food can be eaten on-site.
        [Column("IsEatIn")]
        public bool IsEatIn { get; set; } = false;
        
        // Indicates if the food is available for takeaway.
        [Column("IsTakeAway")]
        public bool IsTakeAway { get; set; } = false;
        
        // Indicates if the food requires the seeker to bring a container or pack.
        [Column("IsBringPack")]
        public bool IsBringPack { get; set; } = false;

        // Optional URL for a photo of the food item, max length 256 characters.
        [MaxLength(256)]
        public string? PhotoUrl { get; set; }

        // Timestamp of when the food item was created, default is UTC now.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Whether the food item is currently active/available.
        public bool IsActive { get; set; } = true;

        // Navigation property to the DonatorProfile owner.
        public DonatorProfile? DonatorProfile { get; set; }

        // Collection of pickup intents indicating users interested in picking up this food item.
        public ICollection<PickupIntent> PickupIntents { get; set; } = new List<PickupIntent>();
    }
}
