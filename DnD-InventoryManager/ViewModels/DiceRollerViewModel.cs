using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DnD_InventoryManager.ViewModels;

public partial class DiceRollerViewModel : ViewModelBase
{
    [ObservableProperty] private string _selectedDice = "d20";
    [ObservableProperty] private int _modifier = 0;
    [ObservableProperty] private string _resultText = "-";
    [ObservableProperty] private string _calculationText = "Shake your phone";
    
    public List<string> DiceOptions { get; } = new() {"d100", "d20", "d12", "d10", "d8", "d6", "d4"};

    public DiceRollerViewModel()
    {
        Title = "Dice Roller";
    }

    [RelayCommand]
    public void Roll()
    {
        int sides = int.Parse(SelectedDice.Replace("d", ""));
        
        int roll = new Random().Next(1, sides + 1);
        int total = roll + Modifier;
        
        ResultText = total.ToString();

        string sign = Modifier >= 0 ? "+" : "";
        CalculationText = $"({roll}) {sign} {Modifier}";

        try
        {
            if (Vibration.Default.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50));
            }
        }
        catch
        {
            return;
        }

    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    
}