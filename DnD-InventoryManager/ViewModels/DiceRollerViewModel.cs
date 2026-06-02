using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;

namespace DnD_InventoryManager.ViewModels;

public partial class DiceRollerViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string SelectedDice { get; set; } = "d20";

    [ObservableProperty]
    public partial int Modifier { get; set; } = 0;

    [ObservableProperty]
    public partial string ResultText { get; set; } = "-";

    [ObservableProperty]
    public partial string CalculationText { get; set; } = "Ready to roll";

    [ObservableProperty]
    public partial bool IsRolling { get; set; }
    
    public static List<string> DiceOptions { get; } = ["d100", "d20", "d12", "d10", "d8", "d6", "d4"];

    public Action? OnRollStarted { get; set; }
    public Action? OnRollFinished { get; set; }

    public DiceRollerViewModel()
    {
        Title = "Dice Roller";
    }

    [RelayCommand]
    public async Task RollAsync()
    {
        if (IsRolling) return;

        try
        {
            IsRolling = true;
            OnRollStarted?.Invoke(); 

            var sides = int.Parse(SelectedDice.Replace("d", ""));
            var rnd = new Random();
            var sign = Modifier >= 0 ? "+" : "";

            int animationSteps = 12;
            int delay = 70;

            for (int i = 0; i < animationSteps; i++)
            {
                var tempRoll = rnd.Next(1, sides + 1);
                var tempTotal = tempRoll + Modifier;
                
                ResultText = tempTotal.ToString();
                CalculationText = $"({tempRoll}) {sign} {Modifier}";

                if (Vibration.Default.IsSupported)
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50));
                }

                await Task.Delay(delay);
            }

            var finalRoll = rnd.Next(1, sides + 1);
            var total = finalRoll + Modifier;

            ResultText = total.ToString();
            CalculationText = $"({finalRoll}) {sign} {Modifier}";

            OnRollFinished?.Invoke();
            
            if (Vibration.Default.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(150));
            }
            
            await Task.Delay(400);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Roll fail: {ex.Message}");
            CalculationText = "Error!";
        }
        finally
        {
            IsRolling = false;
        }
    }

    [RelayCommand]
    private static async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}