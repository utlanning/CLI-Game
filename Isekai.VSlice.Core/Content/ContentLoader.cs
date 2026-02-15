using System.Text.Json;
using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Content;

public static class ContentLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = false, // we use JsonPropertyName explicitly
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = true
    };

    public static ContentPack LoadFromDirectory(string contentDir)
    {
        if (string.IsNullOrWhiteSpace(contentDir))
            throw new ArgumentException("contentDir is required.", nameof(contentDir));

        if (!Directory.Exists(contentDir))
            throw new DirectoryNotFoundException($"Content directory not found: {contentDir}");

        var actorsPc = ReadArray<ActorTemplateDto>(Path.Combine(contentDir, "actors_pc.json"));
        var actorsEnemy = ReadArray<ActorTemplateDto>(Path.Combine(contentDir, "actors_enemy.json"));
        var abilities = ReadArray<AbilityTemplateDto>(Path.Combine(contentDir, "abilities.json"));
        var statuses = ReadArray<StatusTemplateDto>(Path.Combine(contentDir, "statuses.json"));
        var maps = ReadArray<MapTemplateDto>(Path.Combine(contentDir, "maps.json"));
        var encounters = ReadArray<EncounterTemplateDto>(Path.Combine(contentDir, "encounters.json"));
        var palettes = ReadArray<EnemyPaletteDto>(Path.Combine(contentDir, "palettes.json"));
        var rewards = ReadArray<RewardTableDto>(Path.Combine(contentDir, "rewards.json"));

        var pack = new ContentPack
        {
            ActorsPc = actorsPc,
            ActorsEnemy = actorsEnemy,
            Abilities = abilities,
            Statuses = statuses,
            Maps = maps,
            Encounters = encounters,
            Palettes = palettes,
            Rewards = rewards,

            PcById = actorsPc.ToDictionary(x => x.ActorTemplateId, x => x),
            EnemyById = actorsEnemy.ToDictionary(x => x.ActorTemplateId, x => x),
            AbilityById = abilities.ToDictionary(x => x.AbilityId, x => x),
            StatusById = statuses.ToDictionary(x => x.StatusId, x => x),
            MapById = maps.ToDictionary(x => x.MapId, x => x),
            EncounterById = encounters.ToDictionary(x => x.EncounterTemplateId, x => x),
            PaletteById = palettes.ToDictionary(x => x.EnemyPaletteId, x => x),
            RewardById = rewards.ToDictionary(x => x.RewardTableId, x => x),
        };

        ContentValidator.ValidateOrThrow(pack);
        return pack;
    }

    private static List<T> ReadArray<T>(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Missing content file: {filePath}", filePath);

        var json = File.ReadAllText(filePath);

        var data = JsonSerializer.Deserialize<List<T>>(json, JsonOpts);
        if (data is null)
            throw new ContentException(new[] { $"Failed to deserialize {Path.GetFileName(filePath)} into List<{typeof(T).Name}>." });

        return data;
    }
}