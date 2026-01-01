using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
