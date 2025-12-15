using Microsoft.AspNetCore.Mvc;
using Santander.BestStories.Application.Interfaces;

namespace Santander.BestStories.Api.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController : ControllerBase
{
    private readonly IBestStoriesService _service;

    public StoriesController(IBestStoriesService service)
    {
        _service = service;
    }

    [HttpGet("best")]
    public async Task<IActionResult> GetBest(
        [FromQuery] int n = 20,
        CancellationToken cancellationToken = default)
    {
        if (n <= 0 || n > 200)
            return BadRequest("Parameter 'n' must be between 1 and 200.");

        var result = await _service.GetBestStoriesAsync(n, cancellationToken);

        return Ok(result);
    }
}
