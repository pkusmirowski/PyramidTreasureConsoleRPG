namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Test cechy: cecha + k20 ≥ trudność.</summary>
public sealed record SkillCheck(StatKind Stat, int Difficulty);

/// <summary>Skutek wyboru w zdarzeniu. Silnik zdarzeń nakłada go na bohatera i opisuje słowami.</summary>
public abstract record EventEffect;

public sealed record GoldEffect(int Amount) : EventEffect;

/// <summary>Procent maksymalnego zdrowia; ujemny = obrażenia.</summary>
public sealed record HpPercentEffect(int Percent) : EventEffect;

public sealed record ExpEffect(int Amount) : EventEffect;

public sealed record ReputationEffect(Faction Faction, int Delta) : EventEffect;

public sealed record FlagEffect(string Flag) : EventEffect;

public sealed record ClearFlagEffect(string Flag) : EventEffect;

/// <summary>Umarza dług u lichwiarza.</summary>
public sealed record ClearDebtEffect : EventEffect;

/// <summary>Noce koszmarów (nocleg leczy tylko do 90%).</summary>
public sealed record NightmaresEffect(int Nights) : EventEffect;

public sealed record PotionEffect(PotionKind Kind, int Count) : EventEffect;

/// <summary>Walka wybucha po wyborze; ekran uruchamia ją przez CombatScreen.</summary>
public sealed record FightEffect(EnemyDefinition[] Enemies) : EventEffect;

public sealed record EventChoice(
    string Text,
    IReadOnlyList<EventEffect> OnSuccess,
    string SuccessText,
    SkillCheck? Check = null,
    IReadOnlyList<EventEffect>? OnFailure = null,
    string? FailureText = null,
    int GoldCost = 0,
    Faction? RequiresFaction = null,
    int RequiredReputation = 0,
    string? RequiresFlag = null);

public sealed record GameEvent(
    string Id,
    RegionId Region,
    string Title,
    IReadOnlyList<string> Text,
    IReadOnlyList<EventChoice> Choices,
    bool OncePerGame = false,
    string? RequiresFlag = null,
    string? ForbidsFlag = null,
    bool Forced = false,
    int MinLevel = 0)
{
    public string SeenFlag => $"event:{Id}";
}
