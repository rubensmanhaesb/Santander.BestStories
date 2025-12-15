using Microsoft.Extensions.Logging;

namespace Santander.BestStories.Application.Services;

public sealed partial class BestStoriesService
{
    private static readonly Action<ILogger, int, Exception?> _fetchingStories =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1000, "FetchingStories"),
            "Fetching top {N} best stories");

    private static readonly Action<ILogger, int, Exception?> _invalidN =
        LoggerMessage.Define<int>(
            LogLevel.Warning,
            new EventId(1001, "InvalidN"),
            "Invalid n={N}. Returning empty result");

    private static readonly Action<ILogger, int, int, Exception?> _cappedN =
        LoggerMessage.Define<int, int>(
            LogLevel.Warning,
            new EventId(1002, "CappedN"),
            "Requested n={N} exceeds max={MaxN}. Capping");

    private static readonly Action<ILogger, Exception?> _bestIdsCacheMiss =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1003, "BestIdsCacheMiss"),
            "Cache MISS for best story IDs");

    private static readonly Action<ILogger, int, Exception?> _itemsFetched =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1004, "ItemsFetched"),
            "Fetched {Count} stories after filtering");

    private static readonly Action<ILogger, long, Exception?> _itemFetchFailed =
        LoggerMessage.Define<long>(
            LogLevel.Warning,
            new EventId(1005, "ItemFetchFailed"),
            "Failed to fetch item {ItemId}");

    // Wrappers (evita “nome não existe” e fica fácil de chamar)
    private static void LogFetchingStories(ILogger logger, int n) => _fetchingStories(logger, n, null);
    private static void LogInvalidN(ILogger logger, int n) => _invalidN(logger, n, null);
    private static void LogCappedN(ILogger logger, int n, int maxN) => _cappedN(logger, n, maxN, null);
    private static void LogBestIdsCacheMiss(ILogger logger) => _bestIdsCacheMiss(logger, null);
    private static void LogItemsFetched(ILogger logger, int count) => _itemsFetched(logger, count, null);
    private static void LogItemFetchFailed(ILogger logger, long itemId, Exception ex) => _itemFetchFailed(logger, itemId, ex);
}
