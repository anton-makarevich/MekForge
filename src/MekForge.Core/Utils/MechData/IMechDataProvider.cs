namespace Sanet.MekForge.Core.Utils.MechData;

public interface IMechDataProvider
{
    MechData LoadMechFromTextData(IEnumerable<string> lines);
}