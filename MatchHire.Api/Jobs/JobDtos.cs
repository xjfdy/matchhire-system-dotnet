using System.ComponentModel.DataAnnotations;

namespace MatchHire.Api.Jobs;

public record JobRequest(
    [Required, MaxLength(100)] string Title,
    [Required, MaxLength(100)] string Location,
    [Required] JobType? JobType,
    [Range(0, 10_000_000)] decimal MinSalary,
    [Range(0, 10_000_000)] decimal MaxSalary,
    [Required, MaxLength(2000)] string Description,
    bool Remote,
    List<string>? RequiredSkills) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (MaxSalary < MinSalary)
        {
            yield return new ValidationResult(
                "MaxSalary must be greater than or equal to MinSalary",
                [nameof(MaxSalary)]);
        }
    }
}

public record JobResponse(
    Guid Id,
    string Title,
    string Location,
    string JobType,
    decimal MinSalary,
    decimal MaxSalary,
    string Description,
    bool Remote,
    List<string> RequiredSkills,
    Guid EmployerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class JobSearchQuery
{
    public string? Title { get; set; }
    public bool? Remote { get; set; }
    public JobType? JobType { get; set; }
    public decimal? MinSalary { get; set; }

    [Range(0, int.MaxValue)]
    public int Page { get; set; } = 0;

    [Range(1, 50)]
    public int Size { get; set; } = 10;
}

public record PagedResponse<T>(List<T> Items, int Page, int Size, int TotalCount);