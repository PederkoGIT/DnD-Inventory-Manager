using SQLite;

namespace DnD_InventoryManager.Entities;

[Table("Categories")]
public class CategoryEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    
    [Indexed]
    public int CharacterId { get; set; }
    
    public bool IsSystem { get; set; }
    
}