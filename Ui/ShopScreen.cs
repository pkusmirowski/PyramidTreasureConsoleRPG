namespace PyramidTreasureConsoleRPG;

public static class Shop
{
    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.Header("Sklep alchemika");
            GameIO.WriteLine($"Twoje złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            var offers = Potion.AllKinds.Select(Potion.Create).ToList();
            var options = offers
                .Select(p => $"{p.Name} – leczy {p.RestoreHp} HP – {p.Price} g (masz: {hero.CountPotions(p.Kind)})")
                .Append("Wyjdź ze sklepu")
                .ToArray();
            GameIO.Menu("Co chcesz kupić?", options);
            int choice = GameIO.ReadMenuChoice(options.Length);
            GameIO.Clear();
            if (choice == options.Length)
            {
                GameIO.WriteLine("Wychodzisz ze sklepu...");
                return;
            }

            Buy(hero, offers[choice - 1]);
        }
    }

    public static bool Buy(Hero hero, Potion potion)
    {
        ArgumentNullException.ThrowIfNull(hero);
        ArgumentNullException.ThrowIfNull(potion);
        if (hero.Gold < potion.Price)
        {
            Dialogues.NoGold();
            return false;
        }

        hero.Gold -= potion.Price;
        hero.Inventory.Add(potion);
        GameIO.Success($"Kupiłeś: {potion.Name}. Zostało ci {hero.Gold} złota.");
        return true;
    }
}
