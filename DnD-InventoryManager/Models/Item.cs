namespace DnD_InventoryManager.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "/Resources/Images/dotnet_bot.png";
    public double Weight { get; set; }
}