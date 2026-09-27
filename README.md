# PyramidTreasureConsoleRPG

Konsolowa gra RPG (C# / .NET 9) o wyprawie po świętego Graala ukrytego w piramidzie Chufu.
Gra zawiera treści dla dorosłych (brutalne opisy walki, alkohol, hazard) i przy pierwszym uruchomieniu pyta o wiek.

## Uruchomienie

Wymagany jest [.NET SDK 9](https://dotnet.microsoft.com/download).

```bash
dotnet run --project PyramidTreasureConsoleRPG.csproj
```

Interfejs używa biblioteki Spectre.Console (menu strzałkami, paski HP, tabele). Jeśli terminal ma z tym problem,
uruchom grę z przełącznikiem `--plain`, który przełącza na zwykłe, ponumerowane menu:

```bash
dotnet run --project PyramidTreasureConsoleRPG.csproj -- --plain
```

Muzyka w tle (NAudio) działa tylko na Windows. Na Linux i macOS gra uruchamia się bez muzyki.

Zapis gry i ustawienia trafiają do katalogu `GreatPyramidTreasureRPG_DataSave` w folderze danych aplikacji
(`%APPDATA%` na Windows, `~/.config` na Linux).

## Rozgrywka

- Trzy klasy: **Wojownik** (dużo zdrowia, ciężkie ciosy, pancerz z poziomu), **Łucznik** (obrażenia ze zręczności, najlepsze uniki), **Asasyn** (najcelniejszy, najczęstsze krytyki). Każda ma trzy ataki, w tym jeden specjalny.
- Fabułę prowadzi barman w tawernie. Przed każdym nowym etapem wyprawy trzeba z nim pogadać.
- Walka turowa z inicjatywą, miksturami w trakcie walki i ucieczką (poza bossami).
- Tawerna: bar (napoje leczą, jednorazowy miód „Grunwald” wzmacnia na stałe), kasyno (ruletka, jednoręki bandyta, blackjack, kości) i pokoje na górze.
- Na 20. poziomie karawana zabiera bohatera pod piramidę: dwaj Anubisi, a potem bóg Ra.
- W narracji dowolny klawisz pomija pauzę, `Esc` pomija cały tekst. Prędkość tekstu i muzykę ustawia się w menu głównym.

## Struktura kodu

| Katalog | Zawartość |
|---|---|
| `Domain/` | Modele i czyste reguły: `Hero` z definicjami klas (`HeroClasses`), `Enemy` z katalogiem wrogów (`EnemyCatalog`), mikstury, fabuła (`Story`), teksty (`Dialogues`), losowość (`IRandomSource`) |
| `Engine/` | Logika bez UI: `CombatEngine` (walka jako zdarzenia), `CasinoEngine`, serwisy baru, sklepu i noclegu, tabele spotkań |
| `Ui/` | Ekrany konsolowe rozmawiające tylko z `IGameIO`; dwie implementacje: `SpectreGameIO` (domyślna) i `ConsoleGameIO` (`--plain`) |
| `Infrastructure/` | Zapis gry i ustawień (JSON z generatorem źródeł), odtwarzacz muzyki (NAudio) |
| `Program.cs` | Rejestracja zależności (Microsoft.Extensions.DependencyInjection) i start gry |
| `tests/` | Testy xUnit: wzory walki, awanse, silnik walki i kasyna, zapis/odczyt, ekrany z dublerem konsoli, reguły architektury |
| `tools/` | `playbot.py` – bot grający przez potok od startu do napisu końcowego (smoke test w CI) |
| `docs/` | Analiza i code review, plan rozwoju |

## Testy

```bash
dotnet test PyramidTreasureConsoleRPG.sln
```

Bot grający całą grę (wymaga Pythona 3):

```bash
dotnet build -c Release
python3 tools/playbot.py bin/Release/net9.0/PyramidTreasureConsoleRPG.dll 1
```

CI (GitHub Actions) sprawdza formatowanie, buduje projekt z ostrzeżeniami jako błędami, uruchamia testy i przechodzi grę botem.

## Licencja

GPL-3.0 – patrz `LICENSE`.
