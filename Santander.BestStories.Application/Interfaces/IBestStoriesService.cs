using Santander.BestStories.Application.Dto;

namespace Santander.BestStories.Application.Interfaces;

public interface IBestStoriesService
{
    Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct = default);
}
