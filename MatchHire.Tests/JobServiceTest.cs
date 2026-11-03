using System.ComponentModel.DataAnnotations;
using MatchHire.Api.Common;
using MatchHire.Api.Data;
using MatchHire.Api.Jobs;

namespace MatchHire.Tests;

public class JobServiceTests
{
    private readonly AppDbContext _db = TestHelpers.CreateDb();
    private readonly JobService _service;
    private readonly Guid _owner = Guid.NewGuid();
    private readonly Guid _otherEmployer = Guid.NewGuid();

    public JobServiceTests()
    {
        _service = new JobService(_db);
    }

    [Fact]
    public async Task Create_UsesTheCallersIdAsEmployerId()
    {
        var job = await _service.CreateAsync(Request("Backend Developer"), _owner);

        Assert.Equal(_owner, job.EmployerId);
        Assert.Equal(_owner, _db.Jobs.Single().EmployerId);
    }

    [Fact]
    public async Task GetById_WhenJobDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_ByOwner_UpdatesTheJob()
    {
        var job = await _service.CreateAsync(Request("Backend Developer"), _owner);

        var updated = await _service.UpdateAsync(job.Id, Request("Senior Backend Developer"), _owner);

        Assert.Equal("Senior Backend Developer", updated.Title);
    }

    [Fact]
    public async Task Update_ByAnotherEmployer_ThrowsForbiddenAndKeepsTheJob()
    {
        var job = await _service.CreateAsync(Request("Backend Developer"), _owner);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.UpdateAsync(job.Id, Request("Hacked"), _otherEmployer));

        Assert.Equal("Backend Developer", _db.Jobs.Single().Title);
    }

    [Fact]
    public async Task Delete_ByAnotherEmployer_ThrowsForbiddenAndKeepsTheJob()
    {
        var job = await _service.CreateAsync(Request("Backend Developer"), _owner);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAsync(job.Id, _otherEmployer));

        Assert.Single(_db.Jobs);
    }

    [Fact]
    public async Task Delete_ByOwner_RemovesTheJob()
    {
        var job = await _service.CreateAsync(Request("Backend Developer"), _owner);

        await _service.DeleteAsync(job.Id, _owner);

        Assert.Empty(_db.Jobs);
    }

    [Fact]
    public async Task Search_FiltersByTitleAndRemote_AndPaginates()
    {
        await _service.CreateAsync(Request("Backend Developer", remote: true), _owner);
        await _service.CreateAsync(Request("Backend Engineer", remote: true), _owner);
        await _service.CreateAsync(Request("Backend Lead", remote: false), _owner);
        await _service.CreateAsync(Request("Frontend Developer", remote: true), _owner);

        var page = await _service.SearchAsync(new JobSearchQuery
        {
            Title = "Backend",
            Remote = true,
            Page = 0,
            Size = 1
        });

        Assert.Equal(2, page.TotalCount);
        Assert.Single(page.Items);
    }

    [Fact]
    public void JobRequest_WithMaxSalaryBelowMinSalary_IsInvalid()
    {
        var request = Request("Backend Developer") with { MinSalary = 100_000, MaxSalary = 50_000 };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JobRequest.MaxSalary)));
    }

    private static JobRequest Request(string title, bool remote = true) =>
        new(title, "Toronto", JobType.FullTime, 80_000, 120_000,
            "Build APIs with ASP.NET Core", remote, ["C#", "SQL"]);
}