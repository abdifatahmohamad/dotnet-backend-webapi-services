using Microsoft.AspNetCore.Mvc;

namespace Hello.Api.Controllers
{
    [ApiController]
    [Route("/health")] // can be [controller] instead of /health
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealthStatus()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}