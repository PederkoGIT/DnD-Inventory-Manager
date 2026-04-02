namespace DnD_InventoryManager.Models;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "/Resources/Images/dotnet_bot.png";
    public int Strength { get; set; }
    public CharacterSizeEnum Size { get; set; }

    public double CarryingCapacity => Strength * 15;
    
    private double GetSizeModifier() => Size switch
    {
        CharacterSizeEnum.Tiny => 0.5,
        CharacterSizeEnum.Large => 2,
        CharacterSizeEnum.Huge => 4,
        CharacterSizeEnum.Gargantuan => 8,
        _ => 1
    };
}