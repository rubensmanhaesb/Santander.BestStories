using Santander.BestStories.Application.Dtos;

namespace Santander.BestStories.Application.Interfaces;

public interface IBestStoriesService
{
    Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct = default);
}


