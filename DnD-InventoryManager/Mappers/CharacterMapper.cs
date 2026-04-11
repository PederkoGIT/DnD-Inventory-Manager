using DnD_InventoryManager.Models;
using Riok.Mapperly.Abstractions;

namespace DnD_InventoryManager.Mappers;

[Mapper]
public partial class CharacterMapper
{
    public partial Character ToModel(CharacterEntity entity);

    public partial CharacterEntity ToEntity(Character model);
}