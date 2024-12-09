using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Utils.MechData;

public record struct MechData
{
    public required string Chassis { get; init; }
    public required string Model { get; init; }
    public required int Mass { get; init; }
    public required int WalkMp { get; init; }
    public required Dictionary<PartLocation, ArmorLocation> ArmorValues { get; init; }
    public required Dictionary<PartLocation, List<string>> LocationEquipment { get; init; }
    public required Dictionary<string, string> AdditionalAttributes { get; set; }
    public required Dictionary<string,string> Quirks { get; set; }
}