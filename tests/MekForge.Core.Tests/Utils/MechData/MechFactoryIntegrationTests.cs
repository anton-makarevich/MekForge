using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Utils.MechData;
using Sanet.MekForge.Core.Utils.MechData.Community;
using Sanet.MekForge.Core.Utils.TechRules;

namespace Sanet.MekForge.Core.Tests.Utils.MechData;

public class MechFactoryIntegrationTests
{   
    private const string MtfToTest = "Resources/Shadow Hawk SHD-2D.mtf";
    
    [Fact(Skip = "Integration test, run manually.")]
    public void CreateFromMtfFile_IntegrationTest()
    {
        // Arrange
        var parser = new MtfDataProvider();
        var structureValueProvider = new ClassicBattletechRulesProvider(); // Use actual provider
        var mechFactory = new MechFactory(structureValueProvider);

        Mech? createdMech = null;
        // Act
        var act = () =>
        {
            var mtfData = File.ReadAllLines(MtfToTest);
            var mechData = parser.LoadMechFromTextData(mtfData);
            createdMech = mechFactory.Create(mechData);
        };

        // Assert
        act.Should().NotThrow();
        createdMech.Should().NotBeNull();
    }
}
