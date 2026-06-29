namespace RealWorldApp.QueryService.Models;

public class ArticleReadModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ViewCount { get; set; }  // Additional read-specific data
    public int LikeCount { get; set; }   // Additional read-specific data
}