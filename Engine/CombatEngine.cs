namespace PyramidTreasureConsoleRPG;

public enum CombatOutcome
{
    Victory,
    Defeat,
    Fled,
}

/// <summary>Pętla walki: inicjatywa, tury, mikstury, ucieczka, nagrody.</summary>
public static class Combat
{
    public static CombatOutcome Run(Hero hero, List<Enemy> enemies)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(enemies);

        foreach (Enemy enemy in enemies)
        {
            GameIO.Clear();
            GameIO.WriteLine($"Spotkałeś przeciwnika: {enemy.Name} ({enemy.Hp} HP, {enemy.MinDmg}-{enemy.MaxDmg} obrażeń). Przygotuj się do walki!", ConsoleColor.Magenta);
            bool heroFirst = Rng.Chance(Math.Clamp(50 + hero.Dex - enemy.Agility, 20, 80));
            GameIO.WriteLine(heroFirst ? "Jesteś szybszy – atakujesz pierwszy." : $"{enemy.Name} jest szybszy i atakuje pierwszy!", ConsoleColor.DarkGray);
            GameIO.Pause(1200);

            while (enemy.IsAlive && hero.IsAlive)
            {
                if (heroFirst)
                {
                    if (HeroTurn(hero, enemy) == TurnResult.Fled)
                    {
                        return CombatOutcome.Fled;
                    }

                    if (enemy.IsAlive)
                    {
                        EnemyTurn(hero, enemy);
                    }
                }
                else
                {
                    EnemyTurn(hero, enemy);
                    if (hero.IsAlive && HeroTurn(hero, enemy) == TurnResult.Fled)
                    {
                        return CombatOutcome.Fled;
                    }
                }
            }

            if (!hero.IsAlive)
            {
                return CombatOutcome.Defeat;
            }

            Reward(hero, enemy);
        }

        return CombatOutcome.Victory;
    }

    private enum TurnResult
    {
        Continue,
        Fled,
    }

    private static void ShowStatus(Hero hero, Enemy enemy)
    {
        GameIO.WriteLine();
        GameIO.Write($"{hero.Name}: {hero.Hp}/{hero.MaxHp} HP", ConsoleColor.Green);
        GameIO.Write("   vs   ", ConsoleColor.DarkGray);
        GameIO.WriteLine($"{enemy.Name}: {enemy.Hp}/{enemy.MaxHp} HP", ConsoleColor.Red);
    }

    private static TurnResult HeroTurn(Hero hero, Enemy enemy)
    {
        while (true)
        {
            ShowStatus(hero, enemy);
            var attacks = hero.AttackOptions;
            var options = attacks.Select(a => $"{a.Name} ({a.Description})").ToList();
            options.Add($"Wypij miksturę (masz: {hero.Inventory.Count})");
            options.Add(enemy.IsBoss ? "Ucieczka niemożliwa – to boss" : $"Uciekaj (szansa {hero.FleeChance}%)");
            GameIO.Menu("Twoja tura:", options.ToArray());
            int choice = GameIO.ReadMenuChoice(options.Count);
            GameIO.Clear();

            if (choice <= attacks.Count)
            {
                AttackResult result = hero.Attack(attacks[choice - 1].Kind, enemy);
                Report(result, enemy);
                return TurnResult.Continue;
            }

            if (choice == attacks.Count + 1)
            {
                if (Inventory.DrinkMenu(hero))
                {
                    return TurnResult.Continue;
                }

                continue;
            }

            if (enemy.IsBoss)
            {
                GameIO.Error("Nie ma dokąd uciekać. Musisz walczyć.");
                continue;
            }

            if (Rng.Chance(hero.FleeChance))
            {
                GameIO.Info("Udało ci się uciec! Wracasz do miasta bez łupów.");
                return TurnResult.Fled;
            }

            GameIO.Error($"{enemy.Name} odcina ci drogę ucieczki! Tracisz turę.");
            return TurnResult.Continue;
        }
    }

    private static void Report(AttackResult result, Enemy enemy)
    {
        foreach (Strike strike in result.Strikes)
        {
            if (!strike.Hit)
            {
                GameIO.Info($"{strike.Label}: pudło!");
                continue;
            }

            if (strike.Critical)
            {
                GameIO.WriteLine($"{strike.Label}: {Dialogues.CritDescription()} ({strike.Damage} obrażeń)", ConsoleColor.Blue);
            }
            else
            {
                GameIO.Success($"{strike.Label}: trafienie za {strike.Damage} obrażeń.");
            }
        }

        if (result.AnyHit)
        {
            GameIO.WriteLine(enemy.IsAlive ? $"Przeciwnikowi zostało {enemy.Hp} HP." : Dialogues.KillDescription(enemy), enemy.IsAlive ? ConsoleColor.Gray : ConsoleColor.DarkRed);
        }
    }

    private static void EnemyTurn(Hero hero, Enemy enemy)
    {
        EnemyAttackResult result = enemy.Attack(hero);
        if (!result.Hit)
        {
            GameIO.Info($"{enemy.Name} atakuje – unikasz ciosu!");
        }
        else
        {
            GameIO.Error($"{enemy.Name} atakuje! Otrzymujesz {result.Damage} obrażeń. Masz {hero.Hp}/{hero.MaxHp} HP.");
        }

        GameIO.Pause(600);
    }

    private static void Reward(Hero hero, Enemy enemy)
    {
        GameIO.Success($"Pokonałeś: {enemy.Name}!");
        hero.Gold += enemy.Gold;
        GameIO.WriteLine($"Zdobyłeś {enemy.Gold} sztuk złota (masz {hero.Gold}).", ConsoleColor.DarkYellow);

        if (enemy.Exp > 0 && !hero.IsMaxLevel)
        {
            int before = hero.Level;
            int gained = hero.AddExp(enemy.Exp);
            GameIO.WriteLine($"Zyskałeś {enemy.Exp} punktów doświadczenia ({hero.Exp}/{hero.ExpToNextLevel}).", ConsoleColor.DarkCyan);
            for (int i = 1; i <= gained; i++)
            {
                int newLevel = before + i;
                GameIO.WriteLine(newLevel >= CombatMath.MaxLevel
                    ? $"Gratulacje! Zdobyłeś maksymalny poziom {newLevel}. Pogadaj z barmanem o karawanie."
                    : $"Gratulacje! Zdobyłeś poziom {newLevel}!", ConsoleColor.DarkYellow);
            }
        }

        GameIO.PressAnyKey();
        GameIO.Clear();
    }
}
