namespace DocumentManager.Api.DTOs
{
    public class CreateDocumentDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ExpirationDays { get; set; }
    }
}
