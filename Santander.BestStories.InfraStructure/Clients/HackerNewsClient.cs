using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Infrastructure.Options;

namespace Santander.BestStories.Infrastructure.Clients;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient, IOptions<HackerNewsOptions> options)
    {
        _httpClient = httpClient;

        // garante que o HttpClient aponte para a base correta
        var baseUrl = options.Value.BaseUrl?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("HackerNews BaseUrl is not configured.");

        _httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    }

    public async Task<IReadOnlyList<long>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        // beststories.json retorna array de IDs
        var ids = await _httpClient.GetFromJsonAsync<long[]>("beststories.json", ct);
        return ids ?? Array.Empty<long>();
    }

    public async Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct)
    {
        // item/{id}.json retorna um item ou null
        return await _httpClient.GetFromJsonAsync<HackerNewsItem>($"item/{id}.json", ct);
    }
}
