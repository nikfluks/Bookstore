using Bookstore.Application.Interfaces;
using Bookstore.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
}
