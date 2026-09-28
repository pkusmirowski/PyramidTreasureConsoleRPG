namespace PyramidTreasureConsoleRPG.Engine;

public sealed record ChoiceAvailability(bool Available, string? Reason);

public sealed record EventResult(
    bool Success,
    int? Roll,
    string Text,
    IReadOnlyList<string> Notes,
    EnemyDefinition[]? Fight);

/// <summary>Losowanie i rozstrzyganie zdarzeń z wyborami. Efekty są nakładane tutaj i opisywane słowami.</summary>
public static class EventEngine
{
    public static GameEvent? Pick(Hero hero, RegionId region, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(rng);
        var eligible = EventCatalog.InRegion(region).Where(e => IsEligible(hero, e)).ToList();
        GameEvent? forced = eligible.Find(e => e.Forced);
        if (forced is not null)
        {
            return forced;
        }

        return eligible.Count == 0 ? null : rng.Pick(eligible);
    }

    public static bool IsEligible(Hero hero, GameEvent ev)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(ev);
        if (ev.OncePerGame && hero.HasFlag(ev.SeenFlag))
        {
            return false;
        }

        if (ev.RequiresFlag is not null && !hero.HasFlag(ev.RequiresFlag))
        {
            return false;
        }

        return ev.ForbidsFlag is null || !hero.HasFlag(ev.ForbidsFlag);
    }

    public static ChoiceAvailability Availability(Hero hero, EventChoice choice)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(choice);
        if (choice.GoldCost > 0 && hero.Gold < choice.GoldCost)
        {
            return new ChoiceAvailability(false, $"brak złota ({choice.GoldCost} g)");
        }

        if (choice.RequiresFaction is Faction faction && hero.GetReputation(faction) < choice.RequiredReputation)
        {
            return new ChoiceAvailability(false, $"wymaga reputacji {RegionCatalog.FactionName(faction)} ≥ {choice.RequiredReputation}");
        }

        if (choice.RequiresFlag is not null && !hero.HasFlag(choice.RequiresFlag))
        {
            return new ChoiceAvailability(false, "niedostępne");
        }

        return new ChoiceAvailability(true, null);
    }

    public static EventResult Resolve(Hero hero, GameEvent ev, int choiceIndex, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(ev);
        ArgumentNullException.ThrowIfNull(rng);
        if (choiceIndex < 0 || choiceIndex >= ev.Choices.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(choiceIndex));
        }

        EventChoice choice = ev.Choices[choiceIndex];
        if (!Availability(hero, choice).Available)
        {
            throw new InvalidOperationException("Ta opcja nie jest dostępna.");
        }

        var notes = new List<string>();
        if (choice.GoldCost > 0)
        {
            hero.Gold -= choice.GoldCost;
            notes.Add($"−{choice.GoldCost} złota");
        }

        bool success = true;
        int? roll = null;
        if (choice.Check is SkillCheck check)
        {
            int stat = check.Stat switch
            {
                StatKind.Strength => hero.Str,
                StatKind.Dexterity => hero.Dex,
                _ => hero.Vit,
            };
            int dice = rng.Range(1, 20);
            roll = stat + dice;
            success = roll >= check.Difficulty;
            notes.Add($"Test {StatName(check.Stat)}: {stat} + k20 ({dice}) = {roll} wobec {check.Difficulty} – {(success ? "sukces" : "porażka")}");
        }

        IReadOnlyList<EventEffect> effects = success ? choice.OnSuccess : choice.OnFailure ?? [];
        EnemyDefinition[]? fight = ApplyAll(hero, effects, notes);

        if (ev.OncePerGame)
        {
            hero.SetFlag(ev.SeenFlag);
        }

        string text = success ? choice.SuccessText : choice.FailureText ?? choice.SuccessText;
        return new EventResult(success, roll, text, notes, fight);
    }

    /// <summary>Nakłada listę skutków; zwraca walkę do rozegrania (jeśli jest) i dopisuje opisy do notatek.</summary>
    public static EnemyDefinition[]? ApplyAll(Hero hero, IReadOnlyList<EventEffect> effects, List<string> notes)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(effects);
        ArgumentNullException.ThrowIfNull(notes);
        EnemyDefinition[]? fight = null;
        foreach (EventEffect effect in effects)
        {
            string? note = Apply(hero, effect, ref fight);
            if (note is not null)
            {
                notes.Add(note);
            }
        }

        return fight;
    }

    private static string? Apply(Hero hero, EventEffect effect, ref EnemyDefinition[]? fight)
    {
        switch (effect)
        {
            case ClearFlagEffect clear:
                hero.ClearFlag(clear.Flag);
                return null;
            case ClearDebtEffect:
                if (hero.Debt <= 0)
                {
                    return null;
                }

                if (hero.HasFlag("debt:canpay") && hero.Gold >= hero.Debt)
                {
                    hero.Gold -= hero.Debt;
                }

                int cleared = hero.Debt;
                hero.Debt = 0;
                hero.ClearFlag("debt:canpay");
                return $"Dług {cleared} g przestaje istnieć";
            case NightmaresEffect nightmares:
                hero.NightmareNights = Math.Max(hero.NightmareNights, nightmares.Nights);
                return $"Koszmary przez {nightmares.Nights} noce";
            case GoldEffect gold:
                int actual = Math.Max(-hero.Gold, gold.Amount);
                hero.Gold += actual;
                return actual >= 0 ? $"+{actual} złota" : $"{actual} złota";
            case HpPercentEffect hp:
                int amount = hero.MaxHp * hp.Percent / 100;
                if (amount >= 0)
                {
                    hero.Heal(amount);
                    return $"+{amount} HP";
                }

                // Zdarzenia ranią, ale nie zabijają.
                hero.Hp = Math.Max(1, hero.Hp + amount);
                return $"{amount} HP";
            case ExpEffect exp:
                int levels = hero.AddExp(exp.Amount);
                return levels > 0 ? $"+{exp.Amount} doświadczenia, nowy poziom {hero.Level}!" : $"+{exp.Amount} doświadczenia";
            case ReputationEffect rep:
                hero.AdjustReputation(rep.Faction, rep.Delta);
                return $"Reputacja {RegionCatalog.FactionName(rep.Faction)} {(rep.Delta >= 0 ? "+" : "")}{rep.Delta}";
            case FlagEffect flag:
                hero.SetFlag(flag.Flag);
                return null;
            case PotionEffect potion:
                for (int i = 0; i < potion.Count; i++)
                {
                    hero.AddPotion(Potion.Create(potion.Kind));
                }

                return $"+{potion.Count} x {Potion.Create(potion.Kind).Name}";
            case FightEffect fightEffect:
                fight = fightEffect.Enemies;
                return null;
            default:
                return null;
        }
    }

    public static string StatName(StatKind stat) => stat switch
    {
        StatKind.Strength => "siły",
        StatKind.Dexterity => "zręczności",
        _ => "żywotności",
    };
}
