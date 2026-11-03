using MatchHire.Api.Common;
using MatchHire.Api.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchHire.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<JobResponse>>> Search([FromQuery] JobSearchQuery query)
    {
        return Ok(await jobService.SearchAsync(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetById(Guid id)
    {
        return Ok(await jobService.GetByIdAsync(id));
    }

    // Only employers can create, update or delete jobs
    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ProducesResponseType<JobResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<JobResponse>> Create(JobRequest request)
    {
        var job = await jobService.CreateAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
    }

    [Authorize(Roles = "Employer")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobResponse>> Update(Guid id, JobRequest request)
    {
        return Ok(await jobService.UpdateAsync(id, request, User.GetUserId()));
    }

    [Authorize(Roles = "Employer")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await jobService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }
}