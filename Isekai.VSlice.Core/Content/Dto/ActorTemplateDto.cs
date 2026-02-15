using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class ActorTemplateDto
{
    [JsonPropertyName("actor_template_id")] public required string ActorTemplateId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("faction")] public required string Faction { get; init; } // "player" | "enemy"
    [JsonPropertyName("tier")] public int Tier { get; init; }
    [JsonPropertyName("size")] public string? Size { get; init; } // "S"|"M"|"L"...
    [JsonPropertyName("role_tags")] public List<string> RoleTags { get; init; } = [];
    [JsonPropertyName("ai_profile")] public string? AiProfile { get; init; }
    [JsonPropertyName("base_stats")] public required BaseStatsDto BaseStats { get; init; }
    [JsonPropertyName("abilities")] public List<string> Abilities { get; init; } = [];
    [JsonPropertyName("starting_statuses")] public List<string> StartingStatuses { get; init; } = [];
    [JsonPropertyName("tags")] public List<string>? Tags { get; init; } // optional (present on PCs)
}

public sealed class BaseStatsDto
{
    [JsonPropertyName("max_hp")] public int MaxHp { get; init; }
    [JsonPropertyName("max_mp")] public int MaxMp { get; init; }
    [JsonPropertyName("atk")] public int Atk { get; init; }
    [JsonPropertyName("def")] public int Def { get; init; }
    [JsonPropertyName("int")] public int Int { get; init; }
    [JsonPropertyName("wis")] public int Wis { get; init; }

    [JsonPropertyName("accuracy_mod")] public double AccuracyMod { get; init; }
    [JsonPropertyName("evasion_mod")] public double EvasionMod { get; init; }

    [JsonPropertyName("speed")] public int Speed { get; init; }

}