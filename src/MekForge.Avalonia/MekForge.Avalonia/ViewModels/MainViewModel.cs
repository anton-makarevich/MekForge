using CommunityToolkit.Mvvm.ComponentModel;

namespace MekForge.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
}