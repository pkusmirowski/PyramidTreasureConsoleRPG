# PyramidTreasureConsoleRPG

Konsolowa gra RPG (C# / .NET 9) o wyprawie po świętego Graala ukrytego w piramidzie Chufu.
Gra zawiera treści dla dorosłych (brutalne opisy walki, alkohol, hazard) i przy pierwszym uruchomieniu pyta o wiek.

## Uruchomienie

Wymagany jest [.NET SDK 9](https://dotnet.microsoft.com/download).

```bash
dotnet run --project PyramidTreasureConsoleRPG.csproj
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
| `Core/` | Obsługa konsoli (`GameIO`), losowość (`Rng`), ustawienia, odtwarzacz muzyki |
| `Classes/` | `Hero` (wspólna logika, statystyki pochodne, awanse) i trzy klasy postaci |
| `Monsters/` | `Enemy`, definicje przeciwników, tabele spotkań (`Encounters`) |
| `Items/` | Mikstury, sklep, sakwa |
| `Tavern/` | Tawerna, bar, kasyno, nocleg |
| `Game/` | Menu główne, sesja miasta, walka, fabuła, dialogi, zapis gry |
| `tests/` | Testy xUnit (wzory walki, awanse, zapis/odczyt, kasyno, fabuła) |
| `docs/` | Analiza i code review, plan rozwoju |

## Testy

```bash
dotnet test PyramidTreasureConsoleRPG.sln
```

CI (GitHub Actions) buduje projekt z ostrzeżeniami jako błędami i uruchamia testy przy każdym pushu.

## Licencja

GPL-3.0 – patrz `LICENSE`.
