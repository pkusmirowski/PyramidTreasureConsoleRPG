using Microsoft.Extensions.DependencyInjection;
using PyramidTreasureConsoleRPG.Domain;
using PyramidTreasureConsoleRPG.Infrastructure;
using PyramidTreasureConsoleRPG.Ui;

string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GreatPyramidTreasureRPG_DataSave");
string musicPath = Path.Combine(AppContext.BaseDirectory, "Audio", "ancient_egypt.wav");

// Spectre.Console potrzebuje prawdziwego terminala; w potoku (bot, CI) lub na życzenie (--plain) używamy gołej konsoli.
bool plainConsole = args.Any(a => string.Equals(a, "--plain", StringComparison.OrdinalIgnoreCase)) || !SpectreGameIO.IsSupported;

var services = new ServiceCollection();
if (plainConsole)
{
    services.AddSingleton<IGameIO, ConsoleGameIO>();
}
else
{
    services.AddSingleton<IGameIO, SpectreGameIO>();
}
// --seed N: powtarzalna rozgrywka (bot w CI, odtwarzanie błędów).
int seedIndex = Array.FindIndex(args, a => string.Equals(a, "--seed", StringComparison.OrdinalIgnoreCase));
if (seedIndex >= 0 && seedIndex + 1 < args.Length && int.TryParse(args[seedIndex + 1], out int seed))
{
    services.AddSingleton<IRandomSource>(new SeededRandomSource(seed));
}
else
{
    services.AddSingleton<IRandomSource, SystemRandomSource>();
}
services.AddSingleton<ISaveStore>(new JsonFileSaveStore(dataFolder));
services.AddSingleton<ISettingsStore>(new JsonSettingsStore(dataFolder));
services.AddSingleton(sp => sp.GetRequiredService<ISettingsStore>().Load());
services.AddSingleton<IMusicPlayer>(_ => OperatingSystem.IsWindows() ? new NAudioMusicPlayer(musicPath) : (IMusicPlayer)new NullMusicPlayer());
services.AddSingleton<PyramidTreasureConsoleRPG.Engine.RestService>();
services.AddSingleton<CombatScreen>();
services.AddSingleton<InventoryScreen>();
services.AddSingleton<ShopScreen>();
services.AddSingleton<BarScreen>();
services.AddSingleton<CasinoScreen>();
services.AddSingleton<NpcScreen>();
services.AddSingleton<RestScreen>();
services.AddSingleton<TavernScreen>();
services.AddSingleton<EventScreen>();
services.AddSingleton<QuestGiverScreen>();
services.AddSingleton<QuestLogScreen>();
services.AddSingleton<MapScreen>();
services.AddSingleton<TalentScreen>();
services.AddSingleton<EndingScreen>();
services.AddSingleton<RegionScreen>();
services.AddSingleton<MainMenuScreen>();

using ServiceProvider provider = services.BuildServiceProvider();

GameSettings settings = provider.GetRequiredService<GameSettings>();
IGameIO io = provider.GetRequiredService<IGameIO>();
io.NarrationDelayMs = settings.NarrationDelayMs;

IMusicPlayer music = provider.GetRequiredService<IMusicPlayer>();
if (settings.MusicEnabled)
{
    music.Play(settings.MusicVolume);
}

try
{
    provider.GetRequiredService<MainMenuScreen>().Run();
    io.WriteLine("Do zobaczenia!");
    return 0;
}
catch (EndOfStreamException)
{
    // Zamknięte wejście (np. potok) – kończymy spokojnie.
    return 0;
}
