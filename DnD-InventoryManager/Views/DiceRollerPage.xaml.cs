using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnD_InventoryManager.ViewModels;

namespace DnD_InventoryManager.Views;

public partial class DiceRollerPage : ContentPage
{
    public DiceRollerPage(DiceRollerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
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
                viewModel.Roll();
            });
        }
    }
}