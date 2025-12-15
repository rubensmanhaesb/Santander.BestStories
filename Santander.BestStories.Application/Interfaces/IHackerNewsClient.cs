namespace Santander.BestStories.Application.Abstractions;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct = default);

    Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct = default);
}
