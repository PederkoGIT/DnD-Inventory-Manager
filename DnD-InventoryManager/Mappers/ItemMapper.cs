using DnD_InventoryManager.Models;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class ItemMapper
{
    public partial Item? ToModel(ItemEntity? item);
    
    public partial ItemEntity ToEntity(Item item);
    
    public partial IList<Item> EntitiesToListModels(ICollection<ItemEntity> items);
}