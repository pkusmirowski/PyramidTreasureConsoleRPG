namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Zadania od NPC. Główna linia (barman) przesuwa etapy fabuły; poboczne budują reputację.</summary>
public static class QuestCatalog
{
    private static readonly QuestDefinition[] Quests =
    [
        new(
            QuestId.Bandits, QuestGiverId.Barman, "Bandyci za bramą",
            [
                "\"Chłopie! Ostatnie kilka dni musieliśmy się chować przed bandytami.\"",
                "\"Ciągle nas atakują. Kawaleria nie przyjedzie, a straż woli pić niż walczyć.\"",
                "\"Jeśli chcesz zarobić, złodzieje kręcą się tuż za bramą. Przetrzep sześciu, a miasto odetchnie.\"",
                "\"Na razie mam tylko czystą wodę źródlaną. Postawi cię na nogi, jak cię poturbują.\"",
            ],
            new KillObjective(EnemyCatalog.Thief, 6),
            new QuestReward(Gold: 40, Exp: 300, Faction.Town, 10, StoryStage.BanditsCalmed),
            [
                "\"Sześciu? Ludzie gadają, że siedmiu, ale nie będę się kłócił.\"",
                "\"Droga do Starego Miasta i delty jest twoja. Tylko uważaj, w delcie od tygodnia wyją wilki.\"",
            ],
            IsMain: true),
        new(
            QuestId.Wolves, QuestGiverId.Barman, "Wilki w delcie",
            [
                "\"Ostatnio ataki bandytów się uspokoiły. Twoja robota, co?\"",
                "\"Słyszałem od ludzi, że w delcie czają się wilki. Karawany nie chcą tamtędy jechać.\"",
                "\"Wybij pięć, a kupcy zapłacą. A, przypłynął ten statek, o którym mówiłem. Whisky jest.\"",
            ],
            new KillObjective(EnemyCatalog.Wolf, 5),
            new QuestReward(Gold: 80, Exp: 800, Faction.Town, 10, StoryStage.WolvesCleared),
            [
                "\"Pięć wilczych łbów. Kupcy już piją na twoje zdrowie.\"",
                "\"Skoro delta jest czysta, Szlak Karawan stoi otworem. Tam kończy się cywilizacja, zaczynają rycerze bez twarzy.\"",
            ],
            RequiredStage: StoryStage.BanditsCalmed, RequiresCompleted: QuestId.Bandits, IsMain: true),
        new(
            QuestId.CaravanTrail, QuestGiverId.Barman, "Szlak karawan",
            [
                "\"O, to znowu ty. W mieście przybyło sporo nowych ludzi, odkąd delta jest bezpieczna.\"",
                "\"Od podróżników dowiedziałem się, że niedługo przybędzie karawana. Chcą jechać na pustynię szukać piramid.\"",
                "\"Problem w tym, że na szlaku stoją upadli rycerze. Rozwal czterech, a karawana ruszy.\"",
                "\"A, i jeszcze coś. Dostałem od nich specjalny trunek. Prosto z jakiegoś Malborka. Boję się tego spróbować.\"",
            ],
            new KillObjective(EnemyCatalog.FallenKnight, 4),
            new QuestReward(Gold: 150, Exp: 2000, Faction.Town, 15, StoryStage.CaravanAnnounced),
            [
                "\"Czterech. Karawaniarze mówią, że szlak jest przejezdny aż do Oazy Siwa.\"",
                "\"W oazie mieszka kapłanka Neferet. Podobno wie, jak wejść do piramidy i wyjść. Pogadaj z nią.\"",
            ],
            RequiredStage: StoryStage.WolvesCleared, RequiresCompleted: QuestId.Wolves, IsMain: true),
        new(
            QuestId.Oasis, QuestGiverId.Barman, "Kapłanka z oazy",
            [
                "\"Karawana czeka na sygnał z oazy. Bez błogosławieństwa kapłanki nikt nie ruszy pod piramidę.\"",
                "\"Dotrzyj do Oazy Siwa, znajdź świątynię i pogadaj z Neferet. Potem wracaj, a wyruszycie.\"",
            ],
            new FlagObjective("priestess:met", "Porozmawiaj z kapłanką Neferet w świątyni w Oazie Siwa"),
            new QuestReward(Gold: 200, Exp: 3000, Faction.Town, 10, StoryStage.CaravanReady),
            [
                "\"A to ty! Sporo się zmieniło. Karawana stoi przy bramie i wyrusza pod samą piramidę Chufu!\"",
                "\"Mało kto zna drogę w tamte rejony. Jeśli chcesz jechać, śpiesz się. Miło się z tobą gadało.\"",
            ],
            RequiredStage: StoryStage.CaravanAnnounced, RequiresCompleted: QuestId.CaravanTrail, IsMain: true),
        new(
            QuestId.CaptainSmugglers, QuestGiverId.Captain, "Przemytnicy w Starym Mieście",
            [
                "Kapitan portu nie podnosi wzroku znad papierów. \"Hasan i jego ludzie w kradzionych zbrojach okradają moje magazyny.\"",
                "\"Straż się boi. Ty wyglądasz, jakbyś się nie bał. Czterech opancerzonych, sto dwadzieścia sztuk złota.\"",
            ],
            new KillObjective(EnemyCatalog.ArmoredThief, 4),
            new QuestReward(Gold: 120, Exp: 1000, Faction.Town, 20),
            [
                "\"Czterech. Hasan będzie wściekły.\" Kapitan pierwszy raz się uśmiecha. \"Dobrze.\"",
            ],
            RequiredStage: StoryStage.BanditsCalmed),
        new(
            QuestId.SmugglerCargo, QuestGiverId.Smuggler, "Paczka dla myśliwego",
            [
                "Hasan waży w dłoni zawiniątko. \"Nie pytaj, co w środku. Zanieś do delty, myśliwemu przy ognisku.\"",
                "\"Kapitan portu chciałby to zobaczyć. Dlatego ty, nie ja. Dziewięćdziesiąt sztuk po powrocie.\"",
            ],
            new ReachObjective(RegionId.Delta),
            new QuestReward(Gold: 90, Exp: 600, Faction.Underworld, 20),
            [
                "\"Myśliwy dostał? Dobrze. Kapitan nie musi wiedzieć, że w ogóle rozmawialiśmy.\"",
            ],
            RequiredStage: StoryStage.BanditsCalmed, RequiresFaction: Faction.Underworld, MinReputation: -20),
        new(
            QuestId.PriestessMonks, QuestGiverId.Priestess, "Uwolnij braci",
            [
                "Neferet patrzy na ciebie długo, zanim odezwie się szeptem.",
                "\"Płaczący Mnisi w ruinach to nasi bracia, którzy weszli w ogień i nie wrócili. Nie da się ich wyleczyć.\"",
                "\"Da się ich uwolnić. Trzech. Zrób to, a Bractwo uzna cię za swojego.\"",
            ],
            new KillObjective(EnemyCatalog.CryingMonk, 3),
            new QuestReward(Gold: 250, Exp: 4000, Faction.Brotherhood, 25),
            [
                "\"Trzech braci odpoczywa. Bractwo pamięta o tych, którzy niosą ulgę.\"",
            ],
            RequiredStage: StoryStage.CaravanAnnounced),
    ];

    public static IReadOnlyList<QuestDefinition> All => Quests;

    public static QuestDefinition Get(QuestId id) => Quests.First(q => q.Id == id);

    public static IEnumerable<QuestDefinition> ByGiver(QuestGiverId giver) => Quests.Where(q => q.Giver == giver);
}
