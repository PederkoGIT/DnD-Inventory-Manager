using CommunityToolkit.Mvvm.ComponentModel;
using DnD_InventoryManager.Models;
using DnD_InventoryManager.Services;

namespace DnD_InventoryManager.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class QrCodeDisplayViewModel : ViewModelBase
{
    private readonly QrService _qrService;
    
    [ObservableProperty]
    private string qrPayload = string.Empty;

    [ObservableProperty]
    private Character? character;

    public QrCodeDisplayViewModel(QrService qrService)
    {
        _qrService = qrService;
        Title = "Zdieľať cez QR";
    }
    
    partial void OnCharacterChanged(Character? value)
    {
        if (value != null)
        {
            Title = $"QR: {value.Name}";
            QrPayload = _qrService.EncodeCharacter(value);
        }
    }
}