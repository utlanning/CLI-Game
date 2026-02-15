using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class EncounterTemplateDto
{
    [JsonPropertyName("encounter_template_id")] public required string EncounterTemplateId { get; init; }
    [JsonPropertyName("map_id")] public required string MapId { get; init; }

    [JsonPropertyName("budget")] public int Budget { get; init; }
    [JsonPropertyName("unit_cap")] public int UnitCap { get; init; }

    [JsonPropertyName("enemy_palette_id")] public required string EnemyPaletteId { get; init; }
    [JsonPropertyName("spawn_rules")] public required SpawnRulesDto SpawnRules { get; init; }

    // --- NEW: composition rules (previously authored in JSON but silently dropped) ---
    [JsonPropertyName("composition")] public CompositionDto? Composition { get; init; }
}

public sealed class SpawnRulesDto
{
    [JsonPropertyName("player_spawn")] public required string PlayerSpawn { get; init; } // "zone_A"
    [JsonPropertyName("enemy_spawn")] public required string EnemySpawn { get; init; } // "zone_B"
}

/// <summary>
/// Composition constraints for encounter enemy spawning.
/// All fields are optional — null/default = no constraint.
/// </summary>
public sealed class CompositionDto
{
    /// <summary>Minimum count of enemies matching each role tag. E.g. {"ranged": 1, "caster": 1}</summary>
    [JsonPropertyName("min_role_tags")] public Dictionary<string, int>? MinRoleTags { get; init; }

    /// <summary>Maximum count of enemies matching each role tag. E.g. {"melee": 8}</summary>
    [JsonPropertyName("max_role_tag_counts")] public Dictionary<string, int>? MaxRoleTagCounts { get; init; }

    /// <summary>Maximum copies of any single enemy template. Default: no limit.</summary>
    [JsonPropertyName("max_same_template")] public int MaxSameTemplate { get; init; } = int.MaxValue;

    /// <summary>If true, allow falling back to any affordable pick when composition goals can't be met.</summary>
    [JsonPropertyName("allow_fallback_if_budget_blocked")] public bool AllowFallbackIfBudgetBlocked { get; init; } = true;
}
