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
- Świat to sześć regionów (Port Sokoła, Stare Miasto, Delta i las, Szlak Karawan, Oaza Siwa, Piramida Chufu). Podróż kosztuje dni i złoto, a po drodze zdarzają się walki i zdarzenia z wyborami.
- Fabułę prowadzą zadania: główną linię daje barman w porcie, poboczne kapitan portu, przemytnik ze Starego Miasta i kapłanka z oazy. Zadania odblokowują kolejne regiony.
- Reputacja u trzech frakcji (Miasto, Podziemie, Bractwo) rośnie i spada od wyborów w zdarzeniach i zadaniach; otwiera lub zamyka niektóre opcje.
- Walka turowa z inicjatywą, miksturami w trakcie walki i ucieczką (poza bossami).
- Tawerna: bar (napoje leczą, jednorazowy miód „Grunwald” wzmacnia na stałe), kasyno (ruletka, jednoręki bandyta, blackjack, kości) i pokoje na górze.
- Na 20. poziomie, po zadaniach barmana, karawana zabiera bohatera pod piramidę: dwaj Anubisi, a potem bóg Ra.
- W narracji dowolny klawisz pomija pauzę, `Esc` pomija cały tekst. Prędkość tekstu i muzykę ustawia się w menu głównym.

## Struktura kodu

| Katalog | Zawartość |
|---|---|
| `Domain/` | Modele i czyste reguły: `Hero`, definicje klas (`HeroClasses`), katalog wrogów (`EnemyCatalog`), regiony (`World/RegionCatalog`), zdarzenia (`Events/EventCatalog`), zadania (`Quests/QuestCatalog`), mikstury, fabuła, teksty, losowość |
| `Engine/` | Logika bez UI: `CombatEngine` (walka jako zdarzenia), `TravelEngine`, `EventEngine`, `QuestEngine`, `CasinoEngine`, serwisy baru, sklepu i noclegu |
| `Ui/` | Ekrany konsolowe rozmawiające tylko z `IGameIO` (region, mapa, zdarzenia, zleceniodawcy, dziennik, walka, tawerna, kasyno, sklep, sakwa); dwie implementacje: `SpectreGameIO` (domyślna) i `ConsoleGameIO` (`--plain`) |
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
