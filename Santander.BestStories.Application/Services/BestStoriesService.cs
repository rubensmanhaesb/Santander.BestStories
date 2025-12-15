using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Dto;
using Santander.BestStories.Application.Interfaces;

namespace Santander.BestStories.Application.Services;

public sealed class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _client;

    public BestStoriesService(IHackerNewsClient client)
        => _client = client;

    public async Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct = default)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be greater than 0.");
        if (n > 200) throw new ArgumentOutOfRangeException(nameof(n), "n must be <= 200.");

        var ids = await _client.GetBestStoryIdsAsync(ct);

        // pega um "pool" um pouco maior pra compensar itens nulos/deletados
        var take = Math.Min(ids.Count, Math.Max(n * 3, n));
        var candidates = ids.Take(take).ToArray();

        // Concorrência limitada (evita martelar a HN API)
        const int maxDop = 16;
        using var gate = new SemaphoreSlim(maxDop);

        var tasks = candidates.Select(async id =>
        {
            await gate.WaitAsync(ct);
            try
            {
                var item = await _client.GetItemAsync(id, ct);
                if (item is null) return null;

                // só queremos stories
                if (!string.Equals(item.Type, "story", StringComparison.OrdinalIgnoreCase))
                    return null;

                // alguns campos podem vir nulos
                var title = item.Title ?? string.Empty;
                var uri = item.Url ?? string.Empty;
                var postedBy = item.By ?? string.Empty;

                var epoch = item.Time ?? 0;
                var time = DateTimeOffset.FromUnixTimeSeconds(epoch);

                var score = item.Score ?? 0;
                var commentCount = item.Descendants ?? 0;

                return new BestStoryDto(
                    Title: title,
                    Uri: uri,
                    PostedBy: postedBy,
                    Time: time,
                    Score: score,
                    CommentCount: commentCount
                );
            }
            finally
            {
                gate.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.Score)
            .Take(n)
            .ToList();
    }
}
