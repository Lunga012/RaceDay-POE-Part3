using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Attributes;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    /// <summary>
    /// Manages the authenticated user's profile information.
    /// Accessible to both Organiser and Participant roles.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISessionManager _sessionManager;
        private readonly IBlobStorageService _blobStorageService;

        public ProfileController(
            RaceDayDbContext context,
            IPasswordHasher passwordHasher,
            ISessionManager sessionManager,
            IBlobStorageService? blobStorageService = null)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _sessionManager = sessionManager;
            _blobStorageService = blobStorageService ?? null!;
        }

        /// <summary>
        /// Retrieves the profile of the currently logged-in user.
        /// </summary>
        /// <returns>User profile data.</returns>
        /// <response code="200">Returns user profile.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var session = _sessionManager.GetCurrentSession(HttpContext);
            if (session == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == session.UserID);

            if (user == null)
            {
                return NotFound(new { error = "NotFound", message = "User record not found." });
            }

            var dto = new UserProfileDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName ?? "Unknown",
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(dto);
        }

        /// <summary>
        /// Updates the profile (name, email, or password) of the currently logged-in user.
        /// </summary>
        /// <param name="dto">Updated profile fields.</param>
        /// <returns>Updated profile data.</returns>
        /// <response code="200">Profile updated successfully.</response>
        /// <response code="400">Invalid profile payload.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="409">Email address is already in use by another account.</response>
        [HttpPut]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var session = _sessionManager.GetCurrentSession(HttpContext);
            if (session == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == session.UserID);

            if (user == null)
            {
                return NotFound(new { error = "NotFound", message = "User record not found." });
            }

            var newEmail = dto.Email.Trim().ToLowerInvariant();

            // Check if email changed and if already taken by another user
            if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == newEmail && u.UserID != user.UserID);

                if (emailTaken)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        error = "Conflict",
                        message = $"Email '{dto.Email}' is already in use by another user."
                    });
                }
                user.Email = newEmail;
            }

            user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = dto.ProfilePictureUrl.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (dto.NewPassword.Length < 4)
                {
                    return BadRequest(new { error = "BadRequest", message = "New password must be at least 4 characters long." });
                }
                user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            }

            await _context.SaveChangesAsync();

            // Refresh session details
            session.Name = user.Name;
            session.Email = user.Email;

            var result = new UserProfileDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName ?? "Unknown",
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(result);
        }

        /// <summary>
        /// Uploads a user profile picture to Azure Blob Storage and updates the authenticated user's profile.
        /// </summary>
        /// <param name="file">Image file (JPEG, PNG, WebP, max 5MB).</param>
        /// <returns>Updated user profile details with the new profile picture URL.</returns>
        /// <response code="200">Profile picture uploaded successfully.</response>
        /// <response code="400">Invalid file format or file missing.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User record not found.</response>
        [HttpPost("picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "BadRequest", message = "No image file was provided for upload." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { error = "BadRequest", message = "Profile picture size must not exceed 5MB." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!System.Array.Exists(allowedExtensions, ext => ext == extension))
            {
                return BadRequest(new { error = "BadRequest", message = "Only JPG, PNG, and WebP image formats are permitted." });
            }

            var session = _sessionManager.GetCurrentSession(HttpContext);
            if (session == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == session.UserID);

            if (user == null)
            {
                return NotFound(new { error = "NotFound", message = "User record not found." });
            }

            using var stream = file.OpenReadStream();
            var pictureUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType, "raceday-avatars");
            user.ProfilePictureUrl = pictureUrl;
            await _context.SaveChangesAsync();

            var result = new UserProfileDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName ?? "Unknown",
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(result);
        }
    }
}
