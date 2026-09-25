namespace PyramidTreasureConsoleRPG;

/// <summary>Zrównoważone statystyki, najwyższa szansa trafienia i krytyk. Atak specjalny: zatrute ostrze.</summary>
public sealed class Assassin : Hero
{
    public Assassin(string name)
        : base(name)
    {
    }

    public override HeroClass Class => HeroClass.Assassin;

    public override string ClassName => "Asasyn";

    protected override int StartVit => 12;

    protected override int StartStr => 2;

    protected override int StartDex => 2;

    protected override int HpPerVit => 13;

    protected override int BaseMinDmg => 3;

    protected override int BaseMaxDmg => 6;

    protected override int StrWeight => 1;

    protected override int DexWeight => 1;

    protected override double BaseHitChance => 60;

    protected override double BaseCritChance => 30;

    protected override int VitPerLevel => 2;

    protected override int StrPerLevel => 2;

    protected override int DexPerLevel => 3;

    public override string NormalAttackName => "Atak sztyletem";

    public override string StrongAttackName => "Atak z ukrycia";

    public override string SpecialAttackName => "Zatrute ostrze";

    protected override string SpecialAttackDescription => $"{MinDmg * 12 / 10}-{MaxDmg * 12 / 10} obrażeń, trafienie {CombatMath.ClampChance(HitChance - 10):0}%, krytyk {Math.Min(75, CritChance * 3):0}%";

    protected override int ArmorFormula() => Dex / 2;

    protected override int EvasionFormula() => Dex / 3;

    protected override double ExtraHitChance() => Dex / 4.0;

    protected override AttackResult PerformSpecialAttack(Enemy enemy)
        => SingleStrike(enemy, HitChance - 10, 1.2, Math.Min(75, CritChance * 3), SpecialAttackName);
}
