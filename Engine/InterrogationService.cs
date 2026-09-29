namespace PyramidTreasureConsoleRPG.Engine;

public enum PrisonerChoice
{
    Execute = 1,
    Release = 2,
    Interrogate = 3,
}

public sealed record PrisonerResult(string Text, IReadOnlyList<string> Notes);

/// <summary>Po walce z ludźmi jeden z nich czasem żyje. Egzekucja, łaska albo przesłuchanie – każde ma cenę.</summary>
public static class InterrogationService
{
    public const int PrisonerChancePercent = 40;

    public static bool PrisonerSurvives(IReadOnlyList<EnemyDefinition> killed, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(killed);
        ArgumentNullException.ThrowIfNull(rng);
        return killed.Any(k => k.Kind == EnemyKind.Human && !k.IsBoss) && rng.Chance(PrisonerChancePercent);
    }

    public static PrisonerResult Resolve(Hero hero, PrisonerChoice choice, RegionId region, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(rng);
        var notes = new List<string>();
        switch (choice)
        {
            case PrisonerChoice.Execute:
                hero.Stats.RecordChoice(-1);
                hero.AdjustReputation(Faction.Underworld, 3);
                hero.AdjustReputation(Faction.Town, -3);
                notes.Add("Reputacja Podziemie +3, Miasto −3");
                return new PrisonerResult("Jeden ruch ostrzem. Nie prosi o litość, bo wie, że to nic nie da.", notes);
            case PrisonerChoice.Release:
                hero.Stats.RecordChoice(1);
                hero.AdjustReputation(Faction.Town, 5);
                notes.Add("Reputacja Miasto +5");
                return new PrisonerResult("Odchodzi kulejąc, nie oglądając się. Ktoś w mieście o tym usłyszy.", notes);
            default:
                hero.Stats.RecordChoice(-1);
                int dice = rng.Range(1, 20);
                int roll = hero.Str + dice;
                hero.AdjustReputation(Faction.Town, -8);
                hero.AdjustReputation(Faction.Underworld, 5);
                hero.NightmareNights = 3;
                notes.Add($"Test siły: {hero.Str} + k20 ({dice}) = {roll} wobec 12 – {(roll >= 12 ? "sukces" : "porażka")}");
                notes.Add("Reputacja Miasto −8, Podziemie +5; koszmary przez 3 noce");
                if (roll < 12)
                {
                    return new PrisonerResult("Mdleje, zanim cokolwiek powie. Zostaje ci krew na rękach i cisza.", notes);
                }

                hero.SetFlag($"intel:{region}");
                int levels = hero.AddExp(200);
                notes.Add(levels > 0 ? $"+200 doświadczenia, nowy poziom {hero.Level}!" : "+200 doświadczenia");
                return new PrisonerResult("Łamie się przy trzecim palcu. Mówi o kryjówce w tej okolicy: następna wyprawa trafi na coś ciekawego, nie na zasadzkę.", notes);
        }
    }
}
