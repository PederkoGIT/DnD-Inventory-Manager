using CommunityToolkit.Mvvm.ComponentModel;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class QrCodeDisplayViewModel : ViewModelBase
{
    private readonly QrService _qrService;

    [ObservableProperty]
    private string _qrPayload = string.Empty;

    [ObservableProperty]
    private ItemModel? _item;

    public QrCodeDisplayViewModel(QrService qrService)
    {
        _qrService = qrService;
        Title = "Share via QR";
    }

    partial void OnItemChanged(ItemModel? value)
    {
        if (value == null) return;
        Title = $"QR: {value.Name}";
        QrPayload = QrService.EncodeItem(value);
    }
}