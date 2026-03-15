using Asp.Versioning;
using Bookstore.API.Constants;
using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookstore.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting(RateLimitConstants.AuthenticatedPolicyName)]
public class ImportController(IBookImportService bookImportService) : ControllerBase
{
    [HttpPost("trigger")]
    [Authorize(Roles = Roles.ReadWrite)]
    public async Task<ActionResult<int>> TriggerImport()
    {
        var importedCount = await bookImportService.ImportBooksAsync();
        return Ok(importedCount);
    }
}
