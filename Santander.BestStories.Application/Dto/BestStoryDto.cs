namespace Santander.BestStories.Application.Dtos;

public sealed record BestStoryDto
{
    public string Title { get; init; } = "";
    public string Uri { get; init; } = "";
    public string PostedBy { get; init; } = "";
    public DateTimeOffset Time { get; init; }
    public int Score { get; init; }
    public int CommentCount { get; init; }
}
