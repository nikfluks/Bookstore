using Bookstore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController(IBookImportService bookImportService) : ControllerBase
    {
        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerImport()
        {
            var importedCount = await bookImportService.ImportBooksAsync();
            return Ok(importedCount);
        }
    }
}
