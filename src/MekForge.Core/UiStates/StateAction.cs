namespace Sanet.MekForge.Core.UiStates;

public record StateAction(string Label, bool IsVisible, Action OnExecute);