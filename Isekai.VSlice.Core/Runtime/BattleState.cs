using Isekai.VSlice.Core.Content;
using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Runtime;

public sealed class BattleState
{
    public required ContentPack Content { get; init; }
    public required MapTemplateDto Map { get; init; }

    public required List<ActorInstance> Actors { get; init; }
    public required Random Rng { get; init; }
    public required BattleLog Log { get; init; }

    public int RoundCounter { get; set; } = 0;

    public IEnumerable<ActorInstance> AliveActors => Actors.Where(a => a.IsAlive);
    public IEnumerable<ActorInstance> AlivePlayers => Actors.Where(a => a.IsAlive && a.Faction.Equals("player", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<ActorInstance> AliveEnemies => Actors.Where(a => a.IsAlive && a.Faction.Equals("enemy", StringComparison.OrdinalIgnoreCase));

    public bool IsWin => !AliveEnemies.Any() && AlivePlayers.Any();
    public bool IsLose => !AlivePlayers.Any();

    public bool IsBlocked(int x, int y)
    {
        var t = Map.Tiles.FirstOrDefault(t => t.X == x && t.Y == y);
        return t is null || t.Blocked;
    }

    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Map.Size.W && y < Map.Size.H;

    public ActorInstance? ActorAt(int x, int y) => Actors.FirstOrDefault(a => a.IsAlive && a.X == x && a.Y == y);

    public int Manhattan(ActorInstance a, ActorInstance b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    public int Manhattan(int ax, int ay, int bx, int by) => Math.Abs(ax - bx) + Math.Abs(ay - by);

    public IEnumerable<(int x, int y)> Neighbors4(int x, int y)
    {
        yield return (x + 1, y);
        yield return (x - 1, y);
        yield return (x, y + 1);
        yield return (x, y - 1);
    }
}