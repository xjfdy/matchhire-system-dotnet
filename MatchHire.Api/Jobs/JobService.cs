using MatchHire.Api.Common;
using MatchHire.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchHire.Api.Jobs;

public class JobService(AppDbContext db)
{
    public async Task<JobResponse> CreateAsync(JobRequest request, Guid employerId)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId // from token, not from the request body
        };
        Apply(job, request);

        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return ToResponse(job);
    }

    public async Task<JobResponse> GetByIdAsync(Guid id)
    {
        var job = await db.Jobs.FindAsync(id)
                  ?? throw new NotFoundException("Job not found");
        return ToResponse(job);
    }

    public async Task<PagedResponse<JobResponse>> SearchAsync(JobSearchQuery query)
    {
        // Build query like JPA Specifications.
        IQueryable<Job> jobs = db.Jobs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Title))
            jobs = jobs.Where(j => j.Title.Contains(query.Title));
        if (query.Remote is not null)
            jobs = jobs.Where(j => j.Remote == query.Remote);
        if (query.JobType is not null)
            jobs = jobs.Where(j => j.JobType == query.JobType);
        if (query.MinSalary is not null)
            jobs = jobs.Where(j => j.MaxSalary >= query.MinSalary);

        var total = await jobs.CountAsync();
        var items = await jobs
            .OrderByDescending(j => j.CreatedAt)
            .Skip(query.Page * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return new PagedResponse<JobResponse>(
            items.Select(ToResponse).ToList(), query.Page, query.Size, total);
    }

    public async Task<JobResponse> UpdateAsync(Guid id, JobRequest request, Guid employerId)
    {
        var job = await FindOwnedJobAsync(id, employerId);
        Apply(job, request);
        job.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return ToResponse(job);
    }

    public async Task DeleteAsync(Guid id, Guid employerId)
    {
        var job = await FindOwnedJobAsync(id, employerId);
        db.Jobs.Remove(job);
        await db.SaveChangesAsync();
    }

    // only the employer who posted the job can change it
    private async Task<Job> FindOwnedJobAsync(Guid id, Guid employerId)
    {
        var job = await db.Jobs.FindAsync(id)
                  ?? throw new NotFoundException("Job not found");

        if (job.EmployerId != employerId)
        {
            throw new ForbiddenException("You can only modify jobs you posted");
        }
        return job;
    }

    private static void Apply(Job job, JobRequest request)
    {
        job.Title = request.Title;
        job.Location = request.Location;
        job.JobType = request.JobType!.Value;
        job.MinSalary = request.MinSalary;
        job.MaxSalary = request.MaxSalary;
        job.Description = request.Description;
        job.Remote = request.Remote;
        job.RequiredSkills = request.RequiredSkills ?? [];
    }

    private static JobResponse ToResponse(Job job) => new(
        job.Id, job.Title, job.Location, job.JobType.ToString(),
        job.MinSalary, job.MaxSalary, job.Description, job.Remote,
        job.RequiredSkills, job.EmployerId, job.CreatedAt, job.UpdatedAt);
}