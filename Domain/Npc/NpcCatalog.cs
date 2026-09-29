namespace PyramidTreasureConsoleRPG.Domain;

public enum NpcId
{
    Zoja,
    Ptaszek,
    Neferet,
}

/// <summary>Opcja w rozmowie: tekst, wymagania, skutki i węzeł, do którego prowadzi (null = koniec rozmowy).</summary>
public sealed record DialogueOption(
    string Text,
    IReadOnlyList<EventEffect> Effects,
    string? Next,
    string? RequiresFlag = null,
    int GoldCost = 0,
    Faction? RequiresFaction = null,
    int RequiredReputation = 0,
    string? ResultText = null);

public sealed record DialogueNode(string Id, IReadOnlyList<string> Text, IReadOnlyList<DialogueOption> Options);

/// <summary>
/// Postać z własnym wątkiem. Wejście do rozmowy wybiera pierwszy węzeł z listy Entry, którego flaga
/// wymagana jest spełniona (kolejność od najdalszego etapu wątku do początku).
/// </summary>
public sealed record NpcDefinition(
    NpcId Id,
    string Name,
    string Description,
    RegionId Region,
    IReadOnlyList<(string? RequiresFlag, string? ForbidsFlag, string NodeId)> Entry,
    IReadOnlyList<DialogueNode> Nodes)
{
    public DialogueNode Node(string id) => Nodes.First(n => n.Id == id);
}

/// <summary>Trzy postacie na górze tawerny: uciekinierka, złodziejka i kapłanka. Sceny intymne za zamkniętymi drzwiami.</summary>
public static class NpcCatalog
{
    public static NpcDefinition Zoja { get; } = new(
        NpcId.Zoja,
        "Zoja",
        "Uciekinierka z wioski spalonej przez bandytów. Nosi nóż w bucie i nie odwraca wzroku.",
        RegionId.Port,
        [
            ("zoja:night", null, "zoja_after"),
            ("zoja:revenge", null, "zoja_night"),
            ("zoja:met", null, "zoja_revenge"),
            (null, null, "zoja_intro"),
        ],
        [
            new("zoja_intro",
                [
                    "Zoja siedzi na parapecie i patrzy na port. \"Ty jesteś ten, co tłucze złodziei za bramą? Widziałam.\"",
                    "\"Moja wioska to była godzina drogi stąd. Bandyci przyszli w nocy. Ja mam nóż, oni mieli pochodnie.\"",
                ],
                [
                    new("\"Kto ich prowadził?\"", [new FlagEffect("zoja:met"), new ExpEffect(100)], "zoja_intro_2"),
                    new("\"Współczuję.\" (odejdź)", [new FlagEffect("zoja:met")], null, ResultText: "\"Współczucie nie odbudowuje domów.\" Odwraca się do okna."),
                ]),
            new("zoja_intro_2",
                [
                    "\"Blizna przez pół twarzy, kradziona zbroja. Hasan wie, kto to, ale Hasan sprzedaje tylko za złoto albo krew.\"",
                    "\"Jeśli spotkasz opancerzonych w Starym Mieście, to jego ludzie. Zabij czterech, a przyjdź mi powiedzieć.\"",
                ],
                [
                    new("\"Wrócę.\"", [], null, ResultText: "Kiwa głową. Nóż w bucie nie drgnął ani razu."),
                ]),
            new("zoja_revenge",
                [
                    "Zoja czeka na schodach. \"Zabiłeś opancerzonych. Widziałam, jak Hasan blednie.\"",
                ],
                [
                    new("\"Czterech. Tak jak chciałaś.\" (wymaga ukończonego zadania kapitana)", [new FlagEffect("zoja:revenge"), new ReputationEffect(Faction.Town, 5), new ExpEffect(300)], "zoja_revenge_2", RequiresFlag: "quest:CaptainSmugglers"),
                    new("\"Jeszcze nie.\"", [], null, ResultText: "\"To po co przyszedłeś?\""),
                ]),
            new("zoja_revenge_2",
                [
                    "Bierze twoją dłoń i przykłada do swojej blizny na przedramieniu. \"Ta jest od nich. Reszta zostanie.\"",
                    "\"Zostań dziś na noc. Nie z litości. Bo chcę.\"",
                ],
                [
                    new("Zostań", [new FlagEffect("zoja:night"), new HpPercentEffect(100)], null, ResultText: "Drzwi się zamykają. Lampa gaśnie. Rano Zoja śpi z nożem pod poduszką, a ty czujesz, że możesz przenosić góry."),
                    new("Odmów", [new FlagEffect("zoja:declined"), new ReputationEffect(Faction.Town, 2)], null, ResultText: "\"Uczciwy. Ci giną pierwsi.\" Ale się uśmiecha."),
                ]),
            new("zoja_night",
                [
                    "Zoja rzuca ci jabłko. \"Zabiłeś ich, a nie zostałeś. Nie wiem, czy to szacunek, czy głupota.\"",
                ],
                [
                    new("\"Może jedno i drugie.\"", [new ExpEffect(50)], null, ResultText: "\"Wróć, jak zdecydujesz.\""),
                ]),
            new("zoja_after",
                [
                    "Zoja czyści nóż. \"Karawaniarze gadali w nocy. W pustyni jest studnia, przy której koczownicy handlują mapami.\"",
                ],
                [
                    new("\"Dzięki.\"", [new FlagEffect("rumor:desert"), new ExpEffect(80)], null, ResultText: "\"Wróć żywy. To rozkaz.\""),
                ]),
        ]);

    public static NpcDefinition Ptaszek { get; } = new(
        NpcId.Ptaszek,
        "Ptaszek",
        "Złodziejka o palcach szybszych niż jej język. Podobno nikt nie wyszedł od niej z pełną sakwą.",
        RegionId.Port,
        [
            ("ptaszek:robbed", null, "ptaszek_after"),
            ("ptaszek:met", null, "ptaszek_offer"),
            (null, null, "ptaszek_intro"),
        ],
        [
            new("ptaszek_intro",
                [
                    "Ptaszek siedzi na stole i liczy cudze monety. \"Wędrowiec z pełną sakwą. Rzadki ptak.\"",
                    "\"Za trzydzieści sztuk pokażę ci coś, czego nie pokazuję nikomu.\"",
                ],
                [
                    new("Zapłać 30 sztuk złota", [new FlagEffect("ptaszek:met"), new ExpEffect(60)], "ptaszek_show", GoldCost: 30),
                    new("\"Nie mam trzydziestu.\"", [new FlagEffect("ptaszek:met")], null, ResultText: "\"To wróć, jak będziesz miał.\" Jej ręka jest już w twojej kieszeni, ale nic tam nie ma."),
                ]),
            new("ptaszek_show",
                [
                    "Pokazuje ci, jak zdejmuje sakiewkę z pasa strażnika, który nawet nie przestał chrapać.",
                    "\"Lekcja pierwsza: patrz na oczy, nie na ręce. Lekcja druga: ja nie daję lekcji.\"",
                ],
                [
                    new("\"Warto było.\"", [], null, ResultText: "Puszcza oko. Twoja sakwa jest o pięć monet lżejsza. Nie zauważyłeś kiedy."),
                ]),
            new("ptaszek_offer",
                [
                    "\"Wróciłeś. Mam pokój na górze i noc do zabicia. Ty masz sakwę. Coś wymyślimy.\"",
                ],
                [
                    new("Idź na górę", [new FlagEffect("ptaszek:robbed"), new GoldEffect(-40), new HpPercentEffect(100)], null, ResultText: "Noc jest długa i zaskakująco czuła. Rano Ptaszka nie ma, a w sakwie brakuje czterdziestu sztuk. Na poduszce leży zapisany na skrawku adres skrytki w Starym Mieście."),
                    new("\"Znam cię. Nie.\"", [new ReputationEffect(Faction.Underworld, 3)], null, ResultText: "\"Mądry. Nudny, ale mądry.\""),
                ]),
            new("ptaszek_after",
                [
                    "Ptaszek udaje, że cię nie zna. Potem parska śmiechem. \"Skrytka była prawdziwa. Sprawdziłeś?\"",
                ],
                [
                    new("\"Jeszcze nie.\"", [new FlagEffect("rumor:oldtown"), new ExpEffect(80)], null, ResultText: "\"Za paserem, trzecie drzwi. Powiedz, że od Ptaszka. Albo nie mów.\""),
                ]),
        ]);

    public static NpcDefinition Neferet { get; } = new(
        NpcId.Neferet,
        "Neferet",
        "Kapłanka Bractwa. Wieczorami zdejmuje kaptur i pije wino, którego nie wolno jej pić.",
        RegionId.Oasis,
        [
            ("neferet:vow", null, "neferet_after"),
            ("neferet:wine", null, "neferet_vow"),
            (null, null, "neferet_intro"),
        ],
        [
            new("neferet_intro",
                [
                    "Neferet bez kaptura wygląda młodziej. \"W świątyni jestem kapłanką. Tutaj jestem kobietą, która nie może spać.\"",
                    "\"Wiesz, co jest w piramidzie? Nie Graal. Coś, co udaje Graala i zjada tych, którzy go dotkną.\"",
                ],
                [
                    new("Napij się z nią wina", [new FlagEffect("neferet:wine"), new ExpEffect(150), new ReputationEffect(Faction.Brotherhood, 5)], null, ResultText: "Wino jest ciężkie i słodkie. Mówi o Płaczących Mnichach, którzy byli jej braćmi, i o ogniu, w który weszli. Nie płacze. Już nie umie."),
                    new("\"Bractwo mnie nie obchodzi.\"", [new ReputationEffect(Faction.Brotherhood, -5)], null, ResultText: "\"Obchodzić cię będzie, gdy staniesz przed tronem.\" Naciąga kaptur."),
                ]),
            new("neferet_vow",
                [
                    "\"Bractwo ślubuje wstrzemięźliwość. Ja ślubowałam dawno i źle.\" Patrzy na ciebie długo.",
                    "\"Zostań. Jutro znów będę kapłanką.\"",
                ],
                [
                    new("Zostań", [new FlagEffect("neferet:vow"), new HpPercentEffect(100), new ReputationEffect(Faction.Brotherhood, 10)], null, ResultText: "Nie ma lampy do zgaszenia, tylko księżyc nad oazą. Rano Neferet stoi już w kapturze, ale zostawia ci amulet z płaczącą twarzą: \"Ra pozna, że jesteś mój. Wahnie się. To wystarczy.\"", RequiresFaction: Faction.Brotherhood, RequiredReputation: 10),
                    new("Odmów", [new ReputationEffect(Faction.Brotherhood, 2)], null, ResultText: "\"Dobrze. Jeden z nas musi dotrzymać ślubów.\""),
                ]),
            new("neferet_after",
                [
                    "Neferet nie zdejmuje już kaptura przy tobie. \"Kiedy staniesz przed skrzynią, nie bierz tego, co błyszczy. Weź to, co płacze.\"",
                ],
                [
                    new("\"Zapamiętam.\"", [new FlagEffect("ending:brotherhood_hint"), new ExpEffect(100)], null, ResultText: "\"Nie zapamiętasz. Nikt nie pamięta. Ale spróbuj.\""),
                ]),
        ]);

    public static IReadOnlyList<NpcDefinition> All { get; } = [Zoja, Ptaszek, Neferet];

    public static IEnumerable<NpcDefinition> InRegion(RegionId region) => All.Where(n => n.Region == region);
}
