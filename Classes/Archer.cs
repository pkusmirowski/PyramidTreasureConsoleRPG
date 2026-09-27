namespace PyramidTreasureConsoleRPG;

/// <summary>Obrażenia ze zręczności, najwyższe uniki, słabszy pancerz. Atak specjalny: podwójny strzał.</summary>
public sealed class Archer : Hero
{
    public Archer(string name)
        : base(name)
    {
    }

    public override HeroClass HeroClass => HeroClass.Archer;

    public override string ClassName => "Łucznik";

    protected override int StartVit => 11;

    protected override int StartStr => 1;

    protected override int StartDex => 3;

    protected override int HpPerVit => 12;

    protected override int BaseMinDmg => 2;

    protected override int BaseMaxDmg => 5;

    protected override int StrWeight => 0;

    protected override int DexWeight => 2;

    protected override double BaseHitChance => 55;

    protected override double BaseCritChance => 25;

    protected override int VitPerLevel => 2;

    protected override int StrPerLevel => 1;

    protected override int DexPerLevel => 3;

    public override string NormalAttackName => "Strzał z łuku";

    public override string StrongAttackName => "Skupiony strzał";

    public override string SpecialAttackName => "Podwójny strzał";

    protected override string SpecialAttackDescription => $"2 strzały po {MinDmg * 8 / 10}-{MaxDmg * 8 / 10} obrażeń, każdy z trafieniem {HitChance:0}%";

    protected override int ArmorFormula() => Dex / 3;

    protected override int EvasionFormula() => Dex / 2;

    protected override AttackResult PerformSpecialAttack(Enemy enemy)
    {
        var result = new AttackResult();
        result.Strikes.Add(RollStrike(enemy, HitChance, 0.8, CritChance, "Pierwsza strzała"));
        result.Strikes.Add(RollStrike(enemy, HitChance, 0.8, CritChance, "Druga strzała"));
        return result;
    }
}
