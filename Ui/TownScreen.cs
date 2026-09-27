namespace PyramidTreasureConsoleRPG.Ui;

public enum SessionEnd
{
    ReturnedToMenu,
    Died,
    Completed,
}

/// <summary>Pętla miasta: podróż, tawerna, sklep, statystyki, sakwa, zapis.</summary>
public sealed class TownScreen(IGameIO io, IRandomSource rng, ISaveStore saves, CombatScreen combat, TavernScreen tavern, ShopScreen shop, InventoryScreen inventory)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly IRandomSource rng = rng ?? throw new ArgumentNullException(nameof(rng));
    private readonly ISaveStore saves = saves ?? throw new ArgumentNullException(nameof(saves));
    private readonly CombatScreen combat = combat ?? throw new ArgumentNullException(nameof(combat));
    private readonly TavernScreen tavern = tavern ?? throw new ArgumentNullException(nameof(tavern));
    private readonly ShopScreen shop = shop ?? throw new ArgumentNullException(nameof(shop));
    private readonly InventoryScreen inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

    public SessionEnd Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            bool pyramid = Story.CanEnterPyramid(hero);
            io.Header($"{hero.Name} – {hero.ClassName}, poziom {hero.Level}, {hero.Hp}/{hero.MaxHp} HP, {hero.Gold} złota");
            string travel = pyramid
                ? "Wyrusz z karawaną do piramidy Chufu"
                : Story.BarmanHasNews(hero) ? "Udaj się w drogę (barman ma wieści!)" : "Udaj się w drogę";
            int choice = io.Menu("Co chcesz zrobić?", travel, "Idź do tawerny", "Idź do sklepu", "Pokaż statystyki bohatera", "Sakwa", "Zapisz grę", "Wróć do menu głównego");
            io.Clear();
            switch (choice)
            {
                case 1:
                    SessionEnd? end = pyramid ? EnterPyramid(hero) : Travel(hero);
                    if (end.HasValue)
                    {
                        return end.Value;
                    }

                    break;
                case 2:
                    tavern.Run(hero);
                    break;
                case 3:
                    shop.Run(hero);
                    break;
                case 4:
                    io.ShowStats(hero);
                    break;
                case 5:
                    inventory.Run(hero);
                    break;
                case 6:
                    SaveGame(hero);
                    break;
                default:
                    if (ConfirmLeave())
                    {
                        return SessionEnd.ReturnedToMenu;
                    }

                    break;
            }
        }
    }

    private SessionEnd? Travel(Hero hero)
    {
        if (!Story.CanTravel(hero))
        {
            io.ShowInfo(Story.TravelBlockedMessage);
            return null;
        }

        if (hero.Hp < hero.MaxHp / 3)
        {
            io.ShowInfo($"Masz tylko {hero.Hp}/{hero.MaxHp} HP. Na pewno chcesz wyruszyć?");
            if (io.Menu("Wyruszyć?", "Tak", "Nie, najpierw się wyleczę") == 2)
            {
                io.Clear();
                return null;
            }
        }

        CombatStatus status = combat.Run(hero, Encounters.ForLevel(hero.Level, rng));
        return status == CombatStatus.Defeat ? Defeat() : null;
    }

    private SessionEnd? EnterPyramid(Hero hero)
    {
        io.ShowInfo("Karawana rusza o świcie. Nie będzie odwrotu.");
        if (io.Menu("Wyruszyć?", "Tak, do piramidy", "Jeszcze nie") == 2)
        {
            io.Clear();
            return null;
        }

        io.Clear();
        io.Narrate(Dialogues.PyramidHistory);
        if (combat.Run(hero, Encounters.PyramidGuards()) == CombatStatus.Defeat)
        {
            return Defeat();
        }

        io.Narrate(Dialogues.AfterGuards);
        hero.FullHeal();
        if (combat.Run(hero, Encounters.FinalBoss()) == CombatStatus.Defeat)
        {
            return Defeat();
        }

        hero.Completed = true;
        io.Narrate(Dialogues.Ending);
        io.ShowSuccess($"Ukończyłeś grę jako {hero.Name}, {hero.ClassName} na poziomie {hero.Level}, z {hero.Gold} sztukami złota!");
        io.PressAnyKey();
        return SessionEnd.Completed;
    }

    private SessionEnd Defeat()
    {
        io.ShowError("\nZostałeś pokonany. Twoja wyprawa kończy się w piachu, a Graal pozostaje legendą.");
        io.ShowInfo("Możesz wczytać ostatni zapis z menu głównego.");
        io.PressAnyKey();
        return SessionEnd.Died;
    }

    private void SaveGame(Hero hero)
    {
        if (saves.Exists() && io.Menu("Nadpisać istniejący zapis?", "Nie", "Tak") == 1)
        {
            io.Clear();
            return;
        }

        io.Clear();
        if (saves.Save(hero.ToSaveData(), out string message))
        {
            io.ShowSuccess(message);
        }
        else
        {
            io.ShowError(message);
        }
    }

    private bool ConfirmLeave()
    {
        bool leave = io.Menu("Niezapisany postęp zostanie utracony. Wrócić do menu?", "Nie", "Tak") == 2;
        io.Clear();
        return leave;
    }
}
