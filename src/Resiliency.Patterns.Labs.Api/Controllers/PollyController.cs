using Microsoft.AspNetCore.Mvc;

using Resiliency.Patterns.Labs.Api.Models;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

using Serilog;

namespace Resiliency.Patterns.Labs.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollyController : ControllerBase
    {
        private readonly IHttpBinService _httpBinService;
        
        public PollyController(IHttpBinService httpBinService)
        {
            _httpBinService = httpBinService;
        }
        
        [HttpGet("{statusCode}", Name = "GetPolly")]
        public IActionResult Get(int statusCode, [FromQuery]string? type = null)
        {   
            try
            {
                var response = type switch
                {
                    null => _httpBinService.Get(statusCode),
                    "retry" => _httpBinService.GetWithRetryPolicy(500, 500, 500, statusCode),
                    "circuit-break" => _httpBinService.GetWithCircuitBreakerPolicy(statusCode),
                    "timeout" => _httpBinService.GetWithTimeoutPolicy(statusCode),
                    "bulkhead" => _httpBinService.GetWithBulkheadIsolation(statusCode),
                    _ => throw new ArgumentException("Invalid policy"),
                };
            
                var result = new PollyDto { Hello = "World", Status = response.Result };
            
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
