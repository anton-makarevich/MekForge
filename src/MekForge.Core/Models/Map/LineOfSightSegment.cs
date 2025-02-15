namespace Sanet.MekForge.Core.Models.Map;

/// <summary>
/// Represents a segment in the line of sight path.
/// When second option is present, the two coordinates represent equally valid options for the LOS path,
/// </summary>
public readonly record struct LineOfSightSegment(HexCoordinates MainOption, HexCoordinates? SecondOption = null);