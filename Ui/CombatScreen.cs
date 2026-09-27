namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Ekran walki: czyta decyzje gracza, renderuje zdarzenia z silnika.</summary>
public sealed class CombatScreen
{
    private readonly IGameIO io;
    private readonly IRandomSource rng;

    public CombatScreen(IGameIO io, IRandomSource rng)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    public CombatStatus Run(Hero hero, IEnumerable<Enemy> enemies)
    {
        var engine = new CombatEngine(hero, enemies, rng);
        io.Clear();
        Render(engine.Begin(), engine);

        while (engine.Status == CombatStatus.InProgress)
        {
            IReadOnlyList<CombatEvent> events = HeroTurn(engine);
            io.Clear();
            Render(events, engine);
        }

        return engine.Status;
    }

    private IReadOnlyList<CombatEvent> HeroTurn(CombatEngine engine)
    {
        Hero hero = engine.Hero;
        while (true)
        {
            io.ShowCombatStatus(hero, engine.CurrentEnemy);
            IReadOnlyList<AttackOption> attacks = hero.GetAttackOptions();
            var options = attacks.Select(a => $"{a.Name} ({a.Description})").ToList();
            options.Add($"Wypij miksturę (masz: {hero.Inventory.Count})");
            options.Add(engine.CanFlee ? $"Uciekaj (szansa {hero.FleeChance}%)" : "Ucieczka niemożliwa – to boss");
            int choice = io.Menu("Twoja tura:", options.ToArray());

            if (choice <= attacks.Count)
            {
                return engine.HeroAttack(attacks[choice - 1].Kind);
            }

            if (choice == attacks.Count + 1)
            {
                PotionKind? kind = InventoryScreen.ChoosePotion(io, hero);
                if (kind.HasValue)
                {
                    return engine.HeroDrinkPotion(kind.Value);
                }

                io.Clear();
                continue;
            }

            if (!engine.CanFlee)
            {
                io.Clear();
                io.ShowError("Nie ma dokąd uciekać. Musisz walczyć.");
                continue;
            }

            return engine.HeroFlee();
        }
    }

    private void Render(IReadOnlyList<CombatEvent> events, CombatEngine engine)
    {
        foreach (CombatEvent e in events)
        {
            switch (e)
            {
                case FightStartedEvent started:
                    io.WriteLine($"Spotkałeś przeciwnika: {started.Enemy.Name} ({started.Enemy.Hp} HP, {started.Enemy.MinDmg}-{started.Enemy.MaxDmg} obrażeń). Przygotuj się do walki!", ConsoleColor.Magenta);
                    io.WriteLine(started.HeroActsFirst ? "Jesteś szybszy – atakujesz pierwszy." : $"{started.Enemy.Name} jest szybszy i atakuje pierwszy!", ConsoleColor.DarkGray);
                    io.Pause(1200);
                    break;
                case StrikeEvent strike:
                    RenderStrike(strike);
                    break;
                case EnemyAttackEvent attack:
                    if (attack.Result.Hit)
                    {
                        io.ShowError($"{attack.Enemy.Name} atakuje! Otrzymujesz {attack.Result.Damage} obrażeń. Masz {engine.Hero.Hp}/{engine.Hero.MaxHp} HP.");
                    }
                    else
                    {
                        io.ShowInfo($"{attack.Enemy.Name} atakuje – unikasz ciosu!");
                    }

                    io.Pause(600);
                    break;
                case PotionDrunkEvent potion:
                    io.ShowSuccess($"Wypiłeś miksturę i odzyskałeś {potion.Healed} HP. Masz teraz {engine.Hero.Hp}/{engine.Hero.MaxHp} HP.");
                    break;
                case FleeAttemptEvent flee:
                    if (flee.Success)
                    {
                        io.ShowInfo("Udało ci się uciec! Wracasz do miasta bez łupów.");
                    }
                    else
                    {
                        io.ShowError($"{flee.Enemy.Name} odcina ci drogę ucieczki! Tracisz turę.");
                    }

                    break;
                case EnemyDefeatedEvent defeated:
                    RenderReward(defeated, engine.Hero);
                    break;
                case HeroDefeatedEvent:
                    io.ShowError("Padasz na ziemię. Ciemność.");
                    break;
                case VictoryEvent:
                    io.ShowSuccess("Wszyscy przeciwnicy pokonani. Wracasz do miasta.");
                    break;
            }
        }
    }

    private void RenderStrike(StrikeEvent e)
    {
        Strike strike = e.Strike;
        if (!strike.Hit)
        {
            io.ShowInfo($"{strike.Label}: pudło!");
            return;
        }

        if (strike.Critical)
        {
            io.WriteLine($"{strike.Label}: {Dialogues.CritDescription(rng)} ({strike.Damage} obrażeń)", ConsoleColor.Blue);
        }
        else
        {
            io.ShowSuccess($"{strike.Label}: trafienie za {strike.Damage} obrażeń.");
        }

        io.WriteLine(e.Enemy.IsAlive ? $"Przeciwnikowi zostało {e.Enemy.Hp} HP." : Dialogues.KillDescription(e.Enemy, rng), e.Enemy.IsAlive ? ConsoleColor.Gray : ConsoleColor.DarkRed);
    }

    private void RenderReward(EnemyDefeatedEvent e, Hero hero)
    {
        io.ShowSuccess($"Pokonałeś: {e.Enemy.Name}!");
        io.WriteLine($"Zdobyłeś {e.Gold} sztuk złota (masz {hero.Gold}).", ConsoleColor.DarkYellow);
        if (e.Exp > 0 && (e.LevelsGained > 0 || !hero.IsMaxLevel))
        {
            io.WriteLine($"Zyskałeś {e.Exp} punktów doświadczenia ({hero.Exp}/{hero.ExpToNextLevel}).", ConsoleColor.DarkCyan);
        }

        for (int i = 1; i <= e.LevelsGained; i++)
        {
            int level = e.NewLevel - e.LevelsGained + i;
            io.WriteLine(level >= CombatMath.MaxLevel
                ? $"Gratulacje! Zdobyłeś maksymalny poziom {level}. Pogadaj z barmanem o karawanie."
                : $"Gratulacje! Zdobyłeś poziom {level}!", ConsoleColor.DarkYellow);
        }

        io.PressAnyKey();
    }
}
