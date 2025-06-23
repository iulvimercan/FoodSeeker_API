using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/feedback")]
[ApiController]
public class FeedbackController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFeedbacks()
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();
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
                    RestaurantName = f.DonatorProfile != null ? f.DonatorProfile.RestaurantName : "Unknown",
                    DonatorProfilePhotoUrl = f.DonatorProfile != null ? f.DonatorProfile.User!.ProfilePhotoUrl : null,
                    CreatedAt = f.CreatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(new { feedbacks });
        }catch (Exception ex)
        {
            Console.WriteLine("Error fetching feedbacks: " + ex.Message);
            return StatusCode(500, $"An error occurred while fetching feedbacks: {ex.Message}");
        }
    }

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

    [HttpPost]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> CreateFeedback([FromBody] FeedbackRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Validate the rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            // Check if the donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == dto.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Create new feedback entry
            var feedback = new Feedback
            {
                FromUserId = foodSeekerId,
                DonatorId = dto.DonatorId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            // Add the feedback to the database
            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            // Calculate the new average rating for the donator
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

    [HttpPatch("{feedbackId:long}")]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> UpdateFeedback(long feedbackId, [FromBody] FeedbackRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Check if the donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == dto.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Find the feedback to update
            var feedback = await _db.Feedbacks
                .Include(f => f.FromUser)
                .SingleOrDefaultAsync(f => f.FeedbackId == feedbackId && f.FromUserId == foodSeekerId);
            if (feedback == null)
                return NotFound("Feedback not found or you do not have permission to update it");

            // Validate the rating
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            // Update feedback properties
            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment;
            feedback.CreatedAt = DateTime.UtcNow;

            // Save changes to the database
            await _db.SaveChangesAsync();

            // Calculate the new average rating for the donator
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


    [HttpDelete("{feedbackId:long}")]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> DeleteFeedback(long feedbackId)
    {
        try
        {
            // Get user ID from JWT claims
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Find the feedback to delete
            var feedback = await _db.Feedbacks
                .Include(f => f.FromUser)
                .SingleOrDefaultAsync(f => f.FeedbackId == feedbackId && f.FromUserId == foodSeekerId);
            if (feedback == null)
                return NotFound("Feedback not found or you do not have permission to delete it");

            // Remove the feedback from the database
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();

            // Check if the donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.DonatorId == feedback.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Calculate the new average rating for the donator
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