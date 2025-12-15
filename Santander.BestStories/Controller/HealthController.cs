using Microsoft.AspNetCore.Mvc;

namespace Santander.BestStories.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new { status = "ok" });
}
