using Isekai.VSlice.Core.Content;
using Isekai.VSlice.Core.Systems;

static string FindRepoRootFrom(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "Isekai.VSlice.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new DirectoryNotFoundException("Could not find repo root containing Isekai.VSlice.sln");
}

var repoRoot = FindRepoRootFrom(AppContext.BaseDirectory);
var contentDir = Path.Combine(repoRoot, "content", "vslice");

var pack = ContentLoader.LoadFromDirectory(contentDir);

// v0: pick first encounter unless overridden by arg
string? encounterId = args.FirstOrDefault(a => a.StartsWith("--enc=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2).ElementAtOrDefault(1);
var encounter = encounterId is null
    ? pack.Encounters[0]
    : pack.EncounterById[encounterId];

int seed = 12345;
var seedArg = args.FirstOrDefault(a => a.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2).ElementAtOrDefault(1);
if (seedArg is not null && int.TryParse(seedArg, out var parsed)) seed = parsed;

var battle = SpawnSystem.CreateBattle(pack, encounter, seed);
BattleRunner.RunAuto(battle, maxRounds: 200);

bool crawl = args.Any(a => a.Equals("--crawl", StringComparison.OrdinalIgnoreCase));

foreach (var line in battle.Log.Lines)
{
    if (crawl)
    {
        foreach (char c in line)
        {
            Console.Write(c);
            if (c != '\x1b' && !char.IsControl(c))
                Thread.Sleep(36);
        }
        Console.WriteLine();
        Thread.Sleep(360);
    }
    else
    {
        Console.WriteLine(line);
    }
}