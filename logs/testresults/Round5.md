PS C:\Users\Administrator\Documents\CLI Game> dotnet run --project .\Isekai.VSlice.Cli\Isekai.VSlice.Cli.csproj -- --seed=4

=== SPAWN ===
Encounter: enc_v0_baseline_01  Map: map_12x12_arena_01  Seed: 4
pc_0   player Soldier (Tank)           HP=70 MP=12 SPD=8 @(1,10)
pc_1   player Wizard (Fire)            HP=40 MP=36 SPD=10 @(2,10)
pc_2   player Cleric (Grey)            HP=60 MP=20 SPD=8 @(1,11)
pc_3   player Marksman (Crossbow)      HP=50 MP=12 SPD=16 @(2,11)
en_0   enemy  Juvenile Hellbender      HP=60 MP=20 SPD=10 @(9,0)
en_1   enemy  River Crab               HP=60 MP=12 SPD=10 @(10,0)
en_2   enemy  River Crab               HP=60 MP=12 SPD=10 @(9,1)
en_3   enemy  Juvenile Hellbender      HP=60 MP=20 SPD=10 @(10,1)

=== BATTLE ===
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Juvenile Hellbender moves (9,0) -> (5,0)
en_1 River Crab moves (10,0) -> (6,0)
en_2 River Crab moves (9,1) -> (5,1)
en_3 Juvenile Hellbender moves (10,1) -> (6,1)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_0 Soldier (Tank) moves (1,10) -> (4,9)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_0 Juvenile Hellbender moves (5,0) -> (4,3)
en_1 River Crab moves (6,0) -> (4,2)
en_2 River Crab moves (5,1) -> (4,4)
en_3 Juvenile Hellbender moves (6,1) -> (5,4)
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_0 Soldier (Tank) moves (4,9) -> (4,5)
pc_2 Cleric (Grey) acts: ab_heal -> pc_2 Cleric (Grey)
pc_2 Cleric (Grey) uses ab_heal on pc_2 Cleric (Grey) HEAL 60->60
en_0 Juvenile Hellbender moves (4,3) -> (3,5)
en_1 River Crab moves (4,2) -> (4,3)
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 70->60 (-10) via none
en_3 Juvenile Hellbender moves (5,4) -> (5,5)
pc_1 Wizard (Fire) acts: ab_firebolt -> en_0 Juvenile Hellbender
pc_1 Wizard (Fire) hits en_0 Juvenile Hellbender 60->39 (-21) via fire
  status: en_0 Juvenile Hellbender gains st_burning (dur=2)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_0 Juvenile Hellbender
pc_3 Marksman (Crossbow) hits en_0 Juvenile Hellbender 39->29 (-10) via none
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_0 Juvenile Hellbender
pc_3 Marksman (Crossbow) uses ab_shoot_xbow on en_0 Juvenile Hellbender => MISS
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_0 Juvenile Hellbender
pc_0 Soldier (Tank) hits en_0 Juvenile Hellbender 29->15 (-14) via none
pc_2 Cleric (Grey) moves (1,11) -> (3,9)
DOT fire: en_0 Juvenile Hellbender 15->10 (-5)
en_0 Juvenile Hellbender acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_0 Juvenile Hellbender hits pc_0 Soldier (Tank) 60->51 (-9) via none
en_1 River Crab acts: ab_guard -> en_1 River Crab
  status: en_1 River Crab gains st_guarding (dur=1)
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab uses ab_attack_basic on pc_0 Soldier (Tank) => MISS
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_3 Juvenile Hellbender hits pc_0 Soldier (Tank) 51->42 (-9) via none
pc_1 Wizard (Fire) acts: ab_firebolt -> en_0 Juvenile Hellbender
pc_1 Wizard (Fire) hits en_0 Juvenile Hellbender 10->0 (-21) via fire
*** KO: en_0 Juvenile Hellbender
  status: en_0 Juvenile Hellbender gains st_burning (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_1 River Crab moves (4,3) -> (3,5)
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab hits pc_0 Soldier (Tank) 42->32 (-10) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_3 Juvenile Hellbender hits pc_0 Soldier (Tank) 32->23 (-9) via none
pc_1 Wizard (Fire) acts: ab_firebolt -> en_1 River Crab
pc_1 Wizard (Fire) hits en_1 River Crab 60->39 (-21) via fire
  status: en_1 River Crab gains st_burning (dur=2)
pc_0 Soldier (Tank) acts: ab_attack_basic -> en_1 River Crab
pc_0 Soldier (Tank) hits en_1 River Crab 39->25 (-14) via none
pc_2 Cleric (Grey) moves (3,9) -> (3,6)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_1 River Crab
pc_3 Marksman (Crossbow) uses ab_shoot_xbow on en_1 River Crab => MISS
DOT fire: en_1 River Crab 25->20 (-5)
en_1 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_1 River Crab hits pc_0 Soldier (Tank) 23->8 (-15) via none
en_2 River Crab acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_2 River Crab uses ab_attack_basic on pc_0 Soldier (Tank) => MISS
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_0 Soldier (Tank)
en_3 Juvenile Hellbender hits pc_0 Soldier (Tank) 8->0 (-9) via none
*** KO: pc_0 Soldier (Tank)
pc_1 Wizard (Fire) acts: ab_firebolt -> en_1 River Crab
pc_1 Wizard (Fire) hits en_1 River Crab 20->0 (-21) via fire
*** KO: en_1 River Crab
  status: en_1 River Crab gains st_burning (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_2 Cleric (Grey) moves (3,6) -> (4,5)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_2 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_2 River Crab hits pc_2 Cleric (Grey) 60->48 (-12) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_3 Juvenile Hellbender hits pc_2 Cleric (Grey) 48->37 (-11) via none
pc_1 Wizard (Fire) acts: ab_focus -> pc_1 Wizard (Fire)
  status: pc_1 Wizard (Fire) gains st_focused (dur=2)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_2 Cleric (Grey) acts: ab_attack_basic -> en_2 River Crab
pc_2 Cleric (Grey) hits en_2 River Crab 60->50 (-10) via none
en_2 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_2 River Crab hits pc_2 Cleric (Grey) 37->25 (-12) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_3 Juvenile Hellbender hits pc_2 Cleric (Grey) 25->14 (-11) via none
pc_1 Wizard (Fire) moves (2,10) -> (4,8)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_2 Cleric (Grey) acts: ab_attack_basic -> en_2 River Crab
pc_2 Cleric (Grey) uses ab_attack_basic on en_2 River Crab => MISS
en_2 River Crab acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_2 River Crab hits pc_2 Cleric (Grey) 14->2 (-12) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_2 Cleric (Grey)
en_3 Juvenile Hellbender hits pc_2 Cleric (Grey) 2->0 (-11) via none
*** KO: pc_2 Cleric (Grey)
pc_1 Wizard (Fire) moves (4,8) -> (4,5)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_2 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_2 River Crab uses ab_attack_basic on pc_1 Wizard (Fire) => MISS
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_3 Juvenile Hellbender hits pc_1 Wizard (Fire) 40->25 (-15) via none
pc_1 Wizard (Fire) acts: ab_attack_basic -> en_2 River Crab
pc_1 Wizard (Fire) hits en_2 River Crab 50->41 (-9) via none
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_2 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_2 River Crab hits pc_1 Wizard (Fire) 25->9 (-16) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_3 Juvenile Hellbender uses ab_attack_basic on pc_1 Wizard (Fire) => MISS
pc_1 Wizard (Fire) acts: ab_attack_basic -> en_2 River Crab
pc_1 Wizard (Fire) hits en_2 River Crab 41->32 (-9) via none
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
pc_3 Marksman (Crossbow) acts: ab_skirmish_step -> pc_3 Marksman (Crossbow)
en_2 River Crab acts: ab_attack_basic -> pc_1 Wizard (Fire)
en_2 River Crab hits pc_1 Wizard (Fire) 9->0 (-16) via none
*** KO: pc_1 Wizard (Fire)
en_3 Juvenile Hellbender moves (5,5) -> (2,6)
pc_3 Marksman (Crossbow) acts: ab_shoot_xbow -> en_3 Juvenile Hellbender
pc_3 Marksman (Crossbow) hits en_3 Juvenile Hellbender 60->50 (-10) via none
en_2 River Crab moves (4,4) -> (3,7)
en_3 Juvenile Hellbender moves (2,6) -> (2,10)
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_3 Juvenile Hellbender
pc_3 Marksman (Crossbow) uses ab_attack_basic on en_3 Juvenile Hellbender => MISS
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_3 Juvenile Hellbender
pc_3 Marksman (Crossbow) hits en_3 Juvenile Hellbender 50->38 (-12) via none
en_2 River Crab moves (3,7) -> (3,11)
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_3 Marksman (Crossbow)
en_3 Juvenile Hellbender hits pc_3 Marksman (Crossbow) 50->37 (-13) via none
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 32->20 (-12) via none
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 20->8 (-12) via none
en_2 River Crab acts: ab_attack_basic -> pc_3 Marksman (Crossbow)
en_2 River Crab hits pc_3 Marksman (Crossbow) 37->23 (-14) via none
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_3 Marksman (Crossbow)
en_3 Juvenile Hellbender hits pc_3 Marksman (Crossbow) 23->10 (-13) via none
pc_3 Marksman (Crossbow) acts: ab_attack_basic -> en_2 River Crab
pc_3 Marksman (Crossbow) hits en_2 River Crab 8->0 (-12) via none
*** KO: en_2 River Crab
en_3 Juvenile Hellbender acts: ab_attack_basic -> pc_3 Marksman (Crossbow)
en_3 Juvenile Hellbender hits pc_3 Marksman (Crossbow) 10->0 (-13) via none
*** KO: pc_3 Marksman (Crossbow)

=== RESULT ===
LOSE