using SQLite;

namespace DnD_InventoryManager.Models;

[Table("Items")]
public class ItemEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "/Resources/Images/dotnet_bot.png";
    public double Weight { get; set; }
    public int Quantity { get; set; }
    [Indexed]
    public int CharacterId { get; set; }
}