using Microsoft.AspNetCore.Mvc;

namespace SoporteCitas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { mensaje = "API funcionando", fecha = DateTime.UtcNow });
}