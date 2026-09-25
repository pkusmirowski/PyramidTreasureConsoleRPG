namespace PyramidTreasureConsoleRPG;

public static class Tavern
{
    public static void Visit(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            GameIO.Header("Tawerna \"Pod Sokołem\"");
            GameIO.Menu(
                "Witaj w tawernie! Co chcesz zrobić?",
                Story.BarmanHasNews(hero) ? "Podejdź do baru (barman ma wieści!)" : "Podejdź do baru",
                "Podejdź do kasyna i spróbuj szczęścia",
                "Zapytaj o pokój na górze",
                "Wyjdź z tawerny");
            int choice = GameIO.ReadMenuChoice(4);
            GameIO.Clear();
            switch (choice)
            {
                case 1:
                    Bar.Visit(hero);
                    break;
                case 2:
                    Casino.Visit(hero);
                    break;
                case 3:
                    Rest.Visit(hero);
                    break;
                case 4:
                    GameIO.WriteLine("Do zobaczenia!");
                    return;
            }
        }
    }
}
