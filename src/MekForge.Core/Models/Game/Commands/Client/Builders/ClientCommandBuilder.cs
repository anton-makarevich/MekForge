namespace Sanet.MekForge.Core.Models.Game.Commands.Client.Builders;

public abstract class ClientCommandBuilder
{
    protected readonly Guid GameId;
    protected readonly Guid PlayerId;

    protected ClientCommandBuilder(Guid gameId, Guid playerId)
    {
        GameId = gameId;
        PlayerId = playerId;
    }
    
    public abstract bool CanBuild { get; }

    public abstract ClientCommand? Build();
}
