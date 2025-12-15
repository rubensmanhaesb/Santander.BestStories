using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Dtos;
using Santander.BestStories.Application.Interfaces;
using Santander.BestStories.Application.Options;

namespace Santander.BestStories.Application.Services;

public sealed partial class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _client;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsOptions _options;
    private readonly ILogger<BestStoriesService> _logger;

    private const string BestIdsCacheKey = "hn:beststoryids";

    public BestStoriesService(
        IHackerNewsClient client,
        IMemoryCache cache,
        IOptions<HackerNewsOptions> options,
        ILogger<BestStoriesService> logger)
    {
        _client = client;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct = default)
    {
        LogFetchingStories(_logger, n);

        if (n <= 0)
        {
            LogInvalidN(_logger, n);
            return Array.Empty<BestStoryDto>();
        }

        if (n > _options.MaxN)
        {
            LogCappedN(_logger, n, _options.MaxN);
            n = _options.MaxN;
        }

        IReadOnlyList<long> ids;

        
        ids = await _cache.GetOrCreateAsync(BestIdsCacheKey, async entry =>
        {
            LogBestIdsCacheMiss(_logger);

            entry.AbsoluteExpirationRelativeToNow = _options.BestStoriesCacheTtl;
            return await _client.GetBestStoryIdsAsync(ct);
        }) ?? Array.Empty<long>();

        if (ids.Count == 0)
            return Array.Empty<BestStoryDto>();

        
        var targetPool = n * _options.PoolMultiplier;
        var take = Math.Min(ids.Count, Math.Min(_options.PoolMax, Math.Max(n, targetPool)));
        var slice = ids.Take(take).ToArray();

        var gate = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);

        var tasks = slice.Select(async id =>
        {
            await gate.WaitAsync(ct);
            try
            {
                var cacheKey = $"hn:item:{id}";
                var item = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _options.ItemCacheTtl;

                    try
                    {
                        return await _client.GetItemAsync(id, ct);
                    }
                    catch (Exception ex)
                    {
                        LogItemFetchFailed(_logger, id, ex);
                        return null;
                    }
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

        var filtered = results.Where(x => x is not null).Select(x => x!).ToList();

        LogItemsFetched(_logger, filtered.Count);

        return filtered
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
