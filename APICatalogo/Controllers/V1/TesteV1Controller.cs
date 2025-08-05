using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers.V1;

[Route("api/v{version:apiVersion}/teste")]
[ApiController]
[ApiVersion("1.0")]
public class TesteV1Controller : ControllerBase
{
    [HttpGet]
    public string GetVersion()
    {
        return "TesteV1 - GET - Api Versão 1.0";
    }
}