namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents a segment in the line of sight path.
/// When AreOptionsEqual is true, the two coordinates represent equally valid options for the LOS path,
/// and the defender should choose which one to use.
/// When AreOptionsEqual is false and SecondOption is not null, both coordinates should be included in the path sequentially.
/// </summary>
public readonly record struct LineOfSightSegment(HexCoordinates MainOption, HexCoordinates? SecondOption = null, bool AreOptionsEqual = false);