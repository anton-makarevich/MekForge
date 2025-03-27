using System.Text.Json;
using System.Text.Json.Serialization;
using Sanet.MekForge.Core.Exceptions;
using Sanet.MekForge.Core.Models.Game.Commands;
using Sanet.MekForge.Core.Models.Game.Commands.Client;
using Sanet.MekForge.Core.Models.Game.Commands.Server;
using Sanet.Transport;

namespace Sanet.MekForge.Core.Models.Game.Transport;

/// <summary>
/// Adapter that bridges between game commands and transport messages
/// </summary>
public class CommandTransportAdapter
{
    private readonly ITransportPublisher _transportPublisher;
    private readonly Dictionary<string, Type> _commandTypes;
    private readonly JsonSerializerOptions _serializerOptions;
    
    /// <summary>
    /// Creates a new instance of the CommandTransportAdapter
    /// </summary>
    /// <param name="transportPublisher">The transport publisher to use</param>
    public CommandTransportAdapter(ITransportPublisher transportPublisher)
    {
        _transportPublisher = transportPublisher;
        _commandTypes = InitializeCommandTypeDictionary();
        
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    /// <summary>
    /// Converts an IGameCommand to a TransportMessage and publishes it
    /// </summary>
    /// <param name="command">The command to publish</param>
    public void PublishCommand(IGameCommand command)
    {
        var message = new TransportMessage
        {
            MessageType = command.GetType().Name,
            SourceId = command.GameOriginId,
            Payload = SerializeCommand(command),
            Timestamp = command.Timestamp
        };
        
        _transportPublisher.PublishMessage(message);
    }
    
    /// <summary>
    /// Subscribes to transport messages and converts them back to IGameCommand
    /// </summary>
    /// <param name="onCommandReceived">Callback for received commands</param>
    public void Initialize(Action<IGameCommand> onCommandReceived)
    {
        _transportPublisher.Subscribe(message => {
            // Let the exception propagate for unknown command types
            // This is a critical error that should be handled at a higher level
            var command = DeserializeCommand(message);
            onCommandReceived(command);
        });
    }
    
    /// <summary>
    /// Serializes an IGameCommand to a JSON string
    /// </summary>
    /// <param name="command">The command to serialize</param>
    /// <returns>JSON representation of the command</returns>
    private string SerializeCommand(IGameCommand command)
    {
        return JsonSerializer.Serialize(command, command.GetType(), _serializerOptions);
    }
    
    /// <summary>
    /// Deserializes a TransportMessage payload to an IGameCommand
    /// </summary>
    /// <param name="message">The transport message to deserialize</param>
    /// <returns>The deserialized command</returns>
    /// <exception cref="UnknownCommandTypeException">Thrown when the command type is unknown</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when the JSON is invalid</exception>
    private IGameCommand DeserializeCommand(TransportMessage message)
    {
        if (!_commandTypes.TryGetValue(message.MessageType, out var commandType))
        {
            // Unknown command type - throw exception
            throw new UnknownCommandTypeException(message.MessageType);
        }
        
        try
        {
            var command = JsonSerializer.Deserialize(message.Payload, commandType, _serializerOptions) as IGameCommand;
            
            // Ensure the game origin ID and timestamp are set correctly
            if (command != null)
            {
                command.GameOriginId = message.SourceId;
                // Note: Timestamp is init-only, so it might already be set from deserialization
                return command;
            }
            
            throw new InvalidOperationException($"Failed to deserialize command of type {message.MessageType}");
        }
        catch (JsonException ex)
        {
            // Rethrow JSON deserialization errors
            throw new JsonException($"Error deserializing command of type {message.MessageType}: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Initializes a dictionary mapping command type names to their types
    /// This avoids using reflection for type resolution
    /// </summary>
    private Dictionary<string, Type> InitializeCommandTypeDictionary()
    {
        // Explicitly register all command types to avoid reflection
        // This could be auto-generated at build time if needed
        return new Dictionary<string, Type>
        {
            // Client commands
            { nameof(JoinGameCommand), typeof(JoinGameCommand) },
            { nameof(UpdatePlayerStatusCommand), typeof(UpdatePlayerStatusCommand) },
            { nameof(DeployUnitCommand), typeof(DeployUnitCommand) },
            { nameof(MoveUnitCommand), typeof(MoveUnitCommand) },
            { nameof(WeaponConfigurationCommand), typeof(WeaponConfigurationCommand) },
            { nameof(WeaponAttackDeclarationCommand), typeof(WeaponAttackDeclarationCommand) },
            { nameof(PhysicalAttackCommand), typeof(PhysicalAttackCommand) },
            { nameof(TurnEndedCommand), typeof(TurnEndedCommand) },
            { nameof(RollDiceCommand), typeof(RollDiceCommand) },
            
            // Server commands 
            { nameof(WeaponAttackResolutionCommand), typeof(WeaponAttackResolutionCommand) },
            { nameof(HeatUpdatedCommand), typeof(HeatUpdatedCommand) },
            { nameof(TurnIncrementedCommand), typeof(TurnIncrementedCommand) },
            { nameof(DiceRolledCommand), typeof(DiceRolledCommand) },
            { nameof(ChangePhaseCommand), typeof(ChangePhaseCommand) },
            { nameof(ChangeActivePlayerCommand), typeof(ChangeActivePlayerCommand) },
        };
    }
}
