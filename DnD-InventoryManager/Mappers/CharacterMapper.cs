using DnD_InventoryManager.Entities;
using DnD_InventoryManager.Models;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class CharacterMapper
{
    public partial CharacterModel? ToModel(CharacterEntity? entity);

    [MapperIgnoreSource(nameof(CharacterModel.CarryingCapacity))]
    public partial CharacterEntity ToEntity(CharacterModel model);
    
    public partial IList<CharacterModel> EntitiesToListModels(ICollection<CharacterEntity> items);
    
}