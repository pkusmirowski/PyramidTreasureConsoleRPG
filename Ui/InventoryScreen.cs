namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Sakwa poza walką: mikstury i wyposażenie. Wybór mikstury jest wspólny z ekranem walki.</summary>
public sealed class InventoryScreen(IGameIO io)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header("Sakwa");
            Show(hero);
            int choice = io.Menu("Co chcesz zrobić?", "Wypij miksturę", "Załóż lub zdejmij wyposażenie", "Wyjdź");
            io.Clear();
            switch (choice)
            {
                case 1:
                    PotionKind? kind = ChoosePotion(io, hero);
                    if (kind.HasValue)
                    {
                        int healed = hero.DrinkPotion(kind.Value) ?? 0;
                        io.ShowSuccess(DrinkMessage(hero, kind.Value, healed));
                    }

                    break;
                case 2:
                    ManageGear(hero);
                    break;
                default:
                    return;
            }
        }
    }

    public static string DrinkMessage(Hero hero, PotionKind kind, int healed)
    {
        ArgumentNullException.ThrowIfNull(hero);
        return Potion.Create(kind).Effect switch
        {
            PotionUse.Whisky => "Whisky pali w gardle. W następnej walce ręka będzie pewniejsza, ale nogi wolniejsze.",
            PotionUse.Lotus => $"Dym lotosu. Świat mięknie, ból znika. Następna walka będzie łatwa. Dawek: {hero.Addiction}.",
            PotionUse.Antidote => "Gorycz odtrutki wypala truciznę i głód.",
            _ => $"Wypiłeś miksturę i odzyskałeś {healed} HP. Masz teraz {hero.Hp}/{hero.MaxHp} HP.",
        };
    }

    private void Show(Hero hero)
    {
        io.WriteLine($"Punkty zdrowia: {hero.Hp}/{hero.MaxHp}    Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
        io.WriteLine($"Broń: {Describe(hero.Weapon)}   Pancerz: {Describe(hero.EquippedArmor)}   Amulet: {Describe(hero.Trinket)}", ConsoleColor.Cyan);
        if (hero.Inventory.Count == 0)
        {
            io.ShowError("Nie posiadasz żadnych mikstur.");
        }

        foreach (PotionKind kind in Potion.AllKinds)
        {
            int count = hero.CountPotions(kind);
            if (count > 0)
            {
                Potion sample = Potion.Create(kind);
                io.ShowInfo($"{count} x {sample.Name} ({sample.Description})");
            }
        }

        if (hero.NextFightBuff is PotionKind buff)
        {
            io.WriteLine($"Na następną walkę: {Potion.Create(buff).Name}.", ConsoleColor.Magenta);
        }

        if (hero.Craving)
        {
            io.ShowError("Głód lotosu: −15% obrażeń, −10 trafienia.");
        }

        io.WriteLine($"Torba ({hero.Gear.Count}/{Hero.GearCapacity}): " + (hero.Gear.Count == 0 ? "pusta" : string.Join(", ", hero.Gear.Select(g => g.Name))), ConsoleColor.Gray);
    }

    private static string Describe(Item? item) => item is null ? "brak" : $"{item.Name} ({ItemCatalog.Stats(item)})";

    private void ManageGear(Hero hero)
    {
        while (true)
        {
            var options = new List<(string Label, Action Action)>();
            foreach (Item item in hero.Gear.ToList())
            {
                string label = $"Załóż: {item.Name} [{ItemCatalog.SlotName(item.Slot)}] {ItemCatalog.Stats(item)}";
                if (!hero.CanEquip(item))
                {
                    label += " – nie dla twojej klasy";
                }

                options.Add((label, () => Equip(hero, item)));
            }

            foreach (ItemSlot slot in Enum.GetValues<ItemSlot>())
            {
                Item? equipped = slot switch
                {
                    ItemSlot.Weapon => hero.Weapon,
                    ItemSlot.Armor => hero.EquippedArmor,
                    _ => hero.Trinket,
                };
                if (equipped is not null)
                {
                    options.Add(($"Zdejmij: {equipped.Name} [{ItemCatalog.SlotName(slot)}]", () => Unequip(hero, slot)));
                }
            }

            if (options.Count == 0)
            {
                io.ShowInfo("Nie masz nic do założenia ani zdjęcia.");
                return;
            }

            options.Add(("Wróć", () => { }));
            int choice = io.Menu("Wyposażenie:", options.Select(o => o.Label).ToArray());
            io.Clear();
            if (choice == options.Count)
            {
                return;
            }

            options[choice - 1].Action();
        }
    }

    private void Equip(Hero hero, Item item)
    {
        if (!hero.CanEquip(item))
        {
            io.ShowError("Ta broń nie pasuje do twojej klasy. Możesz ją sprzedać w sklepie.");
            return;
        }

        Item? previous = hero.Equip(item);
        io.ShowSuccess($"Założono: {item.Name}." + (previous is null ? "" : $" {previous.Name} trafia do torby."));
        io.WriteLine($"Obrażenia {hero.MinDmg}-{hero.MaxDmg}, pancerz {hero.Armor}, uniki {hero.Evasion}%, krytyk {hero.CritChance:0}%, HP {hero.Hp}/{hero.MaxHp}.", ConsoleColor.Cyan);
    }

    private void Unequip(Hero hero, ItemSlot slot)
    {
        if (hero.Unequip(slot))
        {
            io.ShowSuccess("Zdjęto do torby.");
        }
        else
        {
            io.ShowError("Torba jest pełna.");
        }
    }

    /// <summary>Menu wyboru mikstury. Zwraca rodzaj, który bohater posiada, albo null przy rezygnacji lub braku sensu.</summary>
    public static PotionKind? ChoosePotion(IGameIO io, Hero hero)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Inventory.Count == 0)
        {
            io.ShowError("Nie masz żadnych mikstur.");
            return null;
        }

        var kinds = Potion.AllKinds.Where(k => hero.CountPotions(k) > 0).ToList();
        if (hero.Hp >= hero.MaxHp)
        {
            kinds = kinds.Where(k => Potion.Create(k).Effect != PotionUse.Heal).ToList();
            if (kinds.Count == 0)
            {
                io.ShowInfo($"Masz pełne zdrowie ({hero.Hp}/{hero.MaxHp}). Szkoda mikstury.");
                return null;
            }
        }

        var options = kinds.Select(k => $"{Potion.Create(k).Name} – {Potion.Create(k).Description} (masz: {hero.CountPotions(k)})").Append("Zrezygnuj").ToArray();
        int choice = io.Menu("Którą miksturę wypić?", options);
        if (choice == options.Length)
        {
            return null;
        }

        PotionKind kind = kinds[choice - 1];
        if (hero.CountPotions(kind) == 0)
        {
            io.ShowError("Nie masz takiej mikstury!");
            return null;
        }

        return kind;
    }
}
