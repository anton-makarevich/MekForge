namespace Sanet.MakaMek.Core.Exceptions;

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
}
