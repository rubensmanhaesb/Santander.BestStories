namespace Santander.BestStories.Application.Abstractions;


public sealed class HackerNewsItem
{
    public long Id { get; set; }
    public string? Type { get; set; }
    public string? By { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public long Time { get; set; }              // epoch seconds
    public int Score { get; set; }
    public int? Descendants { get; set; }       // comment count
}
