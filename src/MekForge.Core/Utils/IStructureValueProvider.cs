using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Utils;

public interface IStructureValueProvider
{
    Dictionary<PartLocation, int> GetStructureValues(int tonnage);
}