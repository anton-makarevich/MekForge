namespace Sanet.MekForge.Core.Data;

public interface IMechDataProvider
{
    Data.UnitData LoadMechFromTextData(IEnumerable<string> lines);
}