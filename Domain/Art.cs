namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Jedna linia rysunku ASCII z kolorem.</summary>
public sealed record ArtLine(string Text, ConsoleColor Color);

/// <summary>Rysunek ASCII do konsoli: kilka linii, każda w swoim kolorze.</summary>
public sealed record AsciiArt(string Name, IReadOnlyList<ArtLine> Lines)
{
    public int Width => Lines.Count == 0 ? 0 : Lines.Max(l => l.Text.Length);
}

/// <summary>Katalog rysunków: tytuł, klasy, regiony, wrogowie, tawerna, kasyno, śmierć i Graal.</summary>
public static class ArtCatalog
{
    private const ConsoleColor Sand = ConsoleColor.DarkYellow;
    private const ConsoleColor Gold = ConsoleColor.Yellow;
    private const ConsoleColor Stone = ConsoleColor.Gray;
    private const ConsoleColor Dark = ConsoleColor.DarkGray;
    private const ConsoleColor Blood = ConsoleColor.Red;
    private const ConsoleColor Sky = ConsoleColor.Cyan;
    private const ConsoleColor Water = ConsoleColor.Blue;
    private const ConsoleColor Leaf = ConsoleColor.Green;
    private const ConsoleColor Bone = ConsoleColor.White;
    private const ConsoleColor Magic = ConsoleColor.Magenta;

    public static AsciiArt Title { get; } = new("Tytuł",
    [
        new("            .                                              ", Gold),
        new("           /_\\        P Y R A M I D   T R E A S U R E    ", Sand),
        new("          /___\\                                            ", Sand),
        new("         /_____\\      konsolowe RPG o Graalu w piramidach ", Sand),
        new("        /_______\\                                          ", Sand),
        new("  _____/_________\\_____________________________________    ", Dark),
        new(" ~  ~   ~  ~   ~   ~   ~   ~   ~   ~   ~   ~   ~   ~   ~   ", Sand),
    ]);

    public static AsciiArt Warrior { get; } = new("Wojownik",
    [
        new("    ,^.     ", Stone),
        new("   [o o]  |\\", Bone),
        new("   /|=|\\  |=\\", Stone),
        new("  / |=| \\ |  \\", Stone),
        new("    |=|   |   ", Stone),
        new("   _/ \\_  |   ", Dark),
    ]);

    public static AsciiArt Archer { get; } = new("Łucznik",
    [
        new("     ,^.   )   ", Leaf),
        new("    (o o) /|   ", Bone),
        new("    /|-|\\/ |   ", Leaf),
        new("   / |-|   |   ", Leaf),
        new("     |-|  /    ", Leaf),
        new("    _/ \\_)     ", Dark),
    ]);

    public static AsciiArt Assassin { get; } = new("Asasyn",
    [
        new("     _^_       ", Dark),
        new("    (o_o)  /   ", Bone),
        new("    /|~|\\ /    ", Dark),
        new("   / |~| /     ", Dark),
        new("     |~|       ", Dark),
        new("    _/ \\_      ", Dark),
    ]);

    public static AsciiArt Port { get; } = new("Port Sokoła",
    [
        new("            |    |                      ", Stone),
        new("           )_)  )_)  )_)                ", Bone),
        new("          )___))___))___)\\              ", Bone),
        new("         )____)____)_____)\\\\            ", Bone),
        new("       _____|____|____|____\\\\__         ", Dark),
        new("  -----\\                   /-------     ", Dark),
        new("  ^^^^^ ^^^^^^^^^^^^^^^^^^^^^ ^^^^^^^   ", Water),
        new("    ^^^^      ^^^^     ^^^    ^^        ", Water),
    ]);

    public static AsciiArt OldTown { get; } = new("Stare Miasto",
    [
        new("   _|_|_|_    _|_|_|_    _|_|_|_        ", Stone),
        new("   |  _  |    | [ ] |    |  _  |        ", Stone),
        new("   | | | |  __| [ ] |__  | | | |        ", Stone),
        new("   |_|_|_|  |_|_|_|_|_|  |_|_|_|        ", Dark),
        new("      ~ zaułki, noże i długi ~          ", Dark),
    ]);

    public static AsciiArt Delta { get; } = new("Delta i las",
    [
        new("      /\\      /\\   /\\        /\\         ", Leaf),
        new("     /  \\    /  \\ /  \\      /  \\        ", Leaf),
        new("    /    \\  /    V    \\    /    \\       ", Leaf),
        new("      ||      ||   ||        ||         ", Sand),
        new("  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  ", Water),
        new("    ~~  (wilki wyją za rzeką)  ~~       ", Water),
    ]);

    public static AsciiArt Desert { get; } = new("Szlak Karawan",
    [
        new("                 \\ | /                  ", Gold),
        new("               -- (O) --                ", Gold),
        new("                 / | \\                  ", Gold),
        new("     _.-~-._                  _.-~-._   ", Sand),
        new("  .-~       ~-._  _.-~~-._  .~       ~-.", Sand),
        new(" ~   ,     ,    ~~        ~~   '   .    ", Sand),
    ]);

    public static AsciiArt Oasis { get; } = new("Oaza Siwa",
    [
        new("        _\\|/_       ", Leaf),
        new("      _\\/ | \\/_     ", Leaf),
        new("        \\/|\\/       ", Leaf),
        new("          |         ", Sand),
        new("   ~~~~~~~|~~~~~~~  ", Water),
        new("  ~  ~   ~ ~  ~  ~  ", Water),
    ]);

    public static AsciiArt Pyramid { get; } = new("Piramida Chufu",
    [
        new("                .                   ", Gold),
        new("               / \\                  ", Sand),
        new("              /   \\                 ", Sand),
        new("             / [_] \\                ", Sand),
        new("            /       \\               ", Sand),
        new("           /    _    \\              ", Sand),
        new("          /____/ \\____\\             ", Dark),
        new("  ~   ~   ~   ~   ~   ~   ~   ~   ~ ", Sand),
    ]);

    public static AsciiArt Human { get; } = new("Złodziej",
    [
        new("   ___    ", Dark),
        new("  (o o)   ", Bone),
        new("  /|-|\\ / ", Dark),
        new("   |-| /  ", Dark),
        new("   / \\    ", Dark),
    ]);

    public static AsciiArt Beast { get; } = new("Bestia",
    [
        new("   /\\_/\\      ", Sand),
        new("  ( o.o )__   ", Sand),
        new("   >-^-<   \\  ", Sand),
        new("  /|   |\\  |  ", Sand),
        new("   |   | _/   ", Dark),
    ]);

    public static AsciiArt Undead { get; } = new("Nieumarły",
    [
        new("    .-.     ", Bone),
        new("   (x x)    ", Bone),
        new("   /|#|\\ /  ", Stone),
        new("    |#| /   ", Stone),
        new("   _| |_    ", Dark),
    ]);

    public static AsciiArt Anubis { get; } = new("Anubis",
    [
        new("    /\\  /\\    ", Dark),
        new("   /  \\/  \\   ", Dark),
        new("   | o  o |   ", Gold),
        new("    \\_/\\_/    ", Dark),
        new("   /|====|\\   ", Gold),
        new("    |====|    ", Gold),
        new("   _|    |_   ", Dark),
    ]);

    public static AsciiArt Ra { get; } = new("Ra",
    [
        new("        \\  |  /        ", Gold),
        new("      -- (   ) --      ", Gold),
        new("        /  |  \\        ", Gold),
        new("        ( o o )        ", Blood),
        new("      /|=======|\\      ", Gold),
        new("     / |=======| \\     ", Gold),
        new("    ^  |=======|  ^    ", Magic),
        new("      _|       |_      ", Dark),
    ]);

    public static AsciiArt Tavern { get; } = new("Tawerna",
    [
        new("     _____     ", Sand),
        new("    |~~~~~|_   ", Bone),
        new("    |     | |  ", Sand),
        new("    |     |_|  ", Sand),
        new("    |_____|    ", Sand),
    ]);

    public static AsciiArt Casino { get; } = new("Kasyno",
    [
        new("   .-----.  .-----.  ", Bone),
        new("   | o   |  | o o |  ", Blood),
        new("   |  o  |  |     |  ", Blood),
        new("   |   o |  | o o |  ", Blood),
        new("   '-----'  '-----'  ", Bone),
    ]);

    public static AsciiArt Death { get; } = new("Śmierć",
    [
        new("      _____      ", Bone),
        new("     /     \\     ", Bone),
        new("    | () () |    ", Bone),
        new("     \\  ^  /     ", Bone),
        new("      |||||      ", Bone),
        new("      |||||      ", Dark),
    ]);

    public static AsciiArt Grail { get; } = new("Graal",
    [
        new("      \\ | /      ", Gold),
        new("     \\_____/     ", Gold),
        new("      \\   /      ", Gold),
        new("       \\_/       ", Gold),
        new("        |        ", Gold),
        new("       _|_       ", Sand),
        new("      /___\\      ", Sand),
    ]);

    public static IReadOnlyList<AsciiArt> All { get; } =
        [Title, Warrior, Archer, Assassin, Port, OldTown, Delta, Desert, Oasis, Pyramid, Human, Beast, Undead, Anubis, Ra, Tavern, Casino, Death, Grail];

    public static AsciiArt ForClass(HeroClass heroClass) => heroClass switch
    {
        HeroClass.Archer => Archer,
        HeroClass.Assassin => Assassin,
        _ => Warrior,
    };

    public static AsciiArt ForRegion(RegionId region) => region switch
    {
        RegionId.OldTown => OldTown,
        RegionId.Delta => Delta,
        RegionId.Desert => Desert,
        RegionId.Oasis => Oasis,
        RegionId.Pyramid => Pyramid,
        _ => Port,
    };

    /// <summary>Portret pierwszego wroga w walce: bossowie mają własne, reszta wg rodzaju.</summary>
    public static AsciiArt ForEnemy(EnemyDefinition enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        if (enemy == EnemyCatalog.Ra)
        {
            return Ra;
        }

        if (enemy == EnemyCatalog.Anubis || enemy == EnemyCatalog.ShadowAnubis)
        {
            return Anubis;
        }

        return enemy.Kind switch
        {
            EnemyKind.Beast => Beast,
            EnemyKind.Undead or EnemyKind.Divine => Undead,
            _ => Human,
        };
    }
}
