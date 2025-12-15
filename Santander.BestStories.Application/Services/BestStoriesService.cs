using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Dtos;
using Santander.BestStories.Application.Interfaces;
using Santander.BestStories.Application.Options;

namespace Santander.BestStories.Application.Services;

public sealed class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _client;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsOptions _options;

    private const string BestIdsCacheKey = "hn:beststoryids";

    public BestStoriesService(
        IHackerNewsClient client,
        IMemoryCache cache,
        IOptions<HackerNewsOptions> options)
    {
        _client = client;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct = default)
    {
        if (n <= 0) return Array.Empty<BestStoryDto>();
        if (n > 200) n = 200; // limite defensivo

        // 1) cache dos IDs
        var ids = await _cache.GetOrCreateAsync(BestIdsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _options.BestStoriesCacheTtl;
            return await _client.GetBestStoryIdsAsync(ct);
        }) ?? Array.Empty<long>();

        // Para reduzir chamadas, pega um pool maior que n (tradeoff simples)
        var take = Math.Min(ids.Count, Math.Max(n * 3, n));
        var slice = ids.Take(take).ToArray();

        // 2) buscar itens com cache por item + concorrência limitada
        var gate = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
        var tasks = slice.Select(async id =>
        {
            await gate.WaitAsync(ct);
            try
            {
                var itemKey = $"hn:item:{id}";
                var item = await _cache.GetOrCreateAsync(itemKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _options.ItemCacheTtl;
                    return await _client.GetItemAsync(id, ct);
                });

                if (item is null) return null;
                if (!string.Equals(item.Type, "story", StringComparison.OrdinalIgnoreCase)) return null;

                return MapToDto(item);
            }
            finally
            {
                gate.Release();
            }
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.Score)
            .Take(n)
            .ToList();
    }

    private static BestStoryDto MapToDto(HackerNewsItem item)
    {
        var time = DateTimeOffset.FromUnixTimeSeconds(item.Time);

        return new BestStoryDto
        {
            Title = item.Title ?? "",
            Uri = item.Url ?? "",
            PostedBy = item.By ?? "",
            Time = time,
            Score = item.Score,
            CommentCount = item.Descendants ?? 0
        };
    }
}
