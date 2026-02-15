using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Content;

public sealed class ContentPack
{
    public required IReadOnlyList<ActorTemplateDto> ActorsPc { get; init; }
    public required IReadOnlyList<ActorTemplateDto> ActorsEnemy { get; init; }
    public required IReadOnlyList<AbilityTemplateDto> Abilities { get; init; }
    public required IReadOnlyList<StatusTemplateDto> Statuses { get; init; }
    public required IReadOnlyList<MapTemplateDto> Maps { get; init; }
    public required IReadOnlyList<EncounterTemplateDto> Encounters { get; init; }
    public required IReadOnlyList<EnemyPaletteDto> Palettes { get; init; }
    public required IReadOnlyList<RewardTableDto> Rewards { get; init; }

    // Indices
    public required IReadOnlyDictionary<string, ActorTemplateDto> PcById { get; init; }
    public required IReadOnlyDictionary<string, ActorTemplateDto> EnemyById { get; init; }
    public required IReadOnlyDictionary<string, AbilityTemplateDto> AbilityById { get; init; }
    public required IReadOnlyDictionary<string, StatusTemplateDto> StatusById { get; init; }
    public required IReadOnlyDictionary<string, MapTemplateDto> MapById { get; init; }
    public required IReadOnlyDictionary<string, EncounterTemplateDto> EncounterById { get; init; }
    public required IReadOnlyDictionary<string, EnemyPaletteDto> PaletteById { get; init; }
    public required IReadOnlyDictionary<string, RewardTableDto> RewardById { get; init; }
}