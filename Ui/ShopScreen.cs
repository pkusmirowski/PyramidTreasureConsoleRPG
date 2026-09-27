namespace PyramidTreasureConsoleRPG.Ui;

public sealed class ShopScreen
{
    private readonly IGameIO io;
    private readonly IRandomSource rng;

    public ShopScreen(IGameIO io, IRandomSource rng)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
        this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header("Sklep alchemika");
            io.WriteLine($"Twoje złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            var options = ShopService.Offers
                .Select(p => $"{p.Name} – leczy {p.RestoreHp} HP – {p.Price} g (masz: {hero.CountPotions(p.Kind)})")
                .Append("Wyjdź ze sklepu")
                .ToArray();
            int choice = io.Menu("Co chcesz kupić?", options);
            io.Clear();
            if (choice == options.Length)
            {
                io.WriteLine("Wychodzisz ze sklepu...");
                return;
            }

            PurchaseResult result = ShopService.Buy(hero, ShopService.Offers[choice - 1].Kind);
            if (result.Outcome == PurchaseOutcome.Bought)
            {
                io.ShowSuccess($"Kupiłeś: {result.Potion.Name}. Zostało ci {hero.Gold} złota.");
            }
            else
            {
                io.ShowError(Dialogues.NoGold(rng));
            }
        }
    }
}
