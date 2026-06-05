using Microsoft.AspNetCore.Mvc;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável por expor o endpoint de verificação de integridade e saúde da API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica a integridade e saúde do sistema, retornando uma resposta de status positiva se a API estiver operacional.
    /// </summary>
    /// <returns>Um IActionResult contendo um objeto com o status da aplicação.</returns>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
