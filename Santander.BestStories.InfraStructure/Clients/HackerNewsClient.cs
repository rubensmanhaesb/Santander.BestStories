using Microsoft.Extensions.Options;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Options;
using System.Net.Http.Json;

namespace Santander.BestStories.Infrastructure.Clients;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient, IOptions<HackerNewsOptions> options)
    {
        _httpClient = httpClient;

        
        var baseUrl = options.Value.BaseUrl?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("HackerNews BaseUrl is not configured.");

        _httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    }

    public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        
        var ids = await _httpClient.GetFromJsonAsync<long[]>("beststories.json", ct);
        return ids ?? Array.Empty<long>();
    }

    public async Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct)
    {
        
        return await _httpClient.GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", ct);
    }
}
