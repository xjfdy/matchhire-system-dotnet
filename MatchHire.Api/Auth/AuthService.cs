using MatchHire.Api.Common;
using MatchHire.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchHire.Api.Auth;

// like AuthServiceImpl
public class AuthService(AppDbContext db, JwtTokenService jwt)
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("Email is already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserType = request.UserType!.Value
        };

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Email is already registered");
        }

        return ToResponse(user);
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid email or password");
        }

        return jwt.CreateToken(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id)
                   ?? throw new NotFoundException("User not found");
        return ToResponse(user);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.UserType.ToString());
}