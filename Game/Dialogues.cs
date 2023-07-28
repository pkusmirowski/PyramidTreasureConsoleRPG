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
                "Chłopie, twoje kieszenie są całkowicie puste!",
                "Zobaczmy... szukasz czegoś, czego nie masz? A to pech, bo nie masz tyle złota!",
                "Twoje kieszenie są tak puste, że można w nich usłyszeć echa!",
                "Złoto nie rośnie na drzewach, więc musisz je zdobyć gdzie indziej. Tylko niestety, nie teraz...",
                "Nie martw się, jesteś w dobrym towarzystwie - w grupie ludzi, którzy nie mają złota.",
                "Złoto jest wrogiem, prawda? Dlatego ono zawsze ucieka z twojego portfela."
            };
            string sentence = exp.GetRandomElement();
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
                "Chcesz mi coś zaoferować?",
                "Jesteś stąd? Ja jestem z pobliskiej wioski.",
                "Lubię mężczyzn, którzy potrafią walczyć. A ty?",
                "Czy zawsze taki przystojny jesteś, czy tylko dla mnie?",
                "Słyszałam, że jesteś w poszukiwaniu skarbu. Ja też lubię pieniądze.",
                "Czy miałeś już kiedyś serce złamane?",
                "Nie lubię nudnych rozmów. Masz coś ciekawego do powiedzenia?"
            };
            string sentence = conversation.GetRandomElement();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{sentence}");
            Console.ResetColor();
        }


        public static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Witaj w starożytnym Egipcie, w czasach, kiedy władzę sprawowała dynastia faraonów...");
            StandardFunctions.Sleep();
            Console.WriteLine("W tajemniczej i magicznej piramidzie Chufu, skrywa się niesamowite skarby oraz święty Graal...");
            StandardFunctions.Sleep();
            Console.WriteLine("Jesteś odważnym poszukiwaczem przygód, który zyskał dostęp do mapy, wskazującej dokładne miejsce ukrycia Graala...");
            StandardFunctions.Sleep();
            Console.WriteLine("Odnalezienie skarbu będzie nie lada wyzwaniem, ale jesteś zdeterminowany i gotowy, by podjąć to zadanie...");
            StandardFunctions.Sleep();
            Console.WriteLine("Twoja wyprawa rozpoczyna się w odległym mieście, oddalonym o tysiące kilometrów od celu...");
            StandardFunctions.Sleep();
            Console.WriteLine("Nie masz pojęcia jak się tam dostać, ale wierzysz w swoje umiejętności i szczęście...");
            StandardFunctions.Sleep();
            Console.WriteLine("Odkryjesz sekrety piramidy, stoczysz niebezpieczne pojedynki z groźnymi przeciwnikami i podejmiesz wiele decyzji, które będą miały wpływ na Twoje dalsze losy...");
            StandardFunctions.Sleep();
            Console.WriteLine("Czy uda Ci się odnaleźć święty Graal? Przekonaj się sam...");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }


        public static void Ending()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nBóg Ra pada przed tobą...");
            StandardFunctions.Sleep();
            Console.WriteLine("Widzisz, że za nim znajduje się pewna skrzynia...");
            StandardFunctions.Sleep();
            Console.WriteLine("Jest to skrzynia ze złota, a w niej powinien znajdować się święty gral...");
            StandardFunctions.Sleep();
            Console.WriteLine("Podchodzisz do skrzyni i z trudem otwierasz ją...");
            StandardFunctions.Sleep();
            Console.WriteLine("Blask z wnętrza oślepia cię na chwilę...");
            StandardFunctions.Sleep();
            Console.WriteLine("Kiedy twoje oczy przyzwyczajają się do światła, zobaczysz, że w skrzyni znajdują się dziwne materiały i przedmioty, wykonane z czegoś, co przypomina złoto, ale nie jest nim...");
            StandardFunctions.Sleep();
            Console.WriteLine("Możliwe, że pochodzą z innego świata, innej rzeczywistości, którą udało ci się przejść...");
            StandardFunctions.Sleep();
            Console.WriteLine("Szukasz świętego grala, ale go nie znajdujesz. Zamiast tego, wśród przedmiotów dostrzegasz coś, co przyciąga twoją uwagę - piękny sztylet o złotych zdobieniach, który pobłyskuje...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zabierasz go i kilka innych rzeczy, w tym mapę, która może poprowadzić cię do celu...");
            StandardFunctions.Sleep();
            Console.WriteLine("Zrozumiesz ją na pewno, gdy nadarzy się odpowiednia okazja. Wiedząc, że przed tobą jeszcze wiele przygód, decydujesz się ruszyć w drogę...");
            StandardFunctions.Sleep();
            Console.WriteLine("Może uda ci się odnaleźć święty gral i spełnić swoje marzenia?...");
            StandardFunctions.Sleep();
            Console.WriteLine("--- KONIEC GRY ---");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }


        public static void PyramidHistory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Znalazłeś się przed potężną piramidą Chufu, która kusiła ludzi od wieków swoim bogactwem i tajemnicami.");
            StandardFunctions.Sleep();
            Console.WriteLine("Wchodzisz do jej wnętrza, w głąb mrocznych korytarzy, które kryją niezwykłe sekrety.");
            StandardFunctions.Sleep();
            Console.WriteLine("Początkowo światło twojej pochodni jest silne, ale im dalej idziesz, tym słabiej świeci, odsłaniając jedynie kontury nieznanych ci przedmiotów.");
            StandardFunctions.Sleep();
            Console.WriteLine("Czujesz pod stopami kamienne schody, a po bokach widzisz niezliczoną ilość korytarzy, z których każdy może skrywać niebezpieczeństwo.");
            StandardFunctions.Sleep();
            Console.WriteLine("Nieustannie dasz radę, bo masz cel, który przyciąga cię mocniej niż złoto – święty graal.");
            StandardFunctions.Sleep();
            Console.WriteLine("W końcu dochodzisz do ogromnej komnaty, gdzie ściany zdobią malowidła przedstawiające legendy o piramidzie i jej władcach.");
            StandardFunctions.Sleep();
            Console.WriteLine("Stoisz przed dwoma posągami w kształcie psów, których mądrość wprawia w osłupienie każdego, kto się z nimi spotka.");
            StandardFunctions.Sleep();
            Console.WriteLine("A za nimi, w cieniu, wyczekuje cię największa z zagadek, w której rozwiązaniu tkwi tajemnica nieśmiertelności.");
            StandardFunctions.Sleep();
            Console.ResetColor();
        }


        public static void AfterGuards()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nAnubisy padają pod twoim mieczem, a ich ciała rozpadają się w drobny pył.");
            StandardFunctions.Sleep();
            Console.WriteLine("Jednak nagle poczujesz, jak boska moc, która się z nich uwolniła, przywraca ci pełnię sił!");
            StandardFunctions.Sleep();
            Console.WriteLine("\nNiespodziewanie, tajemnicza postać siedząca na tronie o głowie sokoła wstaje.");
            StandardFunctions.Sleep();
            Console.WriteLine("To sam bóg Ra, boski władca egipskich bogów, z którym musisz stoczyć ostateczną walkę!");
            StandardFunctions.Sleep();
            Console.WriteLine("\nTwoje dłonie ściskają sztylet zrobiony z nieznanego złota, który podobno zdoła pokonać każdego przeciwnika.");
            StandardFunctions.Sleep();
            Console.WriteLine("Nie wiesz, czy to prawda, ale czujesz, jak pulsujące w żyłach krew wzbiera w gorącu.");
            StandardFunctions.Sleep();
            Console.WriteLine("\nBóg Ra patrzy na ciebie surowym spojrzeniem, gotowy do walki.");
            StandardFunctions.Sleep();
            Console.WriteLine("Przygotuj się do ostatniej walki, która zdecyduje o twoim losie oraz losie świętego grala!");
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