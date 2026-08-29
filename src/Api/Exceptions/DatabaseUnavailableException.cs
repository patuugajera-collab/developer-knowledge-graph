namespace DeveloperKnowledgeGraph.Api.Exceptions;

/// <summary>
/// Thrown when the backing SQL database cannot be reached, which maps to a
/// 503 with a safe, generic message.
/// </summary>
public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}