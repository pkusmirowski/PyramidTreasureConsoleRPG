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
            List<string> conversation = new List<string>
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
            Console.WriteLine("Powstała wielka piramida Chufu ...");
            Console.WriteLine("Obecnie jest rok 1411 a ty posiadasz mapę ...");
            Console.WriteLine("Z interpretacji, której wynika, że ów święty gral  ...");
            Console.WriteLine("Którego szukasz znajduje się w tejże piramidzie ...");
            Console.WriteLine("Aktualnie znajdujesz się w mieście odległym parę tysięcy kilometrów od celu ...");
            Console.WriteLine("Nie masz pojęcia jak się tam dostać ...");
            Console.WriteLine("Zrobisz wszystko, aby odnaleźć gral ...");
            Console.ResetColor();
        }

        public static void Ending()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nBóg ra pada przed tobą ...");
            Console.WriteLine("Widzisz, że za nim znajduje się pewna skrzynia ...");
            Console.WriteLine("Jest to skrzynia ze złota ...");
            Console.WriteLine("Jesteś pewny, że w niej będzie to co szukasz czyli gral  ...");
            Console.WriteLine("Podchodzisz do skrzyni i próbujesz ją otworzyć ...");
            Console.WriteLine("Po kilkunastu minutach udaje ci się otworzyć skrzynię ...");
            Console.WriteLine("Blask z wnętrza oślepia cię ...");
            Console.WriteLine("Po chwili twój wzrok powraca ...");
            Console.WriteLine("Znajdujesz dziwne materiały i przedmioty ...");
            Console.WriteLine("Zrobione z czegoś co jest złotem ale złoto to nie jest ...");
            Console.WriteLine("Możliwe, że pochodzi to z innego świata ...");
            Console.WriteLine("Grala nie znalazłeś ale za to pewien sztylet, który pobłyskuje...");
            Console.WriteLine("Zabierasz go i kilka innych przedmiotów w tym mapę i ruszasz w dalszą podróż ...");
            Console.WriteLine("Możliwe, że gdy zrozumiesz tę mapę trafisz na miejsce gdzie znajduje się święty gral ...");
            Console.WriteLine("--- KONIEC GRY ---");
            Console.ResetColor();
        }

        public static void PyramidHistory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Po wielu godzinach podróży dotarłeś pod piramidę Chufu ...");
            Console.WriteLine("Wchodzisz do środka ...");
            Console.WriteLine("Idziesz coraz głębiej, twoja pochodnia pomału zaczyna gasnąć ...");
            Console.WriteLine("Mijasz kolejny korytaż aż w końcu słyszysz niepokojący dźwięk  ...");
            Console.WriteLine("Jest to niepokojące ale po takiej podróży i determinacji jaką się wykazałeś ...");
            Console.WriteLine("Zrobisz wszystko aby zdobyć święty gral ...");
            Console.WriteLine("Wchodzisz do głównej komnaty ...");
            Console.WriteLine("Przed wejściem widzisz dwie postacie o głowie psa ...");
            Console.WriteLine("A za nimi postać w cieniu, ...");
            Console.ResetColor();
        }

        public static void AfterGuards()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nAnubisy padają pod tobą ...");
            Console.WriteLine("Boska moc, która z nich się uwolniła przywróciła cię do pełni sił!");
            Console.WriteLine("\nTajemnicza postać siedząca na tronie o głowie sokoła wstaje ...");
            Console.WriteLine("Okazuje się, że jest to sam bóg Ra ...");
            Console.WriteLine("Szukuj się do walki!");
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
                        Console.WriteLine("Chłopie! Ostatnie kilka dni musieliśmy się chować przed bandytami.");
                        Console.WriteLine("Ciągle nas atakują. Mam nadzieję, że w końcu przyjedzie kawaleria i zrobi z nimi porządek.");
                        Console.WriteLine("Poza tym to, co zawsze. Czekamy aż przypłynie statek z towarami.");
                        Console.WriteLine("Najlepsze towary whisky i rum ... Ahh tego mi trzeba!");
                        Console.WriteLine("Na razie dostępna jest tylko czysta woda źródlana, która doda ci sił!");
                        Console.ResetColor();
                        if(characterClass.GameStatus == 0)
                            characterClass.GameStatus = 1;
                        break;

                    case int n when n >= 5 && n < 10:
                        Console.ResetColor();
                        Console.WriteLine("Ostatnio ataki bandytów się uspokoiły.");
                        Console.WriteLine("Słyszałem od ludzi, że w pobliskich lasach czają się wilki!");
                        Console.WriteLine("Dziwne, dawno ich nie widziałem.");
                        Console.WriteLine("A, przypłynął ten statek, o którym ci mówiłem.");
                        Console.WriteLine("W końcu możesz skosztować najlepszej szkockiej whisky!");
                        Console.ResetColor();
                        if (characterClass.GameStatus == 1)
                            characterClass.GameStatus = 2;
                        break;

                    case int n when n >= 10 && n < 20:
                        Console.ResetColor();
                        Console.WriteLine("O to znowu ty. Aktualnie w mieście przybyło sporo nowych ludzi.");
                        Console.WriteLine("Odkąd wilki i bandyci przestali atakować w pobliskim lesie, przejazd jest o wiele bezpieczniejszy.");
                        Console.WriteLine("Od podróżników, którzy biesiadowali parę dni temu, dowiedziałem się, że niedługo przybędzie karawana.");
                        Console.WriteLine("Mają zamiar jechać na pustynie szukać jakichś piramid. Jeśli chcesz się z nimi zabrać, zagadaj do mnie za jakiś czas.");
                        Console.WriteLine("Powiem ci, gdzie się znajdują, A i jeszcze coś.");
                        Console.WriteLine("Dostałem od nich specjalny trunek. Prosto z jakiegoś Malborka. Boje się tego spróbować.");
                        Console.WriteLine("Jeśli będziesz chciał, mogę ci polać szklanę, ale pamiętaj, to nie są tanie rzeczy!");
                        Console.ResetColor();
                        if (characterClass.GameStatus == 2)
                            characterClass.GameStatus = 3;
                        break;

                    case int n when n == 20:
                        Console.ResetColor();
                        Console.WriteLine("A to ty! Dawno cię nie widziałem.");
                        Console.WriteLine("Sporo się zmieniło. Okolica stała się bardzo bezpieczna. Sporo nowych twarzy przewija się przez miasto!");
                        Console.WriteLine("Wczoraj przybyła karawana. Okazuje się, że wyruszają pod samą Piramidę Chufu!");
                        Console.WriteLine("Coś podobnego. To prawdopodobnie jedyna okazja, aby tam się dostać. Mało osób zna drogę w tamte dalekie rejony.");
                        Console.WriteLine("Znajdują się obok bramy wjazdowej do miasta. Niedługo wyruszają, jeśli chcesz jechać, śpiesz się!");
                        Console.WriteLine("Miło się z tobą gadało. Mam nadzieję, że kiedyś nasze drogi jeszcze się skrzyżują!");
                        Console.ResetColor();
                        if (characterClass.GameStatus == 3)
                            characterClass.GameStatus = 4;
                        break;
                }
            }
        }
    }
}