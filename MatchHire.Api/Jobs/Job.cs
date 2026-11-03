namespace MatchHire.Api.Jobs;

public enum JobType { FullTime, PartTime, Internship, Contract }

public class Job
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType JobType { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Remote { get; set; }
    public List<string> RequiredSkills { get; set; } = [];
    public Guid EmployerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}