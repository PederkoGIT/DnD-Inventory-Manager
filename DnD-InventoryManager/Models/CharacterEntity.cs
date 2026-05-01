using SQLite;

namespace DnD_InventoryManager.Models;

[Table("Characters")]
public class CharacterEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "dotnet_bot.png";
    public int Strength { get; set; }
    public int Size { get; set; }
}