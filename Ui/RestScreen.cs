namespace PyramidTreasureConsoleRPG.Ui;

public sealed class RestScreen(IGameIO io, IRandomSource rng, RestService rest, NpcScreen npcScreen, GameSettings settings)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly RestService rest = rest ?? throw new ArgumentNullException(nameof(rest));
    private readonly NpcScreen npcScreen = npcScreen ?? throw new ArgumentNullException(nameof(npcScreen));
    private readonly GameSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>Zwraca Defeat, jeśli rozmowa z NPC skończyła się przegraną walką.</summary>
    public CombatStatus? Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.WriteLine($"Zdrowie: {hero.Hp}/{hero.MaxHp}   Złoto: {hero.Gold}" + (hero.NightmareNights > 0 ? $"   Koszmary: jeszcze {hero.NightmareNights} noce" : ""), ConsoleColor.DarkYellow);
            var npcs = NpcCatalog.InRegion(hero.CurrentRegion).ToList();
            var options = new List<string>
            {
                $"Wynajmij pokój i prześpij się / {RestService.RoomCost} g (leczy do pełna)",
                "Pogadaj z dziewczynami z tawerny",
                $"Spędź noc w towarzystwie / {RestService.CompanyCost} g",
            };
            options.AddRange(npcs.Select(n => $"Odwiedź: {n.Name} – {n.Description}"));
            options.Add("Zejdź na dół");
            int choice = io.Menu("Na górze:", [.. options]);
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
                    if (choice == options.Count)
                    {
                        return null;
                    }

                    CombatStatus? status = npcScreen.Talk(hero, npcs[choice - 4]);
                    if (status == CombatStatus.Defeat)
                    {
                        return status;
                    }

                    break;
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
            case RestOutcome.Nightmares:
                io.ShowInfo($"Budzisz się z krzykiem. Widzisz połamane palce i twarz, która nie chce zniknąć. Sen nie leczy do końca: {hero.Hp}/{hero.MaxHp} HP.");
                break;
            case RestOutcome.FullHealth:
                io.ShowInfo($"Nie potrzebujesz odpoczynku, masz pełne zdrowie: {hero.Hp}/{hero.MaxHp}.");
                break;
            default:
                io.ShowError(Dialogues.NoGold(rng, settings.ProfanityEnabled));
                break;
        }
    }

    private void SpendNight(Hero hero)
    {
        NightResult? result = rest.SpendNight(hero);
        if (result is null)
        {
            io.ShowError(Dialogues.NoGold(rng, settings.ProfanityEnabled));
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
