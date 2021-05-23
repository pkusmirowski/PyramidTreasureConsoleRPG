using System;
using System.Collections.Generic;

namespace GreatPyramidTreasureConsoleRPG
{
    public static class Dialogues
    {
        public static void NoGold()
        {
            var exp = new List<string>
            {
                "Nie masz tyle złota, paskudo!",
                "Nie masz tyle złota, frajerze!",
                "Frajerze, nawet złota nie masz!",
                "Biedaku, nawet złota nie masz!",
                "Chłopie, twoje kieszenie są całkowicie puste!"
            };
            string sentence = exp.RandomElement();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{sentence}");
            Console.ResetColor();
        }

        public static void TalkToTheGirls()
        {
            List<string> conversation = new()
            {
                "Co słychać?",
                "Ładnie wyglądasz.",
                "Podobam ci się?",
                "Chcesz mi coś zaoferować?"
            };
            string sentence = conversation.RandomElement();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{sentence}");
            Console.ResetColor();
        }

        public static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Dawno dawno temu w starożytnym Egipcie ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Powstała wielka piramida Chufu ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Obecnie jest rok 1411 a ty posiadasz mapę ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Z interpretacji, której wynika, że ów święty gral  ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Którego szukasz znajduje się w tejże piramidzie ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Aktualnie znajdujesz się w mieście odległym parę tysięcy kilometrów od celu ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Nie masz pojęcia jak się tam dostać ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zrobisz wszystko, aby odnaleźć gral ...");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }

        public static void Ending()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nBóg ra pada przed tobą ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Widzisz, że za nim znajduje się pewna skrzynia ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Jest to skrzynia ze złota ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Jesteś pewny, że w niej będzie to co szukasz czyli gral  ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Podchodzisz do skrzyni i próbujesz ją otworzyć ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Po kilkunastu minutach udaje ci się otworzyć skrzynię ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Blask z wnętrza oślepia cię ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Po chwili twój wzrok powraca ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Znajdujesz dziwne materiały i przedmioty ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zrobione z czegoś co jest złotem ale złoto to nie jest ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Możliwe, że pochodzi to z innego świata ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Grala nie znalazłeś ale za to pewien sztylet, który pobłyskuje...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zabierasz go i kilka innych przedmiotów w tym mapę i ruszasz w dalszą podróż ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Możliwe, że gdy zrozumiesz tę mapę trafisz na miejsce gdzie znajduje się święty gral ...");
            StandardFunctions.Sleep();
            Console.WriteLine("--- KONIEC GRY ---");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }

        public static void PyramidHistory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Po wielu godzinach podróży dotarłeś pod piramidę Chufu ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Wchodzisz do środka ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Idziesz coraz głębiej, twoja pochodnia pomału zaczyna gasnąć ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Mijasz kolejny korytaż aż w końcu słyszysz niepokojący dźwięk  ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Jest to niepokojące ale po takiej podróży i determinacji jaką się wykazałeś ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zrobisz wszystko aby zdobyć święty gral ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Wchodzisz do głównej komnaty ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Przed wejściem widzisz dwie postacie o głowie psa ...");
            StandardFunctions.Sleep();
            Console.WriteLine("A za nimi postać w cieniu, ...");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }

        public static void AfterGuards()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nAnubisy padają pod tobą ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Boska moc, która z nich się uwolniła przywróciła cię do pełni sił!");
            StandardFunctions.Sleep();
            Console.WriteLine("\nTajemnicza postać siedząca na tronie o głowie sokoła wstaje ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Okazuje się, że jest to sam bóg Ra ...");
            StandardFunctions.Sleep();
            Console.WriteLine("Szukuj się do walki!");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }

        public static void ConvWithBarman(IClass characterClass)
        {
            if (characterClass != null)
            {
                int level = characterClass.Level;

                switch (level)
                {
                    case int n when n < 5:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (characterClass.GameStatus == 0)
                        {
                            Console.WriteLine("Chłopie! Ostatnie kilka dni musieliśmy się chować przed bandytami.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Ciągle nas atakują. Mam nadzieję, że w końcu przyjedzie kawaleria i zrobi z nimi porządek.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Poza tym to, co zawsze. Czekamy aż przypłynie statek z towarami.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Najlepsze towary whisky i rum ... Ahh tego mi trzeba!");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Na razie dostępna jest tylko czysta woda źródlana, która doda ci sił!");
                            StandardFunctions.Sleep();
                            characterClass.GameStatus = 1;
                        }
                        else
                        {
                            Console.WriteLine("Na ten moment nie ma żadnych nowych wieści.");
                        }
                        Console.ResetColor();
                        break;

                    case int n when n >= 5 && n < 10:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (characterClass.GameStatus == 1)
                        {
                            Console.WriteLine("Ostatnio ataki bandytów się uspokoiły.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Słyszałem od ludzi, że w pobliskich lasach czają się wilki!");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Dziwne, dawno ich nie widziałem.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("A, przypłynął ten statek, o którym ci mówiłem.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("W końcu możesz skosztować najlepszej szkockiej whisky!");
                            StandardFunctions.Sleep();
                            characterClass.GameStatus = 2;
                        }
                        else
                        {
                            Console.WriteLine("Na ten moment nie ma żadnych nowych wieści.");
                        }
                        Console.ResetColor();
                        break;

                    case int n when n >= 10 && n < 20:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (characterClass.GameStatus == 2)
                        {
                            Console.WriteLine("O to znowu ty. Aktualnie w mieście przybyło sporo nowych ludzi.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Odkąd wilki i bandyci przestali atakować w pobliskim lesie, przejazd jest o wiele bezpieczniejszy.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Od podróżników, którzy biesiadowali parę dni temu, dowiedziałem się, że niedługo przybędzie karawana.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Mają zamiar jechać na pustynie szukać jakichś piramid. Jeśli chcesz się z nimi zabrać, zagadaj do mnie za jakiś czas.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Powiem ci, gdzie się znajdują, A i jeszcze coś.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Dostałem od nich specjalny trunek. Prosto z jakiegoś Malborka. Boje się tego spróbować.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Jeśli będziesz chciał, mogę ci polać szklanę, ale pamiętaj, to nie są tanie rzeczy!");
                            StandardFunctions.Sleep();
                            characterClass.GameStatus = 3;
                        }
                        else
                        {
                            Console.WriteLine("Na ten moment nie ma żadnych nowych wieści.");
                        }
                        Console.ResetColor();

                        break;

                    case int n when n == 20:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (characterClass.GameStatus == 3)
                        {
                            Console.WriteLine("A to ty! Dawno cię nie widziałem.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Sporo się zmieniło. Okolica stała się bardzo bezpieczna. Sporo nowych twarzy przewija się przez miasto!");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Wczoraj przybyła karawana. Okazuje się, że wyruszają pod samą Piramidę Chufu!");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Coś podobnego. To prawdopodobnie jedyna okazja, aby tam się dostać. Mało osób zna drogę w tamte dalekie rejony.");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Znajdują się obok bramy wjazdowej do miasta. Niedługo wyruszają, jeśli chcesz jechać, śpiesz się!");
                            StandardFunctions.Sleep();
                            Console.WriteLine("Miło się z tobą gadało. Mam nadzieję, że kiedyś nasze drogi jeszcze się skrzyżują!");
                            StandardFunctions.Sleep();
                            characterClass.GameStatus = 4;
                        }
                        else
                        {
                            Console.WriteLine("Na ten moment nie ma żadnych nowych wieści.");
                        }
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
}