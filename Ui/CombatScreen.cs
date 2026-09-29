namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Ekran walki grupowej: cel, atak, obrona, mikstura, ucieczka; renderuje zdarzenia z silnika.</summary>
public sealed class CombatScreen(IGameIO io, IRandomSource rng)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));

    /// <summary>Prowadzi walkę do końca i aktualizuje postęp zadań za pokonanych wrogów.</summary>
    public CombatStatus Run(Hero hero, IEnumerable<Enemy> enemies)
    {
        ArgumentNullException.ThrowIfNull(hero);
        var engine = new CombatEngine(hero, enemies, rng);
        io.Clear();
        Render(engine.Begin(), engine);

        while (engine.Status == CombatStatus.InProgress)
        {
            IReadOnlyList<CombatEvent> events = HeroTurn(engine);
            io.Clear();
            Render(events, engine);
        }

        foreach (EnemyDefinition definition in engine.Killed)
        {
            foreach (QuestUpdate update in QuestEngine.OnEnemyKilled(hero, definition))
            {
                io.WriteLine(update.JustCompleted
                    ? $"Zadanie \"{update.Quest.Title}\": cel wykonany! Wróć do zleceniodawcy ({RegionCatalog.GiverName(update.Quest.Giver)})."
                    : $"Zadanie \"{update.Quest.Title}\": {update.Progress}/{update.Target}.", ConsoleColor.Cyan);
            }
        }

        if (hero.PendingTalentLevels().Count > 0)
        {
            io.WriteLine("Masz nowy talent do wyboru – zajrzyj do menu regionu.", ConsoleColor.Magenta);
        }

        if (engine.Killed.Count > 0)
        {
            io.PressAnyKey();
        }

        return engine.Status;
    }

    private IReadOnlyList<CombatEvent> HeroTurn(CombatEngine engine)
    {
        Hero hero = engine.Hero;
        while (true)
        {
            io.ShowCombatStatus(hero, engine.Alive);
            IReadOnlyList<AttackOption> attacks = hero.GetAttackOptions();
            var options = attacks.Select(a => $"{a.Name} ({a.Description})").ToList();
            options.Add("Obrona (połowa obrażeń, następny cios krytyczny)");
            options.Add($"Wypij miksturę (masz: {hero.Inventory.Count})");
            options.Add(engine.CanFlee ? $"Uciekaj (szansa {hero.FleeChance}%)" : "Ucieczka niemożliwa – boss");
            int choice = io.Menu("Twoja tura:", [.. options]);

            if (choice <= attacks.Count)
            {
                Enemy? target = ChooseTarget(engine);
                if (target is null)
                {
                    io.Clear();
                    continue;
                }

                return engine.HeroAttack(attacks[choice - 1].Kind, target);
            }

            if (choice == attacks.Count + 1)
            {
                return engine.HeroGuard();
            }

            if (choice == attacks.Count + 2)
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

    private Enemy? ChooseTarget(CombatEngine engine)
    {
        IReadOnlyList<Enemy> alive = engine.Alive;
        if (alive.Count == 1)
        {
            return alive[0];
        }

        var labels = alive.Select(e => $"{e.Name} – {e.Hp}/{e.MaxHp} HP" + (e.Statuses.All.Count > 0 ? $" [{e.Statuses.Describe()}]" : "")).Append("Wróć").ToArray();
        int choice = io.Menu("Cel:", labels);
        return choice > alive.Count ? null : alive[choice - 1];
    }

    private void Render(IReadOnlyList<CombatEvent> events, CombatEngine engine)
    {
        foreach (CombatEvent e in events)
        {
            switch (e)
            {
                case FightStartedEvent started:
                    io.ShowArt(ArtCatalog.ForEnemy(started.Enemies.OrderByDescending(x => x.IsBoss).ThenByDescending(x => x.MaxHp).First().Definition));
                    io.WriteLine("Przeciwnicy: " + string.Join(", ", started.Enemies.Select(x => $"{x.Name} ({x.Hp} HP, {x.MinDmg}-{x.MaxDmg} obr.)")), ConsoleColor.Magenta);
                    io.WriteLine(started.HeroActsFirst ? "Jesteś szybszy – atakujesz pierwszy." : "Wrogowie są szybsi i atakują pierwsi!", ConsoleColor.DarkGray);
                    io.Pause(1200);
                    break;
                case StrikeEvent strike:
                    RenderStrike(strike);
                    break;
                case StatusAppliedEvent applied:
                    io.WriteLine(applied.OnHero ? $"Dostajesz status: {StatusEffect.Name(applied.Kind)} ({applied.Turns} tur)." : $"{applied.TargetName}: {StatusEffect.Name(applied.Kind)} ({applied.Turns} tur).", ConsoleColor.DarkMagenta);
                    break;
                case StatusTickEvent tick:
                    if (tick.Damage > 0)
                    {
                        io.WriteLine(tick.OnHero ? $"{StatusEffect.Name(tick.Kind)}: tracisz {tick.Damage} HP." : $"{tick.TargetName}: {StatusEffect.Name(tick.Kind)} zadaje {tick.Damage}.", ConsoleColor.DarkMagenta);
                    }
                    else if (tick.Expired && tick.Kind != StatusKind.Guard)
                    {
                        io.WriteLine($"{(tick.OnHero ? "Twój status" : tick.TargetName + ":")} {StatusEffect.Name(tick.Kind)} mija.", ConsoleColor.DarkGray);
                    }

                    break;
                case StunnedEvent stunned:
                    io.WriteLine(stunned.OnHero ? "Jesteś ogłuszony – tracisz turę!" : $"{stunned.TargetName} jest ogłuszony i traci turę.", ConsoleColor.DarkMagenta);
                    break;
                case GuardEvent:
                    io.ShowInfo("Zasłaniasz się. Następny cios przeciwnika zada połowę obrażeń, a twój będzie krytyczny.");
                    break;
                case EnemyAbilityEvent ability:
                    io.WriteLine(ability.Text, ConsoleColor.Magenta);
                    break;
                case EnemyFledEvent fled:
                    io.ShowInfo($"{fled.Enemy.Name} rzuca się do ucieczki i znika w tłumie. Łup przepada, ale zyskujesz {fled.Exp} punktów doświadczenia.");
                    if (fled.LevelsGained > 0)
                    {
                        io.ShowSuccess($"Nowy poziom: {fled.NewLevel}!");
                    }

                    break;
                case SummonEvent summon:
                    io.WriteLine($"{summon.Summoner.Name} wzywa: {string.Join(", ", summon.Summoned.Select(s => s.Name))}!", ConsoleColor.Magenta);
                    break;
                case ResurrectEvent resurrect:
                    io.WriteLine($"{resurrect.Enemy.Name} rozsypuje się w pył... i zbiera z powrotem! Wstaje z {resurrect.Enemy.Hp} HP.", ConsoleColor.Magenta);
                    break;
                case EnemyAttackEvent attack:
                    if (attack.Result.Hit)
                    {
                        io.ShowError($"{attack.Enemy.Name} atakuje! Otrzymujesz {attack.Result.Damage} obrażeń. Masz {attack.HeroHp}/{engine.Hero.MaxHp} HP.");
                    }
                    else
                    {
                        io.ShowInfo($"{attack.Enemy.Name} atakuje – unikasz ciosu!");
                    }

                    io.Pause(500);
                    break;
                case PotionDrunkEvent potion:
                    io.ShowSuccess(InventoryScreen.DrinkMessage(engine.Hero, potion.Kind, potion.Healed, potion.HeroHp));
                    break;
                case FleeAttemptEvent flee:
                    if (flee.Success)
                    {
                        io.ShowInfo("Udało ci się uciec! Wracasz bez łupów.");
                    }
                    else
                    {
                        io.ShowError("Odcinają ci drogę ucieczki! Tracisz turę.");
                    }

                    break;
                case EnemyDefeatedEvent defeated:
                    RenderReward(defeated, engine.Hero);
                    break;
                case LootEvent loot:
                    if (loot.Potion is not null)
                    {
                        io.WriteLine($"Łup: {loot.Potion.Name}.", ConsoleColor.DarkYellow);
                    }
                    else if (loot.Item is not null)
                    {
                        io.WriteLine(loot.Kept ? $"Łup: {loot.Item.Name} ({ItemCatalog.Stats(loot.Item)}) – do torby." : $"Łup: {loot.Item.Name}, ale torba jest pełna. Przepada.", loot.Kept ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray);
                    }

                    break;
                case HeroDefeatedEvent:
                    io.ShowError("Padasz na ziemię. Ciemność.");
                    break;
                case VictoryEvent:
                    io.ShowSuccess("Pole walki jest twoje.");
                    break;
            }
        }
    }

    private void RenderStrike(StrikeEvent e)
    {
        Strike strike = e.Strike;
        if (!strike.Hit)
        {
            io.ShowInfo($"{strike.Label} ({e.Enemy.Name}): pudło!");
            return;
        }

        if (strike.Critical)
        {
            io.WriteLine($"{strike.Label} ({e.Enemy.Name}): {Dialogues.CritDescription(rng)} ({strike.Damage} obrażeń)", ConsoleColor.Blue);
        }
        else
        {
            io.ShowSuccess($"{strike.Label} ({e.Enemy.Name}): trafienie za {strike.Damage} obrażeń.");
        }

        io.WriteLine(e.Enemy.IsAlive ? $"{e.Enemy.Name}: zostało {e.Enemy.Hp} HP." : Dialogues.KillDescription(e.Enemy, rng), e.Enemy.IsAlive ? ConsoleColor.Gray : ConsoleColor.DarkRed);
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

        io.Pause(900);
    }
}
