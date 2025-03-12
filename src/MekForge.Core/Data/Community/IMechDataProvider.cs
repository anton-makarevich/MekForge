using Sanet.MekForge.Core.Data.Units;

namespace Sanet.MekForge.Core.Data.Community;

public interface IMechDataProvider
{
    UnitData LoadMechFromTextData(IEnumerable<string> lines);
}