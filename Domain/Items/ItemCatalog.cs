namespace PyramidTreasureConsoleRPG.Domain;

public static class ItemCatalog
{
    private static readonly Item[] Items =
    [
        new Weapon(ItemId.MercenaryAxe, "Topór najemnika", "Ciężki, wyszczerbiony, skuteczny.", 120, 1, HeroClass.Warrior, 2, 4),
        new Weapon(ItemId.TemplarSword, "Miecz templariusza", "Zdjęty z trupa. Ostrze pamięta.", 450, 2, HeroClass.Warrior, 5, 9, HitBonus: 3),
        new Weapon(ItemId.PharaohScimitar, "Bułat faraona", "Złoto na rękojeści nie jest złotem.", 1400, 3, HeroClass.Warrior, 9, 15, HitBonus: 3, CritBonus: 5),
        new Weapon(ItemId.HuntingBow, "Łuk myśliwski", "Prosty, celny, tani.", 110, 1, HeroClass.Archer, 2, 3, HitBonus: 3),
        new Weapon(ItemId.RecurveBow, "Łuk refleksyjny", "Koczownicy robią takie z rogu i ścięgien.", 420, 2, HeroClass.Archer, 4, 7, HitBonus: 5),
        new Weapon(ItemId.FalconBow, "Łuk Sokoła", "Cięciwa śpiewa, zanim strzała trafi.", 1350, 3, HeroClass.Archer, 7, 12, HitBonus: 5, CritBonus: 8),
        new Weapon(ItemId.PoisonedDagger, "Zatruty sztylet", "Rowek na ostrzu nie jest ozdobą.", 115, 1, HeroClass.Assassin, 2, 3, CritBonus: 3),
        new Weapon(ItemId.TwinKindjals, "Bliźniacze kindżały", "Jeden w gardło, drugi między żebra.", 430, 2, HeroClass.Assassin, 4, 7, CritBonus: 6),
        new Weapon(ItemId.AnubisFangs, "Kły Anubisa", "Wyrwane bogu. Wciąż ciepłe.", 1400, 3, HeroClass.Assassin, 7, 12, HitBonus: 5, CritBonus: 10),
        new Armor(ItemId.LeatherJerkin, "Skórzany kaftan", "Zatrzymuje nóż, jeśli nie jest długi.", 100, 1, ArmorBonus: 6),
        new Armor(ItemId.DesertCloak, "Płaszcz pustynny", "Lekki. Trudno w niego trafić, łatwo przebić.", 130, 1, ArmorBonus: 3, EvasionBonus: 4),
        new Armor(ItemId.ChainMail, "Kolczuga", "Ciężka, głośna, ratuje życie.", 400, 2, ArmorBonus: 14, EvasionBonus: -2),
        new Armor(ItemId.BrotherhoodRobe, "Szata Bractwa", "Uszyta z całunów. Śmierdzi lotosem.", 500, 2, ArmorBonus: 8, EvasionBonus: 6),
        new Armor(ItemId.TemplarPlate, "Zbroja templariusza", "Krzyż na napierśniku wyskrobano nożem.", 1300, 3, ArmorBonus: 25, EvasionBonus: -4),
        new Trinket(ItemId.GreedAmulet, "Amulet chciwości", "Więcej złota z każdego trupa.", 300, 2, TrinketEffect.GoldFind, 20),
        new Trinket(ItemId.FalconFeather, "Pióro Sokoła", "Ciosy przelatują obok.", 350, 2, TrinketEffect.Evasion, 5),
        new Trinket(ItemId.EyeOfRa, "Oko Ra", "Widzi, gdzie boli najbardziej.", 700, 3, TrinketEffect.Crit, 6),
        new Trinket(ItemId.Scarab, "Skarabeusz", "Nosi w sobie cudze życie.", 600, 3, TrinketEffect.MaxHp, 60),
        new Trinket(ItemId.WolfBone, "Kość wilka", "Wilki wiedzą, kiedy uciekać.", 200, 1, TrinketEffect.FleeChance, 15),
        new Trinket(ItemId.HasanRing, "Pierścień Hasana", "Handlarze poznają go i spuszczają z ceny.", 250, 1, TrinketEffect.ShopDiscount, 10),
    ];

    public static IReadOnlyList<Item> All => Items;

    public static Item Get(ItemId id) => Items.First(i => i.Id == id);

    public static string SlotName(ItemSlot slot) => slot switch
    {
        ItemSlot.Weapon => "broń",
        ItemSlot.Armor => "pancerz",
        ItemSlot.Trinket => "amulet",
        _ => slot.ToString(),
    };

    /// <summary>Krótki opis bonusów do menu.</summary>
    public static string Stats(Item item) => item switch
    {
        Weapon w => $"+{w.MinDmgBonus}/+{w.MaxDmgBonus} obr." + (w.HitBonus > 0 ? $", +{w.HitBonus:0} traf." : "") + (w.CritBonus > 0 ? $", +{w.CritBonus:0} kryt." : "") + $", {HeroClasses.Get(w.ForClass).Name}",
        Armor a => $"+{a.ArmorBonus} panc." + (a.EvasionBonus != 0 ? $", {a.EvasionBonus:+0;-0} uniki" : ""),
        Trinket t => t.Effect switch
        {
            TrinketEffect.GoldFind => $"+{t.Power}% złota z walk",
            TrinketEffect.Evasion => $"+{t.Power} uniki",
            TrinketEffect.Crit => $"+{t.Power} kryt.",
            TrinketEffect.MaxHp => $"+{t.Power} maks. HP",
            TrinketEffect.FleeChance => $"+{t.Power} ucieczka",
            TrinketEffect.ShopDiscount => $"{t.Power}% taniej w sklepach",
            _ => t.Effect.ToString(),
        },
        _ => string.Empty,
    };
}
