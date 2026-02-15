using Isekai.VSlice.Core.Content.Dto;

namespace Isekai.VSlice.Core.Runtime;

public sealed class ActorInstance
{
    public required string InstanceId { get; init; }     // unique in battle (e.g., "pc_0", "en_3")
    public required ActorTemplateDto Template { get; init; }

    public required int X { get; set; }
    public required int Y { get; set; }

    public required int Hp { get; set; }
    public required int Mp { get; set; }
    public required double Ct { get; set; } // 0..160
    public int CtBank { get; set; } = 0; // 0..60 carryover into next eligible turn

    public List<StatusInstance> Statuses { get; } = new();

    public bool IsAlive => Hp > 0;

    public string Faction => Template.Faction;
    public string Name => Template.Name;

    // --- Computed stats incorporating active status effects ---
    public int Speed => Math.Max(1, Template.BaseStats.Speed + (int)SumStatusEffect("speed_add"));
    public int Atk   => Math.Max(1, Template.BaseStats.Atk   + (int)SumStatusEffect("atk_add"));
    public int Def   => Template.BaseStats.Def;
    public int Int   => Template.BaseStats.Int;
    public int Wis   => Template.BaseStats.Wis;

    public double AccuracyMod => Template.BaseStats.AccuracyMod;
    public double EvasionMod  => Template.BaseStats.EvasionMod;

    public bool HasStatus(string statusId) => Statuses.Any(s => s.Template.StatusId == statusId);

    public bool UsedSkirmishThisContact { get; set; } = false;

    public void RemoveStatus(string statusId) => Statuses.RemoveAll(s => s.Template.StatusId == statusId);

    public StatusInstance? GetStatus(string statusId) => Statuses.FirstOrDefault(s => s.Template.StatusId == statusId);

    // --- ANSI-colored tag: "{InstanceId} {Name}" with color based on HP% and faction ---
    //
    // PCs:     Green (100-75%) → Yellow (75-25%) → Red (25-0%)
    // Enemies: Blue  (100-75%) → Pink   (75-25%) → Carmine (25-0%)
    //
    public string ColorTag
    {
        get
        {
            int maxHp = Template.BaseStats.MaxHp;
            double pct = maxHp > 0 ? (double)Hp / maxHp : 0;
            bool isPlayer = Faction.Equals("player", StringComparison.OrdinalIgnoreCase);

            string ansi;
            if (isPlayer)
            {
                ansi = pct > 0.75 ? "\x1b[92m"          // bright green
                     : pct > 0.25 ? "\x1b[93m"          // bright yellow
                     :               "\x1b[91m";         // bright red
            }
            else
            {
                ansi = pct > 0.75 ? "\x1b[94m"          // bright blue
                     : pct > 0.25 ? "\x1b[38;5;218m"    // pastel pink (256-color)
                     :               "\x1b[38;5;160m";   // carmine (256-color)
            }

            return $"{ansi}{InstanceId} {Name}\x1b[0m";
        }
    }

    // --- Generic status-effect query helpers ---

    public double SumStatusEffect(string effectType)
    {
        double total = 0;
        foreach (var si in Statuses)
            foreach (var eff in si.Template.Effects)
                if (string.Equals(eff.Type, effectType, StringComparison.Ordinal))
                    total += eff.Amount ?? 0;
        return total;
    }

    public double ProductStatusEffect(string effectType)
    {
        double product = 1.0;
        foreach (var si in Statuses)
            foreach (var eff in si.Template.Effects)
                if (string.Equals(eff.Type, effectType, StringComparison.Ordinal))
                    product *= eff.Mult ?? 1.0;
        return product;
    }
}
