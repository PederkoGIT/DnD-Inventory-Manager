using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Models;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class ItemMapper
{
    public partial ItemModel? ToModel(ItemEntity? item);
    
    public partial ItemEntity ToEntity(ItemModel itemModel);
    
    public partial IList<ItemModel> EntitiesToListModels(ICollection<ItemEntity> items);
}