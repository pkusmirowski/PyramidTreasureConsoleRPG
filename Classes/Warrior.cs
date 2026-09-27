namespace PyramidTreasureConsoleRPG;

/// <summary>Dużo HP, wysokie obrażenia z siły, pancerz rośnie z poziomem. Atak specjalny: trzy cięcia.</summary>
public sealed class Warrior : Hero
{
    public Warrior(string name)
        : base(name)
    {
    }

    public override HeroClass HeroClass => HeroClass.Warrior;

    public override string ClassName => "Wojownik";

    protected override int StartVit => 15;

    protected override int StartStr => 4;

    protected override int StartDex => 2;

    protected override int HpPerVit => 14;

    protected override int BaseMinDmg => 2;

    protected override int BaseMaxDmg => 7;

    protected override int StrWeight => 2;

    protected override int DexWeight => 0;

    protected override double BaseHitChance => 60;

    protected override double BaseCritChance => 20;

    protected override int VitPerLevel => 2;

    protected override int StrPerLevel => 3;

    protected override int DexPerLevel => 1;

    public override string NormalAttackName => "Normalny atak";

    public override string StrongAttackName => "Silny atak";

    public override string SpecialAttackName => "Trzystronne cięcie";

    protected override string SpecialAttackDescription => $"3 cięcia po {MinDmg * 6 / 10}-{MaxDmg * 6 / 10} obrażeń, każde z trafieniem {CombatMath.ClampChance(HitChance - 15):0}%";

    protected override int ArmorFormula() => Dex + Level;

    protected override int EvasionFormula() => Dex / 4;

    protected override AttackResult PerformSpecialAttack(Enemy enemy)
    {
        var result = new AttackResult();
        for (int i = 1; i <= 3; i++)
        {
            result.Strikes.Add(RollStrike(enemy, HitChance - 15, 0.6, CritChance, $"Cięcie {i}"));
        }

        return result;
    }
}
