using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.DTOs.Share;
using CalendarApp.API.Models.Entities;
using CalendarApp.API.Models.Messages;
using CalendarApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CalendarApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShareController : ControllerBase
{
    private readonly ICalendarShareRepository _shareRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ShareController> _logger;

    public ShareController(
        ICalendarShareRepository shareRepository,
        IUserRepository userRepository,
        IMessagePublisher messagePublisher,
        ILogger<ShareController> logger)
    {
        _shareRepository = shareRepository;
        _userRepository = userRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet("my-shares")]
    public async Task<ActionResult<IEnumerable<CalendarShareDto>>> GetMyShares()
    {
        try
        {
            var userId = GetUserId();
            var shares = await _shareRepository.GetSharesByOwnerIdAsync(userId);

            var shareDtos = shares.Select(s => new CalendarShareDto
            {
                CalendarShareId = s.CalendarShareId,
                OwnerId = s.OwnerId,
                OwnerUsername = s.Owner?.Username ?? "",
                OwnerEmail = s.Owner?.Email ?? "",
                SpectatorEmail = s.SpectatorEmail,
                SpectatorUserId = s.SpectatorUserId,
                CreatedAt = s.CreatedAt,
                IsAccepted = s.IsAccepted
            });

            return Ok(shareDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user's shares");
            return StatusCode(500, "An error occurred while retrieving shares");
        }
    }

    [HttpGet("spectating")]
    public async Task<ActionResult<IEnumerable<SharedCalendarDto>>> GetSpectatingCalendars()
    {
        try
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var shares = await _shareRepository.GetSharesBySpectatorIdAsync(userId);

            var calendarDtos = shares.Select(s => new SharedCalendarDto
            {
                OwnerId = s.OwnerId,
                OwnerUsername = s.Owner?.Username ?? "",
                OwnerEmail = s.Owner?.Email ?? "",
                SharedAt = s.CreatedAt
            });

            return Ok(calendarDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spectating calendars");
            return StatusCode(500, "An error occurred while retrieving spectating calendars");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CalendarShareDto>> CreateShare([FromBody] CreateCalendarShareDto shareDto)
    {
        try
        {
            var userId = GetUserId();

            // Validate email
            if (string.IsNullOrWhiteSpace(shareDto.SpectatorEmail))
                return BadRequest("Spectator email is required");

            // Check if share already exists
            var exists = await _shareRepository.ShareExistsAsync(userId, shareDto.SpectatorEmail);
            if (exists)
                return BadRequest("This calendar is already shared with this email");

            // Check if spectator user exists
            var spectatorUser = await _userRepository.GetByEmailAsync(shareDto.SpectatorEmail);

            var share = new CalendarShare
            {
                OwnerId = userId,
                SpectatorEmail = shareDto.SpectatorEmail.ToLower(),
                SpectatorUserId = spectatorUser?.UserId,
                CreatedAt = DateTime.UtcNow,
                IsAccepted = spectatorUser != null
            };

            var createdShare = await _shareRepository.CreateAsync(share);

            // Get owner details for the message
            var owner = await _userRepository.GetByIdAsync(userId);

            // Publish calendar shared message to RabbitMQ
            try
            {
                var message = new CalendarSharedMessage
                {
                    ShareId = createdShare.CalendarShareId,
                    OwnerId = userId,
                    OwnerUsername = owner?.Username ?? "Unknown",
                    OwnerEmail = owner?.Email ?? "",
                    SpectatorEmail = shareDto.SpectatorEmail,
                    SharedAt = DateTime.UtcNow
                };

                _messagePublisher.PublishMessage("calendar-shared", message);
                _logger.LogInformation("Calendar shared message published for share {ShareId}", createdShare.CalendarShareId);
            }
            catch (Exception mqEx)
            {
                _logger.LogError(mqEx, "Failed to publish calendar shared message for share {ShareId}", createdShare.CalendarShareId);
                // Don't fail the share creation if message publishing fails
            }

            var responseDto = new CalendarShareDto
            {
                CalendarShareId = createdShare.CalendarShareId,
                OwnerId = userId,
                SpectatorEmail = shareDto.SpectatorEmail,
                SpectatorUserId = spectatorUser?.UserId,
                CreatedAt = createdShare.CreatedAt,
                IsAccepted = createdShare.IsAccepted
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating calendar share");
            return StatusCode(500, "An error occurred while creating the share");
        }
    }

    [HttpDelete("{shareId}")]
    public async Task<ActionResult> DeleteShare(int shareId)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _shareRepository.DeleteAsync(shareId, userId);

            if (!deleted)
                return NotFound("Share not found or you don't have permission to delete it");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting share {ShareId}", shareId);
            return StatusCode(500, "An error occurred while deleting the share");
        }
    }
}
