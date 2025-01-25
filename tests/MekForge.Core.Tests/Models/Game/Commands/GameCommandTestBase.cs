using Shouldly;
using Sanet.MekForge.Core.Models.Game.Commands;

namespace Sanet.MekForge.Core.Tests.Models.Game.Commands;

public abstract class GameCommandTestBase<T> where T : GameCommand
{
    protected abstract T CreateCommand();

    [Fact]
    public void CloneWithGameId_ShouldCreateNewInstanceWithNewGameId()
    {
        // Arrange
        var originalCommand = CreateCommand();
        var newGameId = Guid.NewGuid();

        // Act
        var clonedCommand = originalCommand.CloneWithGameId(newGameId);

        // Assert
        clonedCommand.ShouldNotBeSameAs(originalCommand);
        clonedCommand.ShouldBeOfType<T>();
        clonedCommand.GameOriginId.ShouldBe(newGameId);
        clonedCommand.Timestamp.ShouldBe(originalCommand.Timestamp);
        AssertCommandSpecificProperties(originalCommand, clonedCommand as T);
    }

    protected virtual void AssertCommandSpecificProperties(T original, T? cloned)
    {
        cloned.ShouldNotBeNull();
    }
}
