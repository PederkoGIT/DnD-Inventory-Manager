using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    public partial string CalculationText { get; set; } = "Shake your phone";

    [ObservableProperty]
    public partial bool IsRolling { get; set; }
    public static List<string> DiceOptions { get; } = ["d100", "d20", "d12", "d10", "d8", "d6", "d4"];

    public DiceRollerViewModel()
    {
        Title = "Dice Roller";
    }

    [RelayCommand]
    public async Task RollAsync()
    {
        if (IsRolling)
        {
            return;
        }

        try
        {

            IsRolling = true;

            var sides = int.Parse(SelectedDice.Replace("d", ""));

            var roll = new Random().Next(1, sides + 1);
            var total = roll + Modifier;

            ResultText = total.ToString();

            var sign = Modifier >= 0 ? "+" : "";
            CalculationText = $"({roll}) {sign} {Modifier}";

            if (Vibration.Default.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50));
            }
            
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Roll fail: {ex.Message}");
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