using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.AuthService.Models.DTOs
{
    public record RegisterUserDto(string Username, string Password, Guid CustomerUid);

    public record LoginRequestDto(string Username, string Password);

    public record LoginResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

    public record RefreshTokenRequestDto(string RefreshToken);

    public record AssignRoleDto(Guid UserCredentialsUid, Guid RoleUid);

    public record CreateRoleDto(string Name, string? Description);
}
