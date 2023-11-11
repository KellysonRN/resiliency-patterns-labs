using Microsoft.AspNetCore.Mvc;

using Resiliency.Patterns.Labs.Api.Models;

namespace Resiliency.Patterns.Labs.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollyController : ControllerBase
    {
        [HttpGet("{statusCode}", Name = "GetPolly")]
        public IActionResult Get(int statusCode)
        {
            var result = new PollyDto { Hello = "World", Status = statusCode };
            
            return Ok(result);
        }
    }
}
