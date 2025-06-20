using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class FoodItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FoodId { get; set; }

        [ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        [Required, MaxLength(100)]
        public required string FoodName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("IsEatIn")]
        public bool IsEatIn { get; set; } = false;
        
        [Column("IsTakeAway")]
        public bool IsTakeAway { get; set; } = false;
        
        [Column("IsBringPack")]
        public bool IsBringPack { get; set; } = false;

        [MaxLength(256)]
        public string? PhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property
        public DonatorProfile? DonatorProfile { get; set; }
        public ICollection<PickupIntent> PickupIntents { get; set; } = new List<PickupIntent>();
    }
}