using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Santander.BestStories.Api.Controllers;
using Santander.BestStories.Application.Interfaces;
using Santander.BestStories.Application.Dtos; // ajuste o namespace do BestStoryDto
using Xunit;

public class StoriesControllerTests
{
    [Fact]
    public async Task GetBest_WhenNIsValid_ReturnsOkWithResult()
    {
        // arrange
        var serviceMock = new Mock<IBestStoriesService>();

        IReadOnlyList<BestStoryDto> expected = new List<BestStoryDto>
        {
            new() { Title = "t1" },
            new() { Title = "t2" }
        };

        serviceMock
            .Setup(s => s.GetBestStoriesAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected); 

        var controller = new StoriesController(serviceMock.Object);

        
        var result = await controller.GetBest(20, CancellationToken.None);

        
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(expected);
    }
}
