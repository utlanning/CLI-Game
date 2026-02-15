using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class RewardTableDto
{
    [JsonPropertyName("reward_table_id")] public required string RewardTableId { get; init; }
    [JsonPropertyName("rewards")] public List<RewardEntryDto> Rewards { get; init; } = [];
}

public sealed class RewardEntryDto
{
    [JsonPropertyName("type")] public required string Type { get; init; } // "gold"|"item_stub"
    [JsonPropertyName("min")] public int? Min { get; init; }
    [JsonPropertyName("max")] public int? Max { get; init; }

    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("rolls")] public int? Rolls { get; init; }
}