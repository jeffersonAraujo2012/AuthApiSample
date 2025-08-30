using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApiSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            return Ok($"Olá, {userName}! Você está autenticado.");
        }
    }
}
