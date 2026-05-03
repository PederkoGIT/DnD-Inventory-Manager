namespace DnD_InventoryManager.Models;

public class ItemModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "/Resources/Images/dotnet_bot.png";
    public double Weight { get; set; }
    public int Quantity { get; set; } = 1;
    public int CharacterId { get; set; }
    public double TotalWeight => Weight * Quantity;
}