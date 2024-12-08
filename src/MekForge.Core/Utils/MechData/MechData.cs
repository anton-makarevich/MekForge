using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Utils.Community;

namespace Sanet.MekForge.Core.Utils.MechData;

public class MechData
{
    public required string Chassis { get; init; }
    public required string Model { get; init; }
    public required int Mass { get; init; }
    public required int WalkMp { get; init; }
    public required Dictionary<PartLocation, ArmorLocation> ArmorValues { get; init; }
    public required Dictionary<PartLocation, List<string>> LocationEquipment { get; init; }
}