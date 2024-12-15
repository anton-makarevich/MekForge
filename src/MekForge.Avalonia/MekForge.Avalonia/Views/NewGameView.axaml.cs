using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Data.Community;
using Sanet.MekForge.Core.Models.Units;
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
        var rulesProvider = new ClassicBattletechRulesProvider();
        var mechFactory = new MechFactory(rulesProvider);

        var assembly = typeof(App).Assembly;
        var resources = assembly.GetManifestResourceNames();

        var units = new List<Unit>();
        foreach (var resourceName in resources)
        {
            if (!resourceName.EndsWith(".mtf", StringComparison.OrdinalIgnoreCase)) continue;
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            using var reader = new StreamReader(stream);
            var mtfData = await reader.ReadToEndAsync();
            var mechData = mtfDataProvider.LoadMechFromTextData(mtfData.Split('\n'));
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
