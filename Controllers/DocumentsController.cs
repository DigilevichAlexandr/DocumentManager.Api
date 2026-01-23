using DocumentManager.Api.Data;
using DocumentManager.Api.DTOs;
using DocumentManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentManager.Api.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var docs = await _context.Documents
                .Where(d => d.OwnerId == userId && !d.IsDeleted)
                .ToListAsync();

            return Ok(docs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] Document doc)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            doc.OwnerId = userId;
            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();
            return Ok(doc);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null || doc.OwnerId != userId) return NotFound();
            doc.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }


}
