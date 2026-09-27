namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Sakwa poza walką. Wybór mikstury jest wspólny z ekranem walki.</summary>
public sealed class InventoryScreen
{
    private readonly IGameIO io;

    public InventoryScreen(IGameIO io)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
    }

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header("Sakwa");
            Show(hero);
            int choice = io.Menu("Co chcesz zrobić?", "Wypij miksturę", "Wyjdź");
            io.Clear();
            if (choice == 2)
            {
                return;
            }

            PotionKind? kind = ChoosePotion(io, hero);
            if (kind.HasValue)
            {
                int healed = hero.DrinkPotion(kind.Value) ?? 0;
                io.ShowSuccess($"Wypiłeś miksturę i odzyskałeś {healed} HP. Masz teraz {hero.Hp}/{hero.MaxHp} HP.");
            }
        }
    }

    private void Show(Hero hero)
    {
        io.WriteLine($"Punkty zdrowia: {hero.Hp}/{hero.MaxHp}    Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
        if (hero.Inventory.Count == 0)
        {
            io.ShowError("Nie posiadasz żadnych mikstur.");
            return;
        }

        foreach (PotionKind kind in Potion.AllKinds)
        {
            int count = hero.CountPotions(kind);
            if (count > 0)
            {
                Potion sample = Potion.Create(kind);
                io.ShowInfo($"{count} x {sample.Name} (leczy {sample.RestoreHp} HP)");
            }
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

        if (hero.Hp >= hero.MaxHp)
        {
            io.ShowInfo($"Masz pełne zdrowie ({hero.Hp}/{hero.MaxHp}). Szkoda mikstury.");
            return null;
        }

        var kinds = Potion.AllKinds.ToList();
        var options = kinds.Select(k => $"{Potion.Create(k).Name} (masz: {hero.CountPotions(k)})").Append("Zrezygnuj").ToArray();
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
