using DeveloperKnowledgeGraph.Api.Exceptions;

namespace DeveloperKnowledgeGraph.Api.Services;

public static class DepthGuard
{
    public const int DefaultMaxDepth = 3;
    private const int MaxLimit = 6;

    public static int Normalize(int? maxDepth, int fallback = DefaultMaxDepth)
    {
        if (maxDepth is null)
        {
            return fallback;
        }

        if (maxDepth < 1 || maxDepth > MaxLimit)
        {
            throw new ValidationException(
                $"maxDepth must be between 1 and {MaxLimit}.");
        }

        return maxDepth.Value;
    }

    public static int NormalizeDependencyDepth(int? maxDepth)
    {
        if (maxDepth is null)
        {
            return 5;
        }

        if (maxDepth < 1 || maxDepth > 10)
        {
            throw new ValidationException("maxDepth must be between 1 and 10.");
        }

        return maxDepth.Value;
    }
}