namespace PyramidTreasureConsoleRPG;

/// <summary>Ekran ekwipunku (poza walką). W walce mikstury pije się z menu walki.</summary>
public static class Inventory
{
    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.Header("Sakwa");
            Show(hero);
            GameIO.Menu("Co chcesz zrobić?", "Wypij miksturę", "Wyjdź");
            int choice = GameIO.ReadMenuChoice(2);
            GameIO.Clear();
            if (choice == 2)
            {
                return;
            }

            DrinkMenu(hero);
        }
    }

    public static void Show(Hero hero)
    {
        GameIO.WriteLine($"Punkty zdrowia: {hero.Hp}/{hero.MaxHp}    Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
        if (hero.Inventory.Count == 0)
        {
            GameIO.Error("Nie posiadasz żadnych mikstur.");
            return;
        }

        foreach (PotionKind kind in Potion.AllKinds)
        {
            int count = hero.CountPotions(kind);
            if (count > 0)
            {
                Potion sample = Potion.Create(kind);
                GameIO.Info($"{count} x {sample.Name} (leczy {sample.RestoreHp} HP)");
            }
        }
    }

    /// <summary>Menu wyboru mikstury. Zwraca true, jeśli coś wypito.</summary>
    public static bool DrinkMenu(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        if (hero.Inventory.Count == 0)
        {
            GameIO.Error("Nie masz żadnych mikstur.");
            return false;
        }

        if (hero.Hp >= hero.MaxHp)
        {
            GameIO.Info($"Masz pełne zdrowie ({hero.Hp}/{hero.MaxHp}). Szkoda mikstury.");
            return false;
        }

        var kinds = Potion.AllKinds.ToList();
        var options = kinds
            .Select(k => $"{Potion.Create(k).Name} (masz: {hero.CountPotions(k)})")
            .Append("Zrezygnuj")
            .ToArray();
        GameIO.Menu("Którą miksturę wypić?", options);
        int choice = GameIO.ReadMenuChoice(options.Length);
        if (choice == options.Length)
        {
            return false;
        }

        int? healed = hero.DrinkPotion(kinds[choice - 1]);
        if (healed is null)
        {
            GameIO.Error("Nie masz takiej mikstury!");
            return false;
        }

        GameIO.Success($"Wypiłeś miksturę i odzyskałeś {healed} HP. Masz teraz {hero.Hp}/{hero.MaxHp} HP.");
        return true;
    }
}
