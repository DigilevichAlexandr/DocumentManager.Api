using DocumentManager.Api.Data;
using DocumentManager.Api.DTOs;
using DocumentManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentManager.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DocumentsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("my")]
        public IActionResult MyDocuments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var docs = _db.Documents
                .Where(d => d.OwnerId == userId && d.Status == DocumentStatus.Active)
                .ToList();

            return Ok(docs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var doc = new Document
            {
                OwnerId = userId,
                Name = dto.Name,
                Description = dto.Description,
                ExpirationAt = DateTime.UtcNow.AddDays(dto.ExpirationDays)
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            return Ok(doc);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var doc = _db.Documents.FirstOrDefault(d => d.Id == id && d.OwnerId == userId);
            if (doc == null) return NotFound();

            doc.Status = DocumentStatus.Deleted;
            doc.DeletedAt = DateTime.UtcNow.AddDays(30);

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

}
