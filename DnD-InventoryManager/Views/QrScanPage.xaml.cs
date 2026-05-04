using DnD_InventoryManager.ViewModels;
using ZXing.Net.Maui;

namespace DnD_InventoryManager.Views;

public partial class QrScanPage : ContentPage
{
    private readonly QrScanViewModel _viewModel;

    public QrScanPage(QrScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            TryHarder = true,
            Multiple = false
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.IsDetecting = false;
    }

    private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first != null && !string.IsNullOrWhiteSpace(first.Value))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _viewModel.ProcessBarcodeResultAsync(first.Value);
            });
        }
    }
}