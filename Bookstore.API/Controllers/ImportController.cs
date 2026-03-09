using Asp.Versioning;
using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ImportController(IBookImportService bookImportService) : ControllerBase
{
    [HttpPost("trigger")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<int>> TriggerImportAsync()
    {
        var importedCount = await bookImportService.ImportBooksAsync();
        return Ok(importedCount);
    }
}
