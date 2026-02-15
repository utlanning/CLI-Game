PS C:\Users\Administrator\Documents\CLI Game> dotnet run --project .\Isekai.VSlice.Cli\Isekai.VSlice.Cli.csproj -- --seed=3

=== SPAWN ===
Encounter: enc_v0_baseline_01  Map: map_12x12_arena_01  Seed: 3
pc_0   player Soldier (Tank)           HP=70 MP=12 SPD=8 @(1,10)
pc_1   player Wizard (Fire)            HP=40 MP=36 SPD=10 @(2,10)
pc_2   player Cleric (Grey)            HP=60 MP=20 SPD=8 @(1,11)
pc_3   player Marksman (Crossbow)      HP=50 MP=12 SPD=16 @(2,11)
en_0   enemy  River Crab               HP=60 MP=12 SPD=10 @(9,0)
en_1   enemy  Terrapinnikin (1000y)    HP=80 MP=12 SPD=6 @(10,1)
en_2   enemy  River Crab               HP=60 MP=12 SPD=10 @(9,1)

=== BATTLE ===
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 River Crab moves (9,0) -> (5,0)
en_2 River Crab moves (9,1) -> (5,1)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_0 Soldier (Tank) moves (1,10) -> (4,9)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
en_1 Terrapinnikin (1000y) moves (10,1) -> (6,1)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 River Crab moves (5,0) -> (4,3)
en_2 River Crab moves (5,1) -> (4,4)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_0 Soldier (Tank) moves (4,9) -> (4,5)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
en_0 River Crab moves (4,3) -> (5,5)
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 70->60 (-10) via none
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_1 Terrapinnikin (1000y) moves (6,1) -> (4,3)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_0 River Crab
pc_0 Soldier (Tank) hits en_0 River Crab 60->46 (-14) via none
pc_2 Cleric (Grey) moves (1,11) -> (3,9)
en_0 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_0 River Crab hits pc_0 Soldier (Tank) 60->50 (-10) via none
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 50->35 (-15) via none
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_0 River Crab hits pc_0 Soldier (Tank) 35->25 (-10) via none
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 25->15 (-10) via none
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_0 River Crab
pc_0 Soldier (Tank) uses ab_attack_basic on en_0 River Crab => MISS
pc_2 Cleric (Grey) moves (3,9) -> (5,7)
en_1 Terrapinnikin (1000y) moves (4,3) -> (3,5)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 80->74 (-6) via none
en_0 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_0 River Crab hits pc_0 Soldier (Tank) 15->5 (-10) via none
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 5->0 (-10) via none
*** KO: pc_0 Soldier (Tank)
pc_1 Wizard (Fire) acts: ab_firebolt -> en_1 Terrapinnikin (1000y)
pc_1 Wizard (Fire) hits en_1 Terrapinnikin (1000y) 74->55 (-19) via fire
  status: en_1 Terrapinnikin (1000y) gains st_burning (dur=2)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 55->49 (-6) via none
pc_2 Cleric (Grey) moves (5,7) -> (5,6)
DOT fire: en_1 Terrapinnikin (1000y) 49->44 (-5)
en_1 Terrapinnikin (1000y) moves (3,5) -> (4,6)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 44->38 (-6) via none
en_0 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_0 River Crab hits pc_2 Cleric (Grey) 60->48 (-12) via none
en_2 River Crab moves (4,4) -> (6,6)
pc_1 Wizard (Fire) acts: ab_firebolt -> en_1 Terrapinnikin (1000y)
pc_1 Wizard (Fire) hits en_1 Terrapinnikin (1000y) 38->19 (-19) via fire
  status: en_1 Terrapinnikin (1000y) gains st_burning (dur=2)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) uses ab_shoot_xbow on en_1 Terrapinnikin (1000y) => MISS
pc_2 Cleric (Grey) acts: ab_attack_basic -> en_0 River Crab
pc_2 Cleric (Grey) hits en_0 River Crab 46->36 (-10) via none
en_0 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_0 River Crab uses ab_attack_basic on pc_2 Cleric (Grey) => MISS
en_2 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_2 River Crab hits pc_2 Cleric (Grey) 48->36 (-12) via none
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 19->13 (-6) via none
DOT fire: en_1 Terrapinnikin (1000y) 13->8 (-5)
en_1 Terrapinnikin (1000y) acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_1 Terrapinnikin (1000y) hits pc_2 Cleric (Grey) 36->24 (-12) via none
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 8->2 (-6) via none
pc_2 Cleric (Grey) acts: ab_attack_basic -> en_0 River Crab
pc_2 Cleric (Grey) hits en_0 River Crab 36->26 (-10) via none
en_0 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_0 River Crab hits pc_2 Cleric (Grey) 24->12 (-12) via none
en_2 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_2 River Crab hits pc_2 Cleric (Grey) 12->0 (-12) via none
*** KO: pc_2 Cleric (Grey)
pc_1 Wizard (Fire) moves (2,10) -> (4,8)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) hits en_1 Terrapinnikin (1000y) 2->0 (-6) via none
*** KO: en_1 Terrapinnikin (1000y)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 River Crab moves (5,5) -> (4,7)
en_2 River Crab moves (6,6) -> (5,8)
pc_1 Wizard (Fire) acts: ab_attack_basic -> en_0 River Crab
pc_1 Wizard (Fire) hits en_0 River Crab 26->17 (-9) via none
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_0 River Crab
pc_3 Marksman (Crossbow) uses ab_shoot_xbow on en_0 River Crab => MISS
en_0 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_0 River Crab hits pc_1 Wizard (Fire) 40->24 (-16) via none
en_2 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_2 River Crab hits pc_1 Wizard (Fire) 24->8 (-16) via none
pc_1 Wizard (Fire) acts: ab_attack_basic -> en_0 River Crab
pc_1 Wizard (Fire) hits en_0 River Crab 17->8 (-9) via none
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_0 River Crab
pc_3 Marksman (Crossbow) hits en_0 River Crab 8->0 (-10) via none
*** KO: en_0 River Crab
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 60->50 (-10) via none
en_2 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_2 River Crab hits pc_1 Wizard (Fire) 8->0 (-16) via none
*** KO: pc_1 Wizard (Fire)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 50->40 (-10) via none
en_2 River Crab moves (5,8) -> (2,9)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_2 River Crab
pc_3 Marksman (Crossbow) uses ab_shoot_xbow on en_2 River Crab => MISS
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 40->30 (-10) via none
en_2 River Crab moves (2,9) -> (2,10)
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 30->18 (-12) via none
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) uses ab_attack_basic on en_2 River Crab => MISS
en_2 River Crab acts: ab_attack_basic -> pc_3 Marksman (Crossbow)
en_2 River Crab hits pc_3 Marksman (Crossbow) 50->36 (-14) via none
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 18->0 (-18) via none
*** KO: en_2 River Crab

=== RESULT ===
WIN