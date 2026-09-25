namespace PyramidTreasureConsoleRPG;

/// <summary>Pokoje na górze: nocleg, rozmowy, noc w towarzystwie (z konsekwencjami).</summary>
public static class Rest
{
    public const int RoomCost = 10;
    public const int CompanyCost = 30;

    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.WriteLine($"Zdrowie: {hero.Hp}/{hero.MaxHp}   Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            GameIO.Menu(
                "Na górze:",
                $"Wynajmij pokój i prześpij się / {RoomCost} g (leczy do pełna)",
                "Pogadaj z dziewczynami z tawerny",
                $"Spędź noc w towarzystwie / {CompanyCost} g",
                "Zejdź na dół");
            int choice = GameIO.ReadMenuChoice(4);
            GameIO.Clear();
            switch (choice)
            {
                case 1:
                    RentRoom(hero);
                    break;
                case 2:
                    Dialogues.TalkToTheGirls();
                    break;
                case 3:
                    SpendNight(hero);
                    break;
                case 4:
                    return;
            }
        }
    }

    private static void RentRoom(Hero hero)
    {
        if (hero.Hp >= hero.MaxHp)
        {
            GameIO.Info($"Nie potrzebujesz odpoczynku, masz pełne zdrowie: {hero.Hp}/{hero.MaxHp}.");
            return;
        }

        if (hero.Gold < RoomCost)
        {
            Dialogues.NoGold();
            return;
        }

        hero.Gold -= RoomCost;
        hero.FullHeal();
        GameIO.Success($"Po długiej nocy czujesz się wypoczęty i pełen energii! Masz {hero.Hp}/{hero.MaxHp} HP i {hero.Gold} złota.");
    }

    private static void SpendNight(Hero hero)
    {
        if (hero.Gold < CompanyCost)
        {
            Dialogues.NoGold();
            return;
        }

        hero.Gold -= CompanyCost;
        hero.FullHeal();
        GameIO.Narrate(
            ConsoleColor.Magenta,
            "Dziewczyna o oczach koloru pustynnego nieba bierze cię za rękę i prowadzi po skrzypiących schodach.",
            "Drzwi się zamykają. Lampa gaśnie. Reszta nocy należy tylko do was dwojga.");

        int roll = Rng.Range(1, 100);
        if (roll <= 40)
        {
            GameIO.Success($"Budzisz się rano wypoczęty, z uśmiechem i pełnym zdrowiem ({hero.Hp}/{hero.MaxHp} HP).");
        }
        else if (roll <= 70)
        {
            int exp = Math.Max(50, hero.ExpToNextLevel / 20);
            int levels = hero.AddExp(exp);
            GameIO.Success("Nad ranem opowiada ci, co słyszała od karawaniarzy o piramidzie. Uczysz się więcej niż z niejednej walki.");
            GameIO.WriteLine($"Zyskałeś {exp} punktów doświadczenia" + (levels > 0 ? $" i awansowałeś na poziom {hero.Level}!" : "."), ConsoleColor.DarkCyan);
        }
        else if (roll <= 90)
        {
            int stolen = hero.Gold / 10;
            hero.Gold -= stolen;
            GameIO.Error($"Budzisz się sam. Sakwa jest lżejsza o {stolen} sztuk złota, a po dziewczynie ani śladu. Barman udaje, że nic nie widział.");
        }
        else
        {
            var potion = new MediumPotion();
            hero.Inventory.Add(potion);
            GameIO.Success($"Rano znajdujesz przy łóżku {potion.Name} i liścik: \"Wróć żywy.\"");
        }
    }
}
