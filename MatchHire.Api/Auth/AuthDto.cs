using System.ComponentModel.DataAnnotations;

namespace MatchHire.Api.Auth;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required] UserType? UserType);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record LoginResponse(string Token);

public record UserResponse(Guid Id, string Email, string FirstName, string LastName, string UserType);