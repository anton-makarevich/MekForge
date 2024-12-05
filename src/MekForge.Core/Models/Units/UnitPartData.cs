using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public record UnitPartData(
    string Name,
    int MaxArmor,
    int MaxStructure,
    int Slots,
    IReadOnlyList<Component> Components);
