using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MatchHire.Api.Auth;
using MatchHire.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MatchHire.Tests;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", TestHelpers.JwtKey);

        builder.ConfigureTestServices(services =>
        {
            // use an in-memory database
            var sqlServerConfig = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();
            foreach (var descriptor in sqlServerConfig)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

public class ApiTests
{
    [Fact]
    public async Task CreateJob_WithoutToken_Returns401()
    {
        using var factory = new TestApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/jobs", JobBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateJob_AsCandidate_Returns403()
    {
        using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        await LoginAs(client, "candidate@test.com", "Candidate");

        var response = await client.PostAsJsonAsync("/api/jobs", JobBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateJob_AsEmployer_Returns201()
    {
        using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        await LoginAs(client, "employer@test.com", "Employer");

        var response = await client.PostAsJsonAsync("/api/jobs", JobBody());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Login_SixthAttemptWithinAMinute_Returns429()
    {
        using var factory = new TestApiFactory();
        var client = factory.CreateClient();
        var body = new { email = "nobody@test.com", password = "Password123" };

        for (var i = 0; i < 5; i++)
        {
            var attempt = await client.PostAsJsonAsync("/api/auth/login", body);
            Assert.Equal(HttpStatusCode.Unauthorized, attempt.StatusCode);
        }

        var sixth = await client.PostAsJsonAsync("/api/auth/login", body);
        Assert.Equal(HttpStatusCode.TooManyRequests, sixth.StatusCode);
    }

    private static async Task LoginAs(HttpClient client, string email, string userType)
    {
        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Password123",
            firstName = "Demo",
            lastName = "User",
            userType
        });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123" });
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static object JobBody() => new
    {
        title = "Backend Developer",
        location = "Toronto",
        jobType = "FullTime",
        minSalary = 80000,
        maxSalary = 120000,
        description = "Build APIs with ASP.NET Core",
        remote = true,
        requiredSkills = new[] { "C#", "SQL" }
    };
}