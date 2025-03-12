using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sanet.MekForge.Core.Data.Community;
using Sanet.MekForge.Core.Data.Units;
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
            // Use Environment.NewLine or split on both types of line endings
            var lines = mtfData.Split(["\r\n", "\n"], StringSplitOptions.None);
            var mechData = mtfDataProvider.LoadMechFromTextData(lines);
                
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
