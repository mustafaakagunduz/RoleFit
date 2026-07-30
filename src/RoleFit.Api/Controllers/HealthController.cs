using Microsoft.AspNetCore.Mvc;
using RoleFit.Api.Contracts;

namespace RoleFit.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>Reports basic liveness info for uptime checks.</summary>
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        var version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "0.0.0";
        return Ok(new HealthResponse("healthy", version, DateTime.UtcNow));
    }
}
