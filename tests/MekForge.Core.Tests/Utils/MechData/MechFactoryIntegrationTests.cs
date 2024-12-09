using FluentAssertions;
using Sanet.MekForge.Core.Models.Units.Mechs;
using Sanet.MekForge.Core.Utils.MechData;
using Sanet.MekForge.Core.Utils.MechData.Community;
using Sanet.MekForge.Core.Utils.TechRules;
using System.IO;

namespace Sanet.MekForge.Core.Tests.Utils.MechData;

public class MechFactoryIntegrationTests
{   
    private const string MechDirectory = "Resources/Mechs";
    
    [Fact]
    public void CreateFromMtfFiles_IntegrationTest()
    {
        // Arrange
        var parser = new MtfDataProvider();
        var structureValueProvider = new ClassicBattletechRulesProvider(); // Use actual provider
        var mechFactory = new MechFactory(structureValueProvider);

        // Get all MTF files from Resources/Mechs
        var mtfFiles = Directory.GetFiles(MechDirectory, "*.mtf");

        foreach (var mtfFile in mtfFiles)
        {
            Mech? createdMech = null;
            // Act
            var act = () =>
            {
                var mtfData = File.ReadAllLines(mtfFile);
                var mechData = parser.LoadMechFromTextData(mtfData);
                createdMech = mechFactory.Create(mechData);
            };

            // Assert
            act.Should().NotThrow();
            createdMech.Should().NotBeNull();
        }
    }
}
