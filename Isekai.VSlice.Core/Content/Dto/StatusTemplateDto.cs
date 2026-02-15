using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class StatusTemplateDto
{
    [JsonPropertyName("status_id")] public required string StatusId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("duration_type")] public required string DurationType { get; init; } // "turns"
    [JsonPropertyName("default_duration")] public int DefaultDuration { get; init; }

    [JsonPropertyName("stacking")] public required StackingDto Stacking { get; init; }
    [JsonPropertyName("contradictions")] public List<string> Contradictions { get; init; } = [];

    [JsonPropertyName("effects")] public List<StatusEffectDto> Effects { get; init; } = [];
    [JsonPropertyName("flags")] public required StatusFlagsDto Flags { get; init; }
}

public sealed class StackingDto
{
    [JsonPropertyName("mode")] public required string Mode { get; init; } // "refresh"
    [JsonPropertyName("cap")] public int Cap { get; init; }
    [JsonPropertyName("refresh_rule")] public string? RefreshRule { get; init; }
}

public sealed class StatusFlagsDto
{
    [JsonPropertyName("dispellable")] public bool Dispellable { get; init; }
    [JsonPropertyName("unique")] public bool Unique { get; init; }
}

public sealed class StatusEffectDto
{
    [JsonPropertyName("type")] public required string Type { get; init; }

    // Common numeric knobs (only some will be used depending on Type)
    [JsonPropertyName("mult")] public double? Mult { get; init; }
    [JsonPropertyName("amount")] public double? Amount { get; init; }
    [JsonPropertyName("min")] public double? Min { get; init; }

    // DoT
    [JsonPropertyName("timing")] public string? Timing { get; init; } // "start_of_turn"

    // Flag effect
    [JsonPropertyName("flag")] public string? Flag { get; init; }
    [JsonPropertyName("value")] public bool? Value { get; init; }
}