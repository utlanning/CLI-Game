PS C:\Users\Administrator\Documents\CLI Game> dotnet run --project .\Isekai.VSlice.Cli\Isekai.VSlice.Cli.csproj -- --seed=2

=== SPAWN ===
Encounter: enc_v0_baseline_01  Map: map_12x12_arena_01  Seed: 2
pc_0   player Soldier (Tank)           HP=70 MP=12 SPD=8 @(1,10)
pc_1   player Wizard (Fire)            HP=40 MP=36 SPD=10 @(2,10)
pc_2   player Cleric (Grey)            HP=60 MP=20 SPD=8 @(1,11)
pc_3   player Marksman (Crossbow)      HP=50 MP=12 SPD=16 @(2,11)
en_0   enemy  Backahast Aetherii       HP=40 MP=32 SPD=12 @(9,1)

=== BATTLE ===
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_focus -> en_0 Backahast Aetherii
  status: en_0 Backahast Aetherii gains st_focused (dur=2)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) moves (1,10) -> (4,9)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_focus -> en_0 Backahast Aetherii
  status: en_0 Backahast Aetherii gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) moves (4,9) -> (8,9)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_focus -> en_0 Backahast Aetherii
  status: en_0 Backahast Aetherii gains st_focused (dur=2)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_focus -> en_0 Backahast Aetherii
  status: en_0 Backahast Aetherii gains st_focused (dur=2)
pc_0 Soldier (Tank) moves (8,9) -> (9,6)
pc_2 Cleric (Grey) moves (1,11) -> (3,9)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
en_0 Backahast Aetherii acts: ab_wind_nip_los -> pc_0 Soldier (Tank)
en_0 Backahast Aetherii hits pc_0 Soldier (Tank) 70->56 (-14) via wind
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) moves (9,6) -> (9,2)
pc_2 Cleric (Grey) moves (3,9) -> (7,9)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_wind_nip_los -> pc_0 Soldier (Tank)
en_0 Backahast Aetherii uses ab_wind_nip_los on pc_0 Soldier (Tank) => MISS
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii acts: ab_wind_nip_los -> pc_0 Soldier (Tank)
en_0 Backahast Aetherii hits pc_0 Soldier (Tank) 56->42 (-14) via wind
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_0 Backahast Aetherii
pc_0 Soldier (Tank) hits en_0 Backahast Aetherii 40->22 (-18) via none
pc_2 Cleric (Grey) moves (7,9) -> (9,7)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Backahast Aetherii waits.
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_0 Backahast Aetherii
pc_0 Soldier (Tank) hits en_0 Backahast Aetherii 22->0 (-27) via none
*** KO: en_0 Backahast Aetherii

=== RESULT ===
WIN