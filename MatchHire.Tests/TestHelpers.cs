using MatchHire.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MatchHire.Tests;

public static class TestHelpers
{
    public const string JwtKey = "test-only-signing-key-that-is-long-enough-for-hs256";

    public static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = "MatchHire",
                ["Jwt:Audience"] = "MatchHire",
                ["Jwt:ExpiresMinutes"] = "60"
            })
            .Build();
}