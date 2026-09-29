using System.Globalization;

// Symulacja balansu na prawdziwym CombatEngine: dla każdej klasy, regionu i puli wrogów
// liczy szansę wygranej i średnią utratę HP na poziomach, na których gracz zwykle tam trafia.
// Uruchomienie: dotnet run --project tools/BalanceSim -- [liczba prób] [cykl nowej gry+] [trudność 1-3]
int trials = args.Length > 0 ? int.Parse(args[0], CultureInfo.InvariantCulture) : 200;
int newGamePlus = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 0;
Difficulty difficulty = args.Length > 2 ? (Difficulty)int.Parse(args[2], CultureInfo.InvariantCulture) : Difficulty.Normal;
Console.WriteLine("klasa    lvl region         grupa                              | wygrane  strata  tury");
foreach (HeroClass cls in Enum.GetValues<HeroClass>())
{
    foreach (RegionDefinition region in RegionCatalog.All.Where(r => r.Encounters.Length > 0))
    {
        foreach (int level in LevelsFor(region))
        {
            for (int g = 0; g < region.Encounters.Length; g++)
            {
                EnemyDefinition[] group = region.Encounters[g];
                bool unlocked = g < Encounters.UnlockedGroups(region, level);
                int wins = 0;
                double loss = 0;
                int turns = 0;
                for (int t = 0; t < trials; t++)
                {
                    Hero hero = MakeHero(cls, level, newGamePlus, difficulty);
                    var engine = new CombatEngine(hero, group.Select(d => d.Spawn()), new SeededRandomSource(t + 1));
                    engine.Begin();
                    int startHp = hero.Hp;
                    while (engine.Status == CombatStatus.InProgress)
                    {
                        turns++;
                        if (engine.Alive.Any(e => e.Charging))
                        {
                            engine.HeroGuard();
                            continue;
                        }

                        Enemy target = engine.Alive.OrderBy(e => e.Hp).First();
                        engine.HeroAttack(level >= 8 ? AttackKind.Strong : AttackKind.Normal, target);
                    }

                    if (engine.Status == CombatStatus.Victory)
                    {
                        wins++;
                        loss += 100.0 * (startHp - hero.Hp) / hero.MaxHp;
                    }
                    else
                    {
                        loss += 100;
                    }
                }

                string names = string.Join("+", group.Select(d => d.Name.Split(' ')[0]));
                Console.WriteLine(string.Create(
                    CultureInfo.InvariantCulture,
                    $"{cls,-8} {level,3} {region.Name,-14} {names,-34} | {100 * wins / trials,6}% {loss / trials,6:0}% {turns / trials,5}{(unlocked ? "" : "  (jeszcze zablokowana)")}"));
            }
        }
    }
}

static int[] LevelsFor(RegionDefinition region)
{
    int start = region.RecommendedLevel;
    return [start, Math.Min(20, start + 2), Math.Min(20, start + 5)];
}

static Hero MakeHero(HeroClass cls, int level, int newGamePlus, Difficulty difficulty)
{
    Hero hero = Hero.Create(cls, "Sim", difficulty);
    for (int cycle = 0; cycle < newGamePlus; cycle++)
    {
        hero = Hero.NewGamePlusFrom(hero);
    }

    while (hero.Level < level)
    {
        hero.AddExp(hero.ExpToNextLevel);
    }

    // Sprzęt adekwatny do poziomu: tier 1 od 4., tier 2 od 8., tier 3 od 13. poziomu (kupowalny po drodze).
    int tier = level >= 13 ? 3 : level >= 8 ? 2 : level >= 4 ? 1 : 0;
    foreach (Item item in ItemCatalog.All.Where(i => i.Tier == tier && hero.CanEquip(i)))
    {
        hero.AddGear(item);
        hero.Equip(item);
    }

    foreach (int talentLevel in hero.PendingTalentLevels().ToList())
    {
        hero.ChooseTalent(TalentCatalog.ForClassAtLevel(hero.HeroClass, talentLevel)[0].Id);
    }

    hero.FullHeal();
    return hero;
}
