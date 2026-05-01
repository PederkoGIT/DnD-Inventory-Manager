using SQLite;

namespace DnD_InventoryManager.Models;

public class EntityBase
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}