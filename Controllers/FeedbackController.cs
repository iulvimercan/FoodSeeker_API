using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/feedback")]
[ApiController]
public class FeedbackController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    [HttpGet("by-donator/{donatorId}")]
    public async Task<ActionResult<List<FeedbackDto>>> GetFeedbacksByDonator([FromRoute] long donatorId)
    {
        try
        {
            var feedbacks = await _db.Feedbacks
                .Where(f => f.DonatorId == donatorId)
                .Include(f => f.FromUser)
                .Include(f => f.DonatorProfile)
                .Include(f => f.DonatorProfile!.User)
                .Select(f => new FeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    FromUserId = f.FromUserId,
                    UserFullName = f.FromUser != null ? f.FromUser.FullName : "Unknown",
                    UserProfilePhotoUrl = f.FromUser!.ProfilePhotoUrl,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    DonatorId = f.DonatorId,
                    RestaurantName = f.DonatorProfile != null ? f.DonatorProfile.RestaurantName : "Unknown",
                    DonatorProfilePhotoUrl = f.DonatorProfile != null ? f.DonatorProfile.User!.ProfilePhotoUrl : null,
                    CreatedAt = f.CreatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching feedbacks: " + ex.Message);
            return StatusCode(500, $"An error occurred while fetching feedbacks: {ex.Message}");
        }
    }
}