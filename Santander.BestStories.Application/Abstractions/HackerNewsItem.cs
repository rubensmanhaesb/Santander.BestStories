namespace Santander.BestStories.Application.Abstractions;

public sealed record HackerNewsItem(
    long Id,
    string? Type,
    string? By,
    long? Time,
    string? Title,
    string? Url,
    int? Score,
    int? Descendants
);
