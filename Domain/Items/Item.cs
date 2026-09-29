namespace PyramidTreasureConsoleRPG.Domain;

public enum ItemSlot
{
    Weapon,
    Armor,
    Trinket,
}

public enum ItemId
{
    // broń – Wojownik
    MercenaryAxe,
    TemplarSword,
    PharaohScimitar,

    // broń – Łucznik
    HuntingBow,
    RecurveBow,
    FalconBow,

    // broń – Asasyn
    PoisonedDagger,
    TwinKindjals,
    AnubisFangs,

    // pancerze
    LeatherJerkin,
    DesertCloak,
    ChainMail,
    BrotherhoodRobe,
    TemplarPlate,

    // amulety
    GreedAmulet,
    FalconFeather,
    EyeOfRa,
    Scarab,
    WolfBone,
    HasanRing,
}

public enum TrinketEffect
{
    /// <summary>Procent więcej złota z pokonanych wrogów.</summary>
    GoldFind,
    Evasion,
    Crit,
    MaxHp,
    FleeChance,
    /// <summary>Procent zniżki w sklepach.</summary>
    ShopDiscount,
}

/// <summary>Przedmiot wyposażenia. Mikstury są osobnym typem (<see cref="Potion"/>).</summary>
public abstract record Item(ItemId Id, string Name, string Description, int Price, ItemSlot Slot, int Tier)
{
    /// <summary>Cena skupu.</summary>
    public int SellPrice => Math.Max(1, Price * 40 / 100);
}

public sealed record Weapon(
    ItemId Id,
    string Name,
    string Description,
    int Price,
    int Tier,
    HeroClass ForClass,
    int MinDmgBonus,
    int MaxDmgBonus,
    double HitBonus = 0,
    double CritBonus = 0) : Item(Id, Name, Description, Price, ItemSlot.Weapon, Tier);

public sealed record Armor(
    ItemId Id,
    string Name,
    string Description,
    int Price,
    int Tier,
    int ArmorBonus,
    int EvasionBonus = 0) : Item(Id, Name, Description, Price, ItemSlot.Armor, Tier);

public sealed record Trinket(
    ItemId Id,
    string Name,
    string Description,
    int Price,
    int Tier,
    TrinketEffect Effect,
    int Power) : Item(Id, Name, Description, Price, ItemSlot.Trinket, Tier);

/// <summary>Wpis tabeli łupów: przedmiot albo mikstura, z szansą w procentach.</summary>
public sealed record LootEntry(int ChancePercent, ItemId? Item = null, PotionKind? Potion = null);
