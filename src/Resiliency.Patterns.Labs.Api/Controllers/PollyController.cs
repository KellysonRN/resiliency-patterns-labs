using Microsoft.AspNetCore.Mvc;

using Resiliency.Patterns.Labs.Api.Models;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

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
        public IActionResult Get(int statusCode)
        {
            var response = _httpBinService.Get(statusCode);
            
            var result = new PollyDto { Hello = "World", Status = response.Result };
            
            return Ok(result);
        }
    }
}
