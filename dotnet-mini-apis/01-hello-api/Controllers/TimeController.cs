using Microsoft.AspNetCore.Mvc;

namespace Hello.Api.Controllers
{
    [ApiController]
    [Route("api/time")] // can be api/[controller] instead of api/time
    public class TimeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetCurrentTime()
        {
            return Ok(new { currentTime = System.DateTime.UtcNow });
        }
    }
}