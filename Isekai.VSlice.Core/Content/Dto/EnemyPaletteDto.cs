using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class EnemyPaletteDto
{
    [JsonPropertyName("enemy_palette_id")] public required string EnemyPaletteId { get; init; }
    [JsonPropertyName("entries")] public List<EnemyPaletteEntryDto> Entries { get; init; } = [];
}

public sealed class EnemyPaletteEntryDto
{
    [JsonPropertyName("enemy_template_id")] public required string EnemyTemplateId { get; init; }
    [JsonPropertyName("weight")] public int Weight { get; init; }
}