namespace PyramidTreasureConsoleRPG.Ui;

public sealed class RestScreen(IGameIO io, IRandomSource rng, RestService rest)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly RestService rest = rest ?? throw new ArgumentNullException(nameof(rest));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.WriteLine($"Zdrowie: {hero.Hp}/{hero.MaxHp}   Złoto: {hero.Gold}", ConsoleColor.DarkYellow);
            int choice = io.Menu(
                "Na górze:",
                $"Wynajmij pokój i prześpij się / {RestService.RoomCost} g (leczy do pełna)",
                "Pogadaj z dziewczynami z tawerny",
                $"Spędź noc w towarzystwie / {RestService.CompanyCost} g",
                "Zejdź na dół");
            io.Clear();
            switch (choice)
            {
                case 1:
                    RentRoom(hero);
                    break;
                case 2:
                    io.ShowInfo(Dialogues.TalkToTheGirls(rng));
                    break;
                case 3:
                    SpendNight(hero);
                    break;
                default:
                    return;
            }
        }
    }

    private void RentRoom(Hero hero)
    {
        switch (RestService.RentRoom(hero))
        {
            case RestOutcome.Ok:
                io.ShowSuccess($"Po długiej nocy czujesz się wypoczęty i pełen energii! Masz {hero.Hp}/{hero.MaxHp} HP i {hero.Gold} złota.");
                break;
            case RestOutcome.FullHealth:
                io.ShowInfo($"Nie potrzebujesz odpoczynku, masz pełne zdrowie: {hero.Hp}/{hero.MaxHp}.");
                break;
            default:
                io.ShowError(Dialogues.NoGold(rng));
                break;
        }
    }

    private void SpendNight(Hero hero)
    {
        NightResult? result = rest.SpendNight(hero);
        if (result is null)
        {
            io.ShowError(Dialogues.NoGold(rng));
            return;
        }

        io.Narrate(Dialogues.NightCompany, ConsoleColor.Magenta);
        switch (result.Event)
        {
            case NightEvent.Rumor:
                io.ShowSuccess("Nad ranem opowiada ci, co słyszała od karawaniarzy o piramidzie. Uczysz się więcej niż z niejednej walki.");
                io.WriteLine($"Zyskałeś {result.Amount} punktów doświadczenia" + (result.LevelsGained > 0 ? $" i awansowałeś na poziom {hero.Level}!" : "."), ConsoleColor.DarkCyan);
                break;
            case NightEvent.Robbed:
                io.ShowError($"Budzisz się sam. Sakwa jest lżejsza o {result.Amount} sztuk złota, a po dziewczynie ani śladu. Barman udaje, że nic nie widział.");
                break;
            case NightEvent.Gift:
                io.ShowSuccess($"Rano znajdujesz przy łóżku {result.Gift?.Name} i liścik: \"Wróć żywy.\"");
                break;
            default:
                io.ShowSuccess($"Budzisz się rano wypoczęty, z uśmiechem i pełnym zdrowiem ({hero.Hp}/{hero.MaxHp} HP).");
                break;
        }
    }
}
