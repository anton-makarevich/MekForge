namespace Sanet.MekForge.Core.Exceptions;

/// <summary>
/// Exception thrown when attempting to deserialize an unknown command type
/// </summary>
public class UnknownCommandTypeException : Exception
{
    /// <summary>
    /// The name of the unknown command type
    /// </summary>
    public string CommandType { get; }

    /// <summary>
    /// Creates a new instance of UnknownCommandTypeException
    /// </summary>
    /// <param name="commandType">The name of the unknown command type</param>
    public UnknownCommandTypeException(string commandType)
        : base($"Unknown command type: {commandType}")
    {
        CommandType = commandType;
    }

    /// <summary>
    /// Creates a new instance of UnknownCommandTypeException with an inner exception
    /// </summary>
    /// <param name="commandType">The name of the unknown command type</param>
    /// <param name="innerException">The inner exception</param>
    public UnknownCommandTypeException(string commandType, Exception innerException)
        : base($"Unknown command type: {commandType}", innerException)
    {
        CommandType = commandType;
    }
}
