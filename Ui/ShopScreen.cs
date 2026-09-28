namespace PyramidTreasureConsoleRPG.Ui;

public sealed class ShopScreen(IGameIO io, IRandomSource rng, GameSettings settings)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly GameSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        RegionDefinition region = RegionCatalog.Get(hero.CurrentRegion);
        while (true)
        {
            io.Header($"Sklep – {region.Name}");
            int percent = ShopService.PricePercent(hero, region);
            io.WriteLine($"Twoje złoto: {hero.Gold}   Ceny: {percent}% (reputacja i amulety)   Torba: {hero.Gear.Count}/{Hero.GearCapacity}", ConsoleColor.DarkYellow);
            int choice = io.Menu("Co chcesz zrobić?", "Kup mikstury", "Kup wyposażenie", "Sprzedaj wyposażenie", "Wyjdź ze sklepu");
            io.Clear();
            switch (choice)
            {
                case 1:
                    BuyPotions(hero, region);
                    break;
                case 2:
                    BuyGear(hero, region);
                    break;
                case 3:
                    Sell(hero);
                    break;
                default:
                    io.WriteLine("Wychodzisz ze sklepu...");
                    return;
            }
        }
    }

    private void BuyPotions(Hero hero, RegionDefinition region)
    {
        while (true)
        {
            io.WriteLine($"Twoje złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            IReadOnlyList<Potion> potions = ShopService.Potions(region);
            var options = potions
                .Select(p => $"{p.Name} – {p.Description} – {ShopService.Price(hero, region, p.Price)} g (masz: {hero.CountPotions(p.Kind)})")
                .Append("Wróć")
                .ToArray();
            int choice = io.Menu("Mikstury:", options);
            io.Clear();
            if (choice == options.Length)
            {
                return;
            }

            Report(hero, ShopService.BuyPotion(hero, region, potions[choice - 1].Kind));
        }
    }

    private void BuyGear(Hero hero, RegionDefinition region)
    {
        while (true)
        {
            IReadOnlyList<Item> stock = ShopService.Stock(region);
            if (stock.Count == 0)
            {
                io.ShowInfo("Tutaj nie handluje się wyposażeniem.");
                return;
            }

            io.WriteLine($"Twoje złoto: {hero.Gold}   Torba: {hero.Gear.Count}/{Hero.GearCapacity}", ConsoleColor.DarkYellow);
            var options = stock
                .Select(i => $"{i.Name} [{ItemCatalog.SlotName(i.Slot)}] {ItemCatalog.Stats(i)} – {ShopService.Price(hero, region, i.Price)} g" + (hero.CanEquip(i) ? "" : " (nie dla twojej klasy)"))
                .Append("Wróć")
                .ToArray();
            int choice = io.Menu("Wyposażenie:", options);
            io.Clear();
            if (choice == options.Length)
            {
                return;
            }

            Report(hero, ShopService.BuyItem(hero, region, stock[choice - 1]));
        }
    }

    private void Sell(Hero hero)
    {
        while (true)
        {
            if (hero.Gear.Count == 0)
            {
                io.ShowInfo("Torba jest pusta. Założone rzeczy najpierw zdejmij w sakwie.");
                return;
            }

            var gear = hero.Gear.ToList();
            var options = gear.Select(i => $"{i.Name} ({ItemCatalog.Stats(i)}) – skup {i.SellPrice} g").Append("Wróć").ToArray();
            int choice = io.Menu("Co sprzedajesz?", options);
            io.Clear();
            if (choice == options.Length)
            {
                return;
            }

            int earned = ShopService.Sell(hero, gear[choice - 1]);
            io.ShowSuccess($"Sprzedano: {gear[choice - 1].Name} za {earned} g. Masz {hero.Gold} złota.");
        }
    }

    private void Report(Hero hero, PurchaseResult result)
    {
        switch (result.Outcome)
        {
            case PurchaseOutcome.Bought:
                io.ShowSuccess($"Kupiłeś: {result.Name} za {result.Price} g. Zostało ci {hero.Gold} złota.");
                break;
            case PurchaseOutcome.BagFull:
                io.ShowError("Torba jest pełna. Sprzedaj coś albo załóż.");
                break;
            default:
                io.ShowError(Dialogues.NoGold(rng, settings.ProfanityEnabled));
                break;
        }
    }
}
