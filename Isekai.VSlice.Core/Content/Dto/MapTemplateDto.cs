using System.Text.Json.Serialization;

namespace Isekai.VSlice.Core.Content.Dto;

public sealed class MapTemplateDto
{
    [JsonPropertyName("map_id")] public required string MapId { get; init; }
    [JsonPropertyName("size")] public required MapSizeDto Size { get; init; }
    [JsonPropertyName("tiles")] public List<TileDto> Tiles { get; init; } = [];
    [JsonPropertyName("spawn_zones")] public required Dictionary<string, List<PointDto>> SpawnZones { get; init; }
}

public sealed class MapSizeDto
{
    [JsonPropertyName("w")] public int W { get; init; }
    [JsonPropertyName("h")] public int H { get; init; }
}

public sealed class TileDto
{
    [JsonPropertyName("x")] public int X { get; init; }
    [JsonPropertyName("y")] public int Y { get; init; }
    [JsonPropertyName("terrain")] public required string Terrain { get; init; } // "floor"|"half_wall"
    [JsonPropertyName("blocked")] public bool Blocked { get; init; } // movement blocked
}

public sealed class PointDto
{
    [JsonPropertyName("x")] public int X { get; init; }
    [JsonPropertyName("y")] public int Y { get; init; }
}