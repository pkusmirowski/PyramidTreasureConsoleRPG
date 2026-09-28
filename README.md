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
- Ekwipunek: broń (osobna dla każdej klasy), pancerz i amulet w trzech slotach, torba na 8 przedmiotów, łupy z wrogów, sklepy z innym asortymentem w porcie, Starym Mieście i oazie, ceny zależne od reputacji.
- Talenty: na 5. i 10. poziomie wybór jednego z dwóch talentów klasy (np. Szał, Mur, Sokole oko, Złodziejski fach).
- Walka grupowa: wszyscy wrogowie atakują naraz, a ty wybierasz cel. Statusy (krwawienie, trucizna, ogłuszenie, strach), obrona (połowa obrażeń i gwarantowany krytyk), mikstury w trakcie walki i ucieczka (poza bossami).
- Wrogowie mają umiejętności: złodziej kradnie złoto i ucieka, wilki atakują stadem, dzik szarżuje, opancerzony blokuje, upadły rycerz straszy, templariusz ogłusza tarczą, Płaczący Mnich lamentuje i leczy się, Anubis wstaje raz z martwych, a Ra ma trzy fazy (cienie Anubisa i Oko Słońca, przed którym trzeba się zasłonić).
- Finał wymaga dobrego sprzętu i talentów.
- Tawerna: bar (napoje leczą, jednorazowy miód „Grunwald” wzmacnia na stałe), kasyno (ruletka, jednoręki bandyta, blackjack, kości, lichwiarz) i pokoje na górze.
- Warstwa 18+ z konsekwencjami: whisky i lotos wzmacniają na jedną walkę, ale lotos uzależnia (głód po dwóch dniach bez dawki: −15 % obrażeń, −10 trafienia, mija po tygodniu albo po odtrutce). Lichwiarz pożycza do 500 g na 10 % dziennie; po pięciu dniach zwłoki w porcie i Starym Mieście czekają egzekutorzy z Łamaczem. Po walce z ludźmi jeniec czasem żyje: dobić, puścić albo przesłuchać (test siły, reputacja, koszmary przez trzy noce). Na górze tawerny trzy postacie z własnymi wątkami (Zoja, Ptaszek, Neferet); sceny intymne dzieją się za zamkniętymi drzwiami, ale mają skutki: kradzież, plotka, amulet, wpływ na finał. Przekleństwa włącza się w ustawieniach.
- Pule wrogów w regionie odblokowują się z poziomem: na zalecanym poziomie regionu (mapa pokazuje „poziom N+”) trafia się najłatwiejsza grupa, każde dwa poziomy wyżej dochodzi kolejna.
- Na 20. poziomie, po zadaniach barmana, karawana zabiera bohatera pod piramidę: dwaj Anubisi, a potem bóg Ra.
- W narracji dowolny klawisz pomija pauzę, `Esc` pomija cały tekst. Prędkość tekstu i muzykę ustawia się w menu głównym.

## Struktura kodu

| Katalog | Zawartość |
|---|---|
| `Domain/` | Modele i czyste reguły: `Hero`, definicje klas (`HeroClasses`), katalog wrogów z łupami (`EnemyCatalog`), regiony (`World/RegionCatalog`), zdarzenia (`Events/EventCatalog`), zadania (`Quests/QuestCatalog`), przedmioty (`Items/ItemCatalog`), talenty (`Talents/TalentCatalog`), postacie z dialogami (`Npc/NpcCatalog`), mikstury i używki, fabuła, teksty, losowość |
| `Engine/` | Logika bez UI: `CombatEngine` (walka jako zdarzenia), `TravelEngine`, `EventEngine`, `QuestEngine`, `CasinoEngine`, `DialogueEngine`, `DayService` (dług, nałóg), `DebtService`, `InterrogationService`, serwisy baru, sklepu i noclegu |
| `Ui/` | Ekrany konsolowe rozmawiające tylko z `IGameIO` (region, mapa, zdarzenia, zleceniodawcy, dziennik, walka, tawerna, kasyno, pokoje i postacie na górze, sklep, sakwa); dwie implementacje: `SpectreGameIO` (domyślna) i `ConsoleGameIO` (`--plain`) |
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
