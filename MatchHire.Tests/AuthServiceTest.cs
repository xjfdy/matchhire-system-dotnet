using MatchHire.Api.Auth;
using MatchHire.Api.Common;
using MatchHire.Api.Data;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MatchHire.Tests;

public class AuthServiceTests
{
    private readonly AppDbContext _db = TestHelpers.CreateDb();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_db, new JwtTokenService(TestHelpers.CreateConfig()));
    }

    [Fact]
    public async Task Register_HashesPasswordAndNormalizesEmail()
    {
        await _service.RegisterAsync(Register("  Demo@Test.COM ", "Password123"));

        var user = _db.Users.Single();
        Assert.Equal("demo@test.com", user.Email);
        Assert.NotEqual("Password123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", user.PasswordHash));
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflict()
    {
        await _service.RegisterAsync(Register("demo@test.com", "Password123"));

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RegisterAsync(Register("DEMO@test.com", "Password456")));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenWithUserIdAndRole()
    {
        var registered = await _service.RegisterAsync(Register("demo@test.com", "Password123"));

        var token = await _service.LoginAsync(new LoginRequest("demo@test.com", "Password123"));

        var jwt = new JsonWebToken(token);
        Assert.Equal(registered.Id.ToString(), jwt.GetClaim("sub").Value);
        Assert.Equal("Employer", jwt.GetClaim("role").Value);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_ReturnTheSameError()
    {
        await _service.RegisterAsync(Register("demo@test.com", "Password123"));

        var wrongPassword = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(new LoginRequest("demo@test.com", "WrongPassword")));
        var unknownEmail = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(new LoginRequest("nobody@test.com", "Password123")));

        // Same message, so an attacker cannot tell which emails are registered
        Assert.Equal(wrongPassword.Message, unknownEmail.Message);
    }

    private static RegisterRequest Register(string email, string password) =>
        new(email, password, "Demo", "User", UserType.Employer);
}