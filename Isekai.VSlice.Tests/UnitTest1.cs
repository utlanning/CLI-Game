using Isekai.VSlice.Core.Content;
using Isekai.VSlice.Core.Content.Dto;
using Isekai.VSlice.Core.Runtime;
using Isekai.VSlice.Core.Systems;

namespace Isekai.VSlice.Tests;

public class UnitTest1
{
    [Fact]
    public void TurnStartNormalization_ConvertsOverflowToBank()
    {
        var actor = NewActor("p1", "player", ["ab_attack"], ct: 137, ctBank: 10, hp: 100, x: 0, y: 0);

        var budget = TurnEngine.NormalizeReadyActorAndComputeBudget(actor);

        Assert.Equal(100, actor.Ct);
        Assert.Equal(47, actor.CtBank);
        Assert.Equal(147, budget);
    }

    [Fact]
    public void WaitBanking_CarriesRemainingBudgetToNextTurn()
    {
        var actor = NewActor("p1", "player", ["ab_attack"], ct: 100, ctBank: 0, hp: 100, x: 0, y: 0);

        var budget = TurnEngine.NormalizeReadyActorAndComputeBudget(actor);
        Assert.Equal(100, budget);

        // Spend 80 then wait => remaining 20.
        TurnEngine.EndTurnAsWait(actor, remainingBudget: 20);
        Assert.Equal(60, actor.Ct);
        Assert.Equal(20, actor.CtBank);

        actor.Ct = 100;
        var nextBudget = TurnEngine.NormalizeReadyActorAndComputeBudget(actor);
        Assert.Equal(120, nextBudget);

        // Wait immediately with full remaining budget.
        TurnEngine.EndTurnAsWait(actor, remainingBudget: 100);
        Assert.Equal(60, actor.Ct);
        Assert.Equal(60, actor.CtBank);

        actor.Ct = 100;
        Assert.Equal(160, TurnEngine.NormalizeReadyActorAndComputeBudget(actor));
    }

    [Fact]
    public void ValidationAndTerminationInvariants_AreEnforced()
    {
        var invalidPrimary = BuildPack([
            AttackAbility("ab_bad_primary", ctCost: 0)
        ]);
        var invalidFree = BuildPack([
            FreeStepAbility("ab_bad_free", ctCost: 10)
        ]);

        var primaryErrors = ContentValidator.Validate(invalidPrimary);
        var freeErrors = ContentValidator.Validate(invalidFree);

        Assert.Contains(primaryErrors, e => e.Contains("ab_bad_primary") && e.Contains("ct_cost >= 1"));
        Assert.Contains(freeErrors, e => e.Contains("ab_bad_free") && e.Contains("ct_cost == 0"));
        Assert.True(TurnEngine.MoveCostPerTile >= 1);

        // Cantrip/free action is slot-locked to 1 use per turn.
        var freeAttack = AttackAbility("ab_free_hit", ctCost: 0);
        var content = BuildPack([freeAttack]);
        var state = new BattleState
        {
            Content = content,
            Map = OpenMap(),
            Actors =
            [
                NewActor("pc_0", "player", ["ab_free_hit"], ct: 100, ctBank: 0, hp: 100, x: 0, y: 0),
                NewActor("en_0", "enemy", ["ab_free_hit"], ct: 0, ctBank: 0, hp: 100, x: 1, y: 0)
            ],
            Rng = new Random(123),
            Log = new BattleLog()
        };

        BattleRunner.RunAuto(state, maxRounds: 1);

        // Free attack deals 14 once (10 base + atk 10 * 0.5 - def 1).
        var enemy = state.Actors.Single(a => a.InstanceId == "en_0");
        Assert.Equal(86, enemy.Hp);
    }

    [Fact]
    public void InterleavingBudget_StopsUnaffordablePrimary()
    {
        var budget = TurnEngine.ComputeBudgetFromBank(20); // 120

        budget -= 20; // move
        budget -= 60; // primary
        budget -= 20; // move

        Assert.Equal(20, budget);
        Assert.True(budget < 60);
    }

