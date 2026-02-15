using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class AbilityTemplateDto
{
    [JsonPropertyName("ability_id")] public required string AbilityId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("ct_cost")] public int CtCost { get; init; }
    [JsonPropertyName("mp_cost")] public int MpCost { get; init; }

    [JsonPropertyName("targeting")] public required TargetingDto Targeting { get; init; }
    [JsonPropertyName("resolution")] public required ResolutionDto Resolution { get; init; }
}

public sealed class TargetingDto
{
    [JsonPropertyName("mode")] public required string Mode { get; init; } // "single"|"self"|...
    [JsonPropertyName("range")] public int Range { get; init; }
    [JsonPropertyName("requires_los")] public bool RequiresLos { get; init; }
}

public sealed class ResolutionDto
{
    [JsonPropertyName("type")] public required string Type { get; init; } // "attack"|"heal"|"cleanse"|"utility"

    // attack
    [JsonPropertyName("damage")] public DamageBlockDto? Damage { get; init; }
    [JsonPropertyName("apply_statuses")] public List<ApplyStatusDto>? ApplyStatuses { get; init; }
    [JsonPropertyName("lifesteal")] public LifestealDto? Lifesteal { get; init; }

    // heal
    [JsonPropertyName("heal")] public HealDto? Heal { get; init; }

    // cleanse
    [JsonPropertyName("cleanse")] public CleanseDto? Cleanse { get; init; }

    // utility
    [JsonPropertyName("utility")] public UtilityDto? Utility { get; init; }
}

public sealed class DamageBlockDto
{
    [JsonPropertyName("physical")] public DamagePartDto? Physical { get; init; }
    [JsonPropertyName("elemental")] public DamagePartDto? Elemental { get; init; }
}

public sealed class DamagePartDto
{
    [JsonPropertyName("element")] public string? Element { get; init; } // "fire","water","none", etc.
    [JsonPropertyName("base")] public int Base { get; init; }
    [JsonPropertyName("stat")] public string? Stat { get; init; } // "atk","int","wis","none"
    [JsonPropertyName("scale")] public double Scale { get; init; }
}

public sealed class ApplyStatusDto
{
    [JsonPropertyName("status_id")] public required string StatusId { get; init; }
    [JsonPropertyName("duration")] public int Duration { get; init; }
    [JsonPropertyName("stacks")] public int Stacks { get; init; }
}

public sealed class LifestealDto
{
    [JsonPropertyName("fraction")] public double Fraction { get; init; } // e.g. 0.5
}

public sealed class HealDto
{
    [JsonPropertyName("base")] public int Base { get; init; }
    [JsonPropertyName("stat")] public string? Stat { get; init; } // "wis"
    [JsonPropertyName("scale")] public double Scale { get; init; }
}

public sealed class CleanseDto
{
    [JsonPropertyName("remove")] public int Remove { get; init; } // number of statuses to remove
}

public sealed class UtilityDto
{
    [JsonPropertyName("kind")] public required string Kind { get; init; } // "spawn_obstacle"|"free_step"
    [JsonPropertyName("terrain")] public string? Terrain { get; init; } // for spawn_obstacle
    [JsonPropertyName("duration_turns")] public int? DurationTurns { get; init; } // for spawn_obstacle
    [JsonPropertyName("tiles")] public int? Tiles { get; init; } // for free_step
}