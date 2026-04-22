namespace CashControl.Identity.Application.Services.DTOs;

public record class AuthResponseDto(string? AccessToken, string? RefreshToken, DateTime? RefreshTokenExpiresAtUtc);
