using SQLite;

namespace DnD_InventoryManager.Entities;

public class EntityBase
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}