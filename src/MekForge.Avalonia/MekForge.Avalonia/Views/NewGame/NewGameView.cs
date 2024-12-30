using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Data.Community;
using Sanet.MekForge.Core.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MekForge.Avalonia.Views.NewGame;

public abstract class NewGameView : BaseView<NewGameViewModel>
{
    private async Task LoadUnits()
    {
        if (ViewModel == null) return;
        var mtfDataProvider = new MtfDataProvider();

        var assembly = typeof(App).Assembly;
        var resources = assembly.GetManifestResourceNames();

        var units = new List<UnitData>();
        foreach (var resourceName in resources)
        {
            if (!resourceName.EndsWith(".mtf", StringComparison.OrdinalIgnoreCase)) continue;
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            using var reader = new StreamReader(stream);
            var mtfData = await reader.ReadToEndAsync();
            var mechData = mtfDataProvider.LoadMechFromTextData(mtfData.Split('\n'));
            //var mech = mechFactory.Create(mechData);
                
            units.Add(mechData);
        }

        ViewModel.InitializeUnits(units);
        
    }

    protected async override void OnViewModelSet()
    {
        base.OnViewModelSet();
        await LoadUnits();
    }
}
