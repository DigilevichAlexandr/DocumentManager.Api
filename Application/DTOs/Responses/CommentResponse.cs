namespace DocumentManager.Api.Application.DTOs.Responses;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
}
