using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

/// <summary>
/// Controller for managing feedbacks related to donators.
/// Handles create, read, update, and delete operations.
/// </summary>
[Route("api/feedback")]
[ApiController]
public class FeedbackController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    /// <summary>
    /// Retrieves feedbacks given or received by the current user.
    /// Only accessible to authenticated users.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFeedbacks()
    {
        try
        {
            // Extract user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Fetch all feedbacks where the user is either sender or target donator
            var feedbacks = await _db.Feedbacks
                .Where(f => f.FromUserId == userId || f.DonatorId == userId)
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
                    RestaurantName = f.DonatorProfile?.RestaurantName ?? "Unknown",
                    DonatorProfilePhotoUrl = f.DonatorProfile?.User?.ProfilePhotoUrl,
                    CreatedAt = f.CreatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(new { feedbacks });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching feedbacks: " + ex.Message);
            return StatusCode(500, $"An error occurred while fetching feedbacks: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all feedbacks given to a specific donator along with average score.
    /// </summary>
    [HttpGet("by-donator/{donatorId}")]
    public async Task<ActionResult<List<FeedbackDto>>> GetFeedbacksByDonator([FromRoute] long donatorId)
    {
        try
        {
            // Fetch feedbacks for given donator
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
                    RestaurantName = f.DonatorProfile?.RestaurantName ?? "Unknown",
                    DonatorProfilePhotoUrl = f.DonatorProfile?.User?.ProfilePhotoUrl,
                    CreatedAt = f.CreatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // Calculate average rating
            var averageScore = await _db.Feedbacks
                .Where(f => f.DonatorId == donatorId)
                .AverageAsync(f => f.Rating);

            return Ok(new { feedbacks, averageScore });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching feedbacks: " + ex.Message);
            return StatusCode(500, $"An error occurred while fetching feedbacks: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new feedback entry by a food seeker for a donator.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> CreateFeedback([FromBody] FeedbackRequestDto dto)
    {
        try
        {
            // Extract user ID
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            // Ensure donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == dto.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Add new feedback
            var feedback = new Feedback
            {
                FromUserId = foodSeekerId,
                DonatorId = dto.DonatorId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            // Recalculate donator average score
            var averageScore = await _db.Feedbacks
                .Where(f => f.DonatorId == dto.DonatorId)
                .AverageAsync(f => f.Rating);
            donator.AverageScore = (float?)averageScore ?? 0f;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Your comment has been created successfully.",
                donatorAverageScore = donator.AverageScore,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating feedback: " + ex.Message);
            return StatusCode(500, $"An error occurred while creating feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing feedback by its owner.
    /// </summary>
    [HttpPatch("{feedbackId:long}")]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> UpdateFeedback(long feedbackId, [FromBody] FeedbackRequestDto dto)
    {
        try
        {
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Validate donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == dto.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Fetch feedback and validate ownership
            var feedback = await _db.Feedbacks
                .Include(f => f.FromUser)
                .SingleOrDefaultAsync(f => f.FeedbackId == feedbackId && f.FromUserId == foodSeekerId);
            if (feedback == null)
                return NotFound("Feedback not found or you do not have permission to update it");

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            // Update fields
            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment;
            feedback.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Recalculate average rating
            var averageScore = await _db.Feedbacks
                .Where(f => f.DonatorId == dto.DonatorId)
                .AverageAsync(f => f.Rating);
            donator.AverageScore = (float?)averageScore ?? 0f;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Your comment has been updated successfully.",
                donatorAverageScore = donator.AverageScore,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating feedback: " + ex.Message);
            return StatusCode(500, $"An error occurred while updating feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes an existing feedback by its owner.
    /// </summary>
    [HttpDelete("{feedbackId:long}")]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> DeleteFeedback(long feedbackId)
    {
        try
        {
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Locate feedback to delete
            var feedback = await _db.Feedbacks
                .Include(f => f.FromUser)
                .SingleOrDefaultAsync(f => f.FeedbackId == feedbackId && f.FromUserId == foodSeekerId);
            if (feedback == null)
                return NotFound("Feedback not found or you do not have permission to delete it");

            // Delete feedback
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();

            // Recalculate donator average score
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == feedback.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            var averageScore = await _db.Feedbacks
                .Where(f => f.DonatorId == feedback.DonatorId)
                .AverageAsync(f => f.Rating);
            donator.AverageScore = (float?)averageScore ?? 0f;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Your comment has been deleted successfully.",
                donatorAverageScore = donator.AverageScore,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting feedback: " + ex.Message);
            return StatusCode(500, $"An error occurred while deleting feedback: {ex.Message}");
        }
    }
}
