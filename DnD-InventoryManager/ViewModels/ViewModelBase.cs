using CommunityToolkit.Mvvm.ComponentModel;

namespace DnD_InventoryManager.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    string? title;

    [ObservableProperty] 
    bool isBusy;
}