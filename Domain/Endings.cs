namespace PyramidTreasureConsoleRPG.Domain;

public enum EndingKind
{
    /// <summary>Zabrać Graal: moc i klątwa.</summary>
    TakeGrail = 1,

    /// <summary>Oddać Graal Bractwu Płaczących Mnichów.</summary>
    GiveToBrotherhood = 2,

    /// <summary>Zniszczyć Graal w komnacie pod tronem (mapa koczowników).</summary>
    Destroy = 3,
}

/// <summary>Zakończenie: warunek dostępności opisuje tekst, a sprawdza go EndingEngine.</summary>
public sealed record EndingDefinition(
    EndingKind Kind,
    string Title,
    string Choice,
    string Requirement,
    IReadOnlyList<string> Text,
    string NewGamePlusBonus);

/// <summary>Trzy zakończenia po pokonaniu Ra. Treść w jednym miejscu, żeby test pilnował spójności.</summary>
public static class EndingCatalog
{
    public static EndingDefinition TakeGrail { get; } = new(
        EndingKind.TakeGrail,
        "Graal jest mój",
        "Zabierz Graal",
        "zawsze dostępne",
        [
            "Podnosisz kielich. Jest cięższy, niż wygląda, i ciepły jak żywe ciało.",
            "Rany zasklepiają się na twoich oczach. Blizny znikają. Czujesz, że mógłbyś zburzyć tę piramidę gołymi rękami.",
            "W nocy, w oazie, śni ci się Ra. Nie jest zły. Uśmiecha się, jakby wiedział coś, czego ty się dopiero dowiesz.",
            "Wracasz do portu z kielichem w sakwie i z twarzą, której barman nie poznaje. Kobiety patrzą dłużej. Mężczyźni schodzą z drogi.",
            "Sen o Ra wraca każdej nocy. Zawsze się uśmiecha. Zawsze jest bliżej.",
            "--- KONIEC: GRAAL JEST MÓJ ---",
        ],
        "Nowa gra+: +2 do wszystkich atrybutów, ale koszmary przez pierwsze 20 nocy.");

    public static EndingDefinition GiveToBrotherhood { get; } = new(
        EndingKind.GiveToBrotherhood,
        "Ofiara Bractwa",
        "Oddaj Graal Bractwu Płaczących Mnichów",
        "reputacja Bractwa ≥ 30 albo słowo dane Neferet",
        [
            "Za tronem stoją już mnisi. Nie wiesz, jak weszli. Może byli tu zawsze.",
            "Neferet zdejmuje kaptur. Nie płacze. Bierze kielich z twoich rąk tak, jak bierze się dziecko od obcego.",
            "\"Będzie spał w studni pod oazą. Dopóki ktoś taki jak ty nie przyjdzie po niego znowu.\"",
            "W porcie kapłanka Bractwa wita cię jak swojego. Karczmarz nalewa za darmo. Kapitan portu udaje, że cię nie widzi.",
            "Nocą w oazie ktoś zostawia pod drzwiami amulet z płaczącą twarzą i jedno zdanie na skrawku: \"Dotrzymałeś.\"",
            "--- KONIEC: OFIARA BRACTWA ---",
        ],
        "Nowa gra+: Bractwo od początku ci ufa (+40), a Neferet czeka w oazie z darem.");

    public static EndingDefinition Destroy { get; } = new(
        EndingKind.Destroy,
        "Popiół pod tronem",
        "Zejdź do komnaty z mapy koczowników i zniszcz Graal",
        "mapa koczowników z Szlaku Karawan",
        [
            "Mapa koczowników prowadzi za tron, pod płytę, której nie ruszył nikt od tysiąca lat.",
            "W komnacie pali się ogień, który nie potrzebuje drewna. Kielich w nim nie topi się – krzyczy.",
            "Kiedy milknie, ogień gaśnie, a piramida jęczy jak stary człowiek, któremu w końcu pozwolono umrzeć.",
            "Wychodzisz z pustymi rękami. Nikt ci nie uwierzy. Legendy będą opowiadać, że Graal wciąż tam jest.",
            "Ale Ra już nikomu się nie przyśni. I to jest twoja zapłata.",
            "--- KONIEC: POPIÓŁ POD TRONEM ---",
        ],
        "Nowa gra+: Miasto pamięta (+40 reputacji), a w sakwie zostaje ci mapa: 300 sztuk złota na start.");

    public static IReadOnlyList<EndingDefinition> All { get; } = [TakeGrail, GiveToBrotherhood, Destroy];

    public static EndingDefinition Get(EndingKind kind) => All.First(e => e.Kind == kind);
}
