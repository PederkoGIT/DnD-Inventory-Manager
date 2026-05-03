using DnD_InventoryManager.Api;
using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Models;
using Newtonsoft.Json.Linq;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class ItemMapper
{
    public partial ItemModel? ToModel(ItemEntity? item);
    
    [MapperIgnoreSource(nameof(ItemModel.TotalWeight))]
    public partial ItemEntity ToEntity(ItemModel itemModel);
    
    public partial IList<ItemModel> EntitiesToListModels(ICollection<ItemEntity> items);

    public static ItemModel EquipmentModelToItemModel(EquipmentModel equipment)
    {
        var description = (JArray)equipment.AdditionalProperties["desc"];
        return new ItemModel
        {
            Name = equipment.AdditionalProperties["name"] as string ?? "",
            Weight = equipment.AdditionalProperties["weight"] as long? ?? 0,
            Description = string.Join(", ", description)
        };
    }

    public static ItemModel MagicItemModelToItemModel(MagicItemModel magicItem)
    {
        return new ItemModel
        {
            Name = magicItem.Name,
            Description = string.Join(", ", magicItem.Desc)
        };
    }
}