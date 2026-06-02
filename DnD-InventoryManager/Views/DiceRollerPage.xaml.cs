using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class DiceRollerPage : ContentPage
{
    public DiceRollerPage(DiceRollerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        viewModel.OnRollStarted = () =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                ShockwaveRing.Opacity = 0;
                ShockwaveRing.Scale = 1;

                var moveUp = DiceContainer.TranslateToAsync(0, -50, 400, Easing.CubicOut);
                var scaleUp = DiceContainer.ScaleToAsync(1.05, 400, Easing.CubicOut);

                await Task.WhenAll(moveUp, scaleUp);
            });
        };

        viewModel.OnRollFinished = () =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var moveDown = DiceContainer.TranslateToAsync(0, 0, 150, Easing.SpringIn);
                var scaleDown = DiceContainer.ScaleToAsync(1.0, 150, Easing.SpringIn);
                
                await Task.WhenAll(moveDown, scaleDown);

                ShockwaveRing.Opacity = 0.8;
                var expand = ShockwaveRing.ScaleToAsync(2.5, 400, Easing.CubicOut);
                var fade = ShockwaveRing.FadeToAsync(0, 400, Easing.CubicOut);
                
                await Task.WhenAll(expand, fade);
            });
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.ShakeDetected += Accelerometer_ShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.Game);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (Accelerometer.Default.IsSupported)
        {
            Accelerometer.Default.Stop();
            Accelerometer.Default.ShakeDetected -= Accelerometer_ShakeDetected;
        }
    }

    private void Accelerometer_ShakeDetected(object? sender, EventArgs eventArgs)
    {
        if (BindingContext is DiceRollerViewModel viewModel)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                viewModel.RollCommand.Execute(null);
            });
        }
    }
}