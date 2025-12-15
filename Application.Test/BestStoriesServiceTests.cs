using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Dtos;
using Santander.BestStories.Application.Options;
using Santander.BestStories.Application.Services;

public class BestStoriesServiceTests
{
    private readonly Mock<IHackerNewsClient> _clientMock = new();

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsTopN_OrderedByScoreDesc()
    {
        _clientMock
            .Setup(x => x.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 1, 2, 3, 4, 5 });

        var items = new Dictionary<long, HackerNewsItem?>
        {
            [1] = Item(1, "A", 10),
            [2] = Item(2, "B", 50),
            [3] = Item(3, "C", 30),
            [4] = Item(4, "D", 80),
            [5] = Item(5, "E", 20)
        };

        _clientMock
            .Setup(x => x.GetItemAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long id, CancellationToken _) => items[id]);

        var sut = CreateSut(_clientMock);

        var result = await sut.GetBestStoriesAsync(3);

        Assert.Equal(new[] { "D", "B", "C" }, result.Select(x => x.Title));
    }

    private static BestStoriesService CreateSut(Mock<IHackerNewsClient> clientMock)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());

        var options = Options.Create(new HackerNewsOptions
        {
            MaxN = 200,
            PoolMultiplier = 2,
            PoolMax = 500,
            MaxConcurrentRequests = 8,
            BestStoriesCacheTtl = TimeSpan.FromMinutes(5),
            ItemCacheTtl = TimeSpan.FromMinutes(5)
        });

        return new BestStoriesService(
            clientMock.Object,
            cache,
            options,
            NullLogger<BestStoriesService>.Instance);
    }

    private static HackerNewsItem Item(long id, string title, int score)
        => new()
        {
            Id = id,
            Title = title,
            By = "user",
            Url = "https://x",
            Time = 1700000000,
            Score = score,
            Descendants = 1,
            Type = "story"
        };
}
