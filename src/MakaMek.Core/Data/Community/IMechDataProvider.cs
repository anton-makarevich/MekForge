using Sanet.MakaMek.Core.Data.Units;

namespace Sanet.MakaMek.Core.Data.Community;

public interface IMechDataProvider
{
    UnitData LoadMechFromTextData(IEnumerable<string> lines);
}