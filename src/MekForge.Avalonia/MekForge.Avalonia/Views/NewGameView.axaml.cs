using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sanet.MekForge.Core.Models.Units;
using Sanet.MekForge.Core.Utils.MechData;
using Sanet.MekForge.Core.Utils.MechData.Community;
using Sanet.MekForge.Core.Utils.TechRules;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MekForge.Avalonia.Views;

public partial class NewGameView : BaseView<NewGameViewModel>
{
    public NewGameView()
    {
        InitializeComponent();
    }
    
    private async Task LoadUnits()
    {
        if (ViewModel == null) return;
        var mtfDataProvider = new MtfDataProvider();
        var unitFiles = Directory.GetFiles("Resources/Units/Mechs", "*.mtf");
        var rulesProvider = new ClassicBattletechRulesProvider();
        var mechFactory = new MechFactory(rulesProvider);

        var units = new List<Unit>();
        foreach (var file in unitFiles)
        {
            var lines = await File.ReadAllLinesAsync(file);
            var mechData = mtfDataProvider.LoadMechFromTextData(lines);
            var mech = mechFactory.Create(mechData);
            
            units.Add(mech);
        }

        ViewModel.InitializeUnits(units);
    }

    protected async override void OnViewModelSet()
    {
        base.OnViewModelSet();
        await LoadUnits();
    }
}