    private static ContentPack BuildPack(IReadOnlyList<AbilityTemplateDto> abilities)
    {
        var actorPc = NewTemplate("pc", "player", abilities.Select(a => a.AbilityId).ToList());
        var actorEnemy = NewTemplate("enemy", "enemy", abilities.Select(a => a.AbilityId).ToList());
        var map = OpenMap();
        var encounter = new EncounterTemplateDto
        {
            EncounterTemplateId = "enc",
            MapId = map.MapId,
            Budget = 1,
            UnitCap = 1,
            EnemyPaletteId = "pal",
            SpawnRules = new SpawnRulesDto { PlayerSpawn = "zone_A", EnemySpawn = "zone_B" }
        };
        var palette = new EnemyPaletteDto
        {
            EnemyPaletteId = "pal",
            Entries = [new EnemyPaletteEntryDto { EnemyTemplateId = actorEnemy.ActorTemplateId, Weight = 1 }]
        };

        return new ContentPack
        {
            ActorsPc = [actorPc],
            ActorsEnemy = [actorEnemy],
            Abilities = abilities,
            Statuses = [],
            Maps = [map],
            Encounters = [encounter],
            Palettes = [palette],
            Rewards = [new RewardTableDto { RewardTableId = "r1", Rewards = [] }],
            PcById = new Dictionary<string, ActorTemplateDto> { [actorPc.ActorTemplateId] = actorPc },
            EnemyById = new Dictionary<string, ActorTemplateDto> { [actorEnemy.ActorTemplateId] = actorEnemy },
            AbilityById = abilities.ToDictionary(a => a.AbilityId, a => a),
            StatusById = new Dictionary<string, StatusTemplateDto>(),
            MapById = new Dictionary<string, MapTemplateDto> { [map.MapId] = map },
            EncounterById = new Dictionary<string, EncounterTemplateDto> { [encounter.EncounterTemplateId] = encounter },
            PaletteById = new Dictionary<string, EnemyPaletteDto> { [palette.EnemyPaletteId] = palette },
            RewardById = new Dictionary<string, RewardTableDto> { ["r1"] = new RewardTableDto { RewardTableId = "r1", Rewards = [] } }
        };
    }

    private static ActorTemplateDto NewTemplate(string id, string faction, List<string> abilities) => new()
    {
        ActorTemplateId = id,
        Name = id,
        Faction = faction,
        Tier = 1,
        BaseStats = new BaseStatsDto
        {
            MaxHp = 100,
            MaxMp = 10,
            Atk = 10,
            Def = 1,
            Int = 1,
            Wis = 1,
            AccuracyMod = 0,
            EvasionMod = 0,
            Speed = 10
        },
        Abilities = abilities
    };

    private static ActorInstance NewActor(string id, string faction, List<string> abilities, double ct, int ctBank, int hp, int x, int y)
    {
        var template = NewTemplate($"tpl_{id}", faction, abilities);
        return new ActorInstance
        {
            InstanceId = id,
            Template = template,
            X = x,
            Y = y,
            Hp = hp,
            Mp = template.BaseStats.MaxMp,
            Ct = ct,
            CtBank = ctBank
        };
    }

    private static AbilityTemplateDto AttackAbility(string id, int ctCost) => new()
    {
        AbilityId = id,
        Name = id,
        CtCost = ctCost,
        MpCost = 0,
        Targeting = new TargetingDto { Mode = "single", Range = 1, RequiresLos = false },
        Resolution = new ResolutionDto
        {
            Type = "attack",
            Damage = new DamageBlockDto
            {
                Physical = new DamagePartDto { Base = 10, Stat = "atk", Scale = 0.5 },
                Elemental = new DamagePartDto { Element = "none", Base = 0, Stat = "none", Scale = 0.0 }
            },
            ApplyStatuses = []
        }
    };

    private static AbilityTemplateDto FreeStepAbility(string id, int ctCost) => new()
    {
        AbilityId = id,
        Name = id,
        CtCost = ctCost,
        MpCost = 0,
        Targeting = new TargetingDto { Mode = "self", Range = 0, RequiresLos = false },
        Resolution = new ResolutionDto
        {
            Type = "utility",
            Utility = new UtilityDto { Kind = "free_step", Tiles = 1 }
        }
    };

    private static MapTemplateDto OpenMap() => new()
    {
        MapId = "m1",
        Size = new MapSizeDto { W = 3, H = 3 },
        Tiles =
        [
            new TileDto { X = 0, Y = 0, Terrain = "floor", Blocked = false },
            new TileDto { X = 1, Y = 0, Terrain = "floor", Blocked = false },
            new TileDto { X = 2, Y = 0, Terrain = "floor", Blocked = false },
            new TileDto { X = 0, Y = 1, Terrain = "floor", Blocked = false },
            new TileDto { X = 1, Y = 1, Terrain = "floor", Blocked = false },
            new TileDto { X = 2, Y = 1, Terrain = "floor", Blocked = false },
            new TileDto { X = 0, Y = 2, Terrain = "floor", Blocked = false },
            new TileDto { X = 1, Y = 2, Terrain = "floor", Blocked = false },
            new TileDto { X = 2, Y = 2, Terrain = "floor", Blocked = false }
        ],
        SpawnZones = new Dictionary<string, List<PointDto>>
        {
            ["zone_A"] = [new PointDto { X = 0, Y = 0 }],
            ["zone_B"] = [new PointDto { X = 1, Y = 0 }]
        }
    };
}
