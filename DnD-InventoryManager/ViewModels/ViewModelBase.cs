using CommunityToolkit.Mvvm.ComponentModel;

namespace DnD_InventoryManager.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }
}