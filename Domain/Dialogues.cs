using System.Globalization;

namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Wszystkie teksty narracyjne w jednym miejscu. Klasa tylko zwraca tekst – nie zna konsoli.</summary>
public static class Dialogues
{
    public const string NoNews = "Barman wzrusza ramionami: \"Na ten moment nie mam żadnych nowych wieści. Wróć, jak nabierzesz doświadczenia.\"";

    private static readonly string[] NoGoldLines =
    {
        "Nie masz tyle złota, paskudo!",
        "Nie masz tyle złota, frajerze!",
        "Frajerze, nawet złota nie masz!",
        "Biedaku, nawet złota nie masz!",
        "Chłopie, twoje kieszenie są całkowicie puste!",
        "Szukasz czegoś, czego nie masz? A to pech, bo nie masz tyle złota!",
        "Twoje kieszenie są tak puste, że słychać w nich echo!",
        "Złoto nie rośnie na drzewach. Wróć, jak je zdobędziesz.",
        "Nie martw się, jesteś w dobrym towarzystwie – ludzi bez złota.",
        "Złoto to twój wróg, prawda? Dlatego zawsze ucieka z twojej sakwy.",
    };

    private static readonly string[] GirlsLines =
    {
        "\"Co słychać, wędrowcze?\"",
        "\"Ładnie wyglądasz jak na kogoś, kto śpi w rowach.\"",
        "\"Podobam ci się? Widzę, że tak.\"",
        "\"Chcesz mi coś zaoferować? Złoto zawsze mile widziane.\"",
        "\"Jesteś stąd? Ja jestem z pobliskiej wioski. Uciekłam, jak przyszli bandyci.\"",
        "\"Lubię ludzi, którzy potrafią walczyć. A ty potrafisz?\"",
        "\"Słyszałam, że szukasz skarbu. Ja też lubię pieniądze.\"",
        "\"Miałeś już kiedyś złamane serce? Bo ja mam złamany nóż i to gorsze.\"",
        "\"Nie lubię nudnych rozmów. Masz coś ciekawego do powiedzenia?\"",
        "\"Karawaniarze mówią, że w piramidzie nikt nie przeżył nocy. Ty wyglądasz, jakbyś chciał spróbować.\"",
    };

    private static readonly string[] HumanKillLines =
    {
        "{0} osuwa się na kolana, próbując zatkać ranę dłońmi. Nie daje rady.",
        "Ostatni cios trafia w gardło. {0} charczy jeszcze chwilę, zanim zamilknie na dobre.",
        "{0} pada twarzą w piach. Krew wsiąka w ziemię szybciej, niż zdążysz otrzeć ostrze.",
        "Twój cios rozłupuje hełm. {0} już nie wstanie.",
    };

    private static readonly string[] BeastKillLines =
    {
        "{0} wyje ostatni raz i nieruchomieje w kałuży własnej krwi.",
        "Zwierzę rzuca się jeszcze raz, ale trafia na twoje ostrze. {0} kona u twoich stóp.",
        "{0} drga w konwulsjach, po czym zastyga.",
    };

    private static readonly string[] UndeadKillLines =
    {
        "{0} rozpada się w kupę zbutwiałych kości i rdzy. Z pustego hełmu dobiega westchnienie ulgi.",
        "Przeklęta zbroja pęka. To, co było w środku, wreszcie przestaje krzyczeć. {0} znika.",
        "{0} osuwa się, a ciemność, która go poruszała, ucieka szczelinami w podłodze.",
    };

    private static readonly string[] DivineKillLines =
    {
        "{0} pada, a jego ciało rozsypuje się w złoty pył, który parzy ci skórę.",
        "Boska krew {0} syczy na kamieniach. Coś w piramidzie budzi się i patrzy.",
    };

    private static readonly string[] CritLines =
    {
        "Cios krytyczny! Ostrze wchodzi aż po rękojeść.",
        "Cios krytyczny! Słyszysz trzask łamanych kości.",
        "Cios krytyczny! Krew bryzga ci na twarz.",
        "Cios krytyczny! Przeciwnik zatacza się, wypluwając zęby.",
    };

    private static readonly string[] IntroLines =
    {
        "Witaj w starożytnym Egipcie, w czasach, kiedy władzę sprawowała dynastia faraonów...",
        "W tajemniczej i magicznej piramidzie Chufu skrywają się niesamowite skarby oraz święty Graal...",
        "Jesteś poszukiwaczem przygód, który zdobył mapę wskazującą dokładne miejsce ukrycia Graala...",
        "Odnalezienie skarbu będzie nie lada wyzwaniem, ale jesteś zdeterminowany i gotowy, by podjąć to zadanie...",
        "Twoja wyprawa rozpoczyna się w odległym mieście portowym, oddalonym o tysiące kilometrów od celu...",
        "Nie masz pojęcia, jak się tam dostać, ale wierzysz w swoje umiejętności i szczęście...",
        "Odkryjesz sekrety piramidy, stoczysz niebezpieczne pojedynki i podejmiesz decyzje, które wpłyną na twoje dalsze losy...",
        "Czy uda ci się odnaleźć święty Graal? Przekonaj się sam...",
        "(Dowolny klawisz pomija pauzę, Esc pomija cały tekst.)",
    };

    private static readonly string[] PyramidHistoryLines =
    {
        "Po tygodniach podróży z karawaną stajesz przed potężną piramidą Chufu, która od wieków kusiła ludzi bogactwem i tajemnicami.",
        "Wchodzisz do jej wnętrza, w głąb mrocznych korytarzy, które kryją niezwykłe sekrety.",
        "Początkowo światło twojej pochodni jest silne, ale im dalej idziesz, tym słabiej świeci, odsłaniając jedynie kontury nieznanych przedmiotów.",
        "Czujesz pod stopami kamienne schody, a po bokach widzisz niezliczone korytarze, z których każdy może skrywać niebezpieczeństwo.",
        "Idziesz dalej, bo masz cel, który przyciąga cię mocniej niż złoto – święty Graal.",
        "W końcu dochodzisz do ogromnej komnaty, gdzie ściany zdobią malowidła przedstawiające legendy o piramidzie i jej władcach.",
        "Stoisz przed dwoma posągami w kształcie psów. Posągi otwierają oczy.",
        "A za nimi, w cieniu, czeka największa z zagadek, w której rozwiązaniu tkwi tajemnica nieśmiertelności.",
    };

    private static readonly string[] AfterGuardsLines =
    {
        "Anubisy padają pod twoimi ciosami, a ich ciała rozpadają się w drobny pył.",
        "Nagle czujesz, jak boska moc, która się z nich uwolniła, przywraca ci pełnię sił!",
        "Niespodziewanie tajemnicza postać o głowie sokoła, siedząca na tronie, wstaje.",
        "To sam bóg Ra, władca egipskich bogów, z którym musisz stoczyć ostateczną walkę!",
        "Twoje dłonie zaciskają się na broni. Czujesz, jak krew pulsuje ci w żyłach.",
        "Bóg Ra patrzy na ciebie surowym spojrzeniem, gotowy do walki.",
        "Przygotuj się do ostatniej walki, która zdecyduje o twoim losie oraz losie świętego Graala!",
    };

    private static readonly string[] EndingLines =
    {
        "Bóg Ra pada przed tobą...",
        "Widzisz, że za nim znajduje się skrzynia ze złota. W niej powinien być święty Graal...",
        "Podchodzisz do skrzyni i z trudem ją otwierasz...",
        "Blask z wnętrza oślepia cię na chwilę...",
        "Kiedy twoje oczy przyzwyczajają się do światła, widzisz, że w skrzyni są dziwne materiały i przedmioty, wykonane z czegoś, co przypomina złoto, ale nim nie jest...",
        "Możliwe, że pochodzą z innego świata, innej rzeczywistości, przez którą udało ci się przejść...",
        "Szukasz świętego Graala, ale go nie znajdujesz. Zamiast tego dostrzegasz coś, co przyciąga twoją uwagę – piękny sztylet o złotych zdobieniach, który pobłyskuje...",
        "Zabierasz go i kilka innych rzeczy, w tym mapę, która może poprowadzić cię do celu...",
        "Zrozumiesz ją na pewno, gdy nadarzy się odpowiednia okazja. Wiedząc, że przed tobą jeszcze wiele przygód, decydujesz się ruszyć w drogę...",
        "Może uda ci się odnaleźć święty Graal i spełnić swoje marzenia?...",
        "--- KONIEC GRY (na razie) ---",
    };

    private static readonly string[] BarmanBandits =
    {
        "\"Chłopie! Ostatnie kilka dni musieliśmy się chować przed bandytami.\"",
        "\"Ciągle nas atakują. Mam nadzieję, że w końcu przyjedzie kawaleria i zrobi z nimi porządek.\"",
        "\"Poza tym to, co zawsze. Czekamy, aż przypłynie statek z towarami.\"",
        "\"Najlepsze towary: whisky i rum... Ahh, tego mi trzeba!\"",
        "\"Na razie mam tylko czystą wodę źródlaną. Postawi cię na nogi, jak cię bandyci poturbują.\"",
        "\"Jeśli chcesz zarobić, złodzieje kręcą się tuż za bramą. Tylko uważaj na siebie.\"",
    };

    private static readonly string[] BarmanWolves =
    {
        "\"Ostatnio ataki bandytów się uspokoiły. Twoja robota, co?\"",
        "\"Słyszałem od ludzi, że w pobliskich lasach czają się wilki!\"",
        "\"Dziwne, dawno ich nie widziałem.\"",
        "\"A, przypłynął ten statek, o którym ci mówiłem.\"",
        "\"W końcu możesz skosztować najlepszej szkockiej whisky!\"",
    };

    private static readonly string[] BarmanCaravanAnnounced =
    {
        "\"O, to znowu ty. W mieście przybyło sporo nowych ludzi.\"",
        "\"Odkąd wilki i bandyci przestali atakować w pobliskim lesie, przejazd jest o wiele bezpieczniejszy.\"",
        "\"Od podróżników, którzy biesiadowali parę dni temu, dowiedziałem się, że niedługo przybędzie karawana.\"",
        "\"Mają zamiar jechać na pustynię szukać jakichś piramid. Jeśli chcesz się z nimi zabrać, zagadaj do mnie za jakiś czas.\"",
        "\"A, i jeszcze coś. Dostałem od nich specjalny trunek. Prosto z jakiegoś Malborka. Boję się tego spróbować.\"",
        "\"Jeśli będziesz chciał, mogę ci polać szklankę, ale pamiętaj: to nie są tanie rzeczy!\"",
    };

    private static readonly string[] BarmanCaravanReady =
    {
        "\"A to ty! Dawno cię nie widziałem.\"",
        "\"Sporo się zmieniło. Okolica stała się bardzo bezpieczna. Sporo nowych twarzy przewija się przez miasto!\"",
        "\"Wczoraj przybyła karawana. Okazuje się, że wyruszają pod samą piramidę Chufu!\"",
        "\"To prawdopodobnie jedyna okazja, aby się tam dostać. Mało kto zna drogę w tamte dalekie rejony.\"",
        "\"Stoją obok bramy wjazdowej do miasta. Niedługo wyruszają, jeśli chcesz jechać, śpiesz się!\"",
        "\"Miło się z tobą gadało. Mam nadzieję, że nasze drogi jeszcze się skrzyżują!\"",
    };

    private static readonly string[] BarmanDefault = { "Barman kiwa głową i wraca do polerowania szklanek." };

    private static readonly string[] SpecialDrinkLines =
    {
        "\"Specjalność prosto od krzyżaków z Malborka. Miód pitny zwany Grunwald!\"",
        "Pijesz legendarny miód, który podobno stał na stołach biesiadnych przed bitwą pod Grunwaldem.",
        "Świat wiruje. Kiedy dochodzisz do siebie, czujesz się silniejszy niż kiedykolwiek.",
    };

    private static readonly string[] NightCompanyLines =
    {
        "Dziewczyna o oczach koloru pustynnego nieba bierze cię za rękę i prowadzi po skrzypiących schodach.",
        "Drzwi się zamykają. Lampa gaśnie. Reszta nocy należy tylko do was dwojga.",
    };

    public static string NoGold(IRandomSource rng) => rng.Pick(NoGoldLines);

    public static string TalkToTheGirls(IRandomSource rng) => rng.Pick(GirlsLines);

    public static string CritDescription(IRandomSource rng) => rng.Pick(CritLines);

    public static string KillDescription(Enemy enemy, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        string[] pool = enemy.Kind switch
        {
            EnemyKind.Beast => BeastKillLines,
            EnemyKind.Undead => UndeadKillLines,
            EnemyKind.Divine => DivineKillLines,
            _ => HumanKillLines,
        };
        return string.Format(CultureInfo.InvariantCulture, rng.Pick(pool), enemy.Name);
    }

    public static IReadOnlyList<string> Intro => IntroLines;

    public static IReadOnlyList<string> PyramidHistory => PyramidHistoryLines;

    public static IReadOnlyList<string> AfterGuards => AfterGuardsLines;

    public static IReadOnlyList<string> Ending => EndingLines;

    public static IReadOnlyList<string> SpecialDrink => SpecialDrinkLines;

    public static IReadOnlyList<string> NightCompany => NightCompanyLines;

    public static IReadOnlyList<string> Barman(StoryStage stage) => stage switch
    {
        StoryStage.BanditsCalmed => BarmanBandits,
        StoryStage.WolvesCleared => BarmanWolves,
        StoryStage.CaravanAnnounced => BarmanCaravanAnnounced,
        StoryStage.CaravanReady => BarmanCaravanReady,
        _ => BarmanDefault,
    };
}
