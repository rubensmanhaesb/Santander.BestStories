using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Santander.BestStories.Application.Options;
using Santander.BestStories.Infrastructure.Clients;
using Xunit;

namespace Santander.BestStories.InfraStructure.Tests;

public class HackerNewsClientTests
{
    [Fact]
    public async Task GetBestStoryIdsAsync_ReturnsIds()
    {
        var handler = new StubHttpMessageHandler(req =>
        {
            Assert.EndsWith("/beststories.json", req.RequestUri!.AbsoluteUri);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "[1,2,3,4]",
                    Encoding.UTF8,
                    MediaTypeHeaderValue.Parse("application/json"))
            };
        });

        var sut = CreateSut(new HttpClient(handler));

        var ids = await sut.GetBestStoryIdsAsync(CancellationToken.None);

        Assert.Equal(new long[] { 1, 2, 3, 4 }, ids);
    }

    [Fact]
    public async Task GetItemAsync_ReturnsMappedItem()
    {
        var handler = new StubHttpMessageHandler(req =>
        {
            Assert.EndsWith("/item/123.json", req.RequestUri!.AbsoluteUri);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "id": 123,
                      "type": "story",
                      "by": "alice",
                      "time": 1700000000,
                      "title": "Hello",
                      "url": "https://example.com",
                      "score": 42,
                      "descendants": 7
                    }
                    """,
                    Encoding.UTF8,
                    MediaTypeHeaderValue.Parse("application/json"))
            };
        });

        var sut = CreateSut(new HttpClient(handler));

        var item = await sut.GetItemAsync(123, CancellationToken.None);

        Assert.NotNull(item);
        Assert.Equal(123, item!.Id);
    }

    [Fact]
    public async Task GetItemAsync_When404_ThrowsHttpRequestException()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var sut = CreateSut(new HttpClient(handler));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.GetItemAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetItemAsync_When500_ThrowsHttpRequestException()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var sut = CreateSut(new HttpClient(handler));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.GetItemAsync(500, CancellationToken.None));
    }

    private static HackerNewsClient CreateSut(HttpClient http)
        => new(
            http,
            Options.Create(new HackerNewsOptions
            {
                BaseUrl = "https://hacker-news.firebaseio.com"
            }));
}
