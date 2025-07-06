using Microsoft.AspNetCore.Mvc;

namespace WebApp3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnrichmentController : ControllerBase
    {
        private readonly ILogger<EnrichmentController> _logger;

        public EnrichmentController(ILogger<EnrichmentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("Got request from " + HttpContext.TraceIdentifier);
            return "Hello World";
        }
    }
}
