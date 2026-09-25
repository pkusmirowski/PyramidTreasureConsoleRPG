# PyramidTreasureConsoleRPG – kompletna analiza, code review i propozycje rozwoju

Data analizy: 2026-09-25. Zakres: cały kod źródłowy w repozytorium (34 pliki `.cs`, ok. 3300 linii), konfiguracja projektu, stan repozytorium, fabuła i balans rozgrywki.

Uwaga: w środowisku analizy nie było dostępu do .NET SDK (pobranie zablokowane przez politykę sieci), więc analiza jest statyczna. Wszystkie opisane błędy wynikają z lektury kodu i symulacji wzorów na statystyki (skrypt w Pythonie), nie z uruchomienia gry.

---

## 1. Podsumowanie (TL;DR)

Projekt to działający szkielet konsolowego RPG: 3 klasy postaci, 9 typów przeciwników, tawerna z barem, kasynem i noclegiem, sklep z miksturami, zapis/odczyt stanu do JSON, muzyka w tle (NAudio) i liniowa fabuła prowadząca od miasta do piramidy Chufu, zakończona walką z Anubisami i Ra.

Najważniejsze wnioski:

1. **Gra ma kilka błędów blokujących**: ruletka, z której nie da się wyjść; blackjack i kości, które zapętlają się na zawsze przy 0 złota; crash (NullReferenceException) przy wczytaniu gry bez pliku zapisu lub przy złym wyborze w menu; crash przy wpisaniu bardzo dużej liczby.
2. **Ekonomia i balans są całkowicie do przebudowania**: woda za 5 g permanentnie podnosi obrażenia i pancerz, ujemne zakłady dodają złoto, ruletka na kolor nigdy nie zabiera stawki, „specjał” z Malborka można kupować bez końca, a pancerz rośnie kwadratowo, przez co Łucznik i Asasyn są nieśmiertelni od ok. 8–10 poziomu (Ra nie jest w stanie ich draśnąć).
3. **Zapis gry gubi dane**: ekwipunek, etap fabuły (`GameStatus`) i wewnętrzny licznik `nextLevel` nie są zapisywane. Po wczytaniu formuła na doświadczenie daje ujemne wartości i każdy zabity wróg to nowy poziom.
4. **Architektura**: trzy klasy postaci są w ~95% kopią tego samego pliku, dziewięć klas wrogów różni się tylko liczbami w konstruktorze, logika jest splątana z `Console.WriteLine`, zero testów. Refaktoryzacja do klasy bazowej + danych jest tania i odblokuje dalszy rozwój.
5. **Repozytorium**: śledzone są katalogi `bin/`, `obj/` i `.vs/` (220 plików, ok. 245 MB) oraz pliki `.user` z lokalnymi ścieżkami autora. Brakuje `.gitignore`, CI i sensownego README.
6. **Świat i fabuła**: obecnie miks epok (faraonowie, templariusze, whisky, miód z Malborka, jednoręki bandyta) bez wyjaśnienia; zakończenie jest cliffhangerem bez Graala. Treści „18+” praktycznie nie ma. Poniżej jest propozycja spójnej wizji świata i warstwy dla dorosłych.

---

## 2. Jak gra jest zbudowana (mapa kodu)

| Obszar | Pliki | Rola |
|---|---|---|
| Wejście | `Program.cs` | Start muzyki (NAudio), uruchomienie `Game` |
| Pętla gry | `Game/Game.cs`, `Game/GameState.cs` | Menu główne, pętla tur, walka, wybór klasy, statystyki |
| Fabuła | `Game/Dialogues.cs`, `Game/GameStatus.cs` | Intro, rozmowy z barmanem, zakończenie, bramkowanie fabuły wg poziomu |
| Zapis | `Game/DataSave.cs`, `Game/ClassState.cs` | Serializacja do `%APPDATA%/GreatPyramidTreasureRPG_DataSave/DataSave.json` |
| Postacie | `Classes/IClass.cs`, `Warrior.cs`, `Archer.cs`, `Assassin.cs` | Statystyki, 3 ataki na klasę, awans |
| Wrogowie | `Monsters/IEnemy.cs`, `GenerateEnemies.cs`, `Enemies/*.cs` | Statystyki, dobór grup wg poziomu |
| Przedmioty | `Items/IItem.cs`, `Shop.cs`, `Potions/*.cs` | Trzy mikstury, sklep, picie |
| Tawerna | `Tavern/Tavern.cs`, `Bar.cs`, `Casino.cs`, `Rest.cs` | Barman, napoje, ruletka/automat/blackjack/kości, nocleg, „dziewczyny” |
| Narzędzia | `StandardFunctions.cs`, `CollectionExtension.cs` | Parsowanie wejścia, losowość, opóźnienia |

Przepływ: `Program.Main` → `Game.MainGame` → **konstruktor `GameState` pyta „Nowa gra / Wczytaj”** → dopiero potem menu „Rozpocznij / Wyjdź” → pętla `GameState.StartGame` → menu miasta → `FightOpponents` (poziom < 20) lub `FinalBoss` (poziom 20).

---

## 3. Błędy krytyczne (crash, zawieszenie, exploit)

Kolejność wg wagi. Odwołania do plików i linii dotyczą stanu gałęzi `main`.

### 3.1. Ruletka: nie da się z niej wyjść
`Tavern/TavernOptions/Casino.cs:53-87`. Zmienna `ifRoulette` nigdy nie jest ustawiana na `false`. Wybór „2: Zrezygnuj” ustawia tylko `ifPlay = false`, po czym zewnętrzna pętla `while (ifRoulette)` znów wyświetla menu. Jedyne wyjście to zamknięcie procesu.

### 3.2. Blackjack i kości: nieskończona pętla przy 0 złota
`Casino.cs:313-331` (`GetValidBet`) i `Casino.cs:393-416` (`GetValidCrapsBet`). Warunek `while (bet <= 0 || bet > characterClass.Gold)` przy `Gold == 0` nie ma żadnej poprawnej wartości: 0 jest odrzucane, 1 przekracza stan konta. Nie ma opcji rezygnacji (w przeciwieństwie do `GetBetAmount`, gdzie 0 oznacza wyjście). Wojownik startuje z 1 sztuką złota, więc jest to łatwe do osiągnięcia.

### 3.3. Ujemne zakłady dodają złoto
`Casino.cs:154-173` (`GetBetAmount`). Warunek `gold == 0 || gold <= characterClass.Gold` przepuszcza liczby ujemne. W `PlayRouletteNumber` (`Gold -= gold`) i `PlaySlotMachine` (`Gold -= bet`) postawienie `-100000` daje +100000 złota.

### 3.4. Ruletka na kolor nigdy nie pobiera stawki
`Casino.cs:89-109`. Przy wygranej `gold *= 2; Gold += gold`, przy przegranej nic. Gracz nigdy nie traci, więc jest to nieskończone źródło złota (w odróżnieniu od `PlayRouletteNumber`, gdzie stawka jest pobierana z góry).

### 3.5. Woda za 5 g to permanentny „god mode”
`Tavern/TavernOptions/Bar.cs:61-94`. Każde wypicie wody wywołuje `characterClass.UpdateStats()`, a `UpdateStats` (np. `Classes/Warrior.cs:113-120`) używa `+=`: `MinDmg += Str/3; MaxDmg += Str/2; Armor += Dex/2` i resetuje HP do maksimum. Sto szklanek wody = setki punktów pancerza i obrażeń, plus darmowe leczenie tańsze niż nocleg. Ten sam problem dotyczy specjału (`Bar.cs:204`).

Dodatkowo logika wody jest odwrócona: barman na poziomie < 5 mówi, że „dostępna jest tylko woda” (`Dialogues.cs:172`), a `DrinkWater` przy `Level < MinLevelForWater (5)` pobiera złoto i odmawia (`Bar.cs:70-75`). Gałąź dająca doświadczenie (`Level < MinLevelForWhisky`) jest nieosiągalna, bo ten sam warunek już zakończył metodę.

### 3.6. Specjał z Malborka można kupować bez końca
`Bar.cs:19` – flaga `specialDrink` jest polem instancji `Bar`, a `Tavern.TavernOptions` tworzy `new Bar()` przy każdym wejściu do tawerny (`Tavern.cs:9`). Każde wejście = kolejne +5000 EXP, +5 do statystyki i kolejny `UpdateStats`.

### 3.7. Crash przy wczytywaniu gry bez pliku / błędnym wyborze
- `Game/GameState.cs:14-32`: konstruktor przy wyborze innym niż 1/2 zostawia `characterClass == null`; `StartGame` od razu robi `characterClass.Level` → `NullReferenceException`.
- `Game/DataSave.cs:9-47`: brak pliku zapisu → `stateVariables == null`, mimo to wypisuje „Gra została wczytana!” i zwraca `null`; gra wywala się jak wyżej. Nieznany `ClassType` → `ConvertStatsToClass(null, …)` → NRE. Uszkodzony JSON → nieobsłużony `JsonException`.

### 3.8. Crash przy dużej liczbie na wejściu
`StandardFunctions.cs:53-72` łapie tylko `FormatException`. Wpisanie np. `99999999999` rzuca `OverflowException` i zamyka grę. `Shop.PotionShop` używa `int.TryParse`, reszta gry nie – niespójność.

### 3.9. Śmierć gracza zabija proces
`GameState.cs:203-208`: `Environment.Exit(-1)` w trakcie walki. Pomija menu główne, blok obsługi śmierci w `StartGame` (`GameState.cs:58-63`) jest martwym kodem, brak opcji „wczytaj zapis / zacznij od nowa”.

---

## 4. Błędy logiczne i balans

### 4.1. Zapis gry gubi stan
`Game/ClassState.cs` nie zawiera:
- `Inventory` – wszystkie kupione mikstury znikają po wczytaniu,
- `GameStatus` – etap fabuły wraca do 0, więc rozmowy z barmanem się rozjeżdżają (na poziomie 5–9 ze statusem 0 barman mówi „brak wieści” i nigdy nie przejdzie do statusu 2),
- prywatnego `nextLevel` z klas postaci – po wczytaniu licznik startuje od 2, a wzór `MaxExp = 250·(n−1)·n − MaxExp` (`Warrior.cs:229`) daje **ujemny** próg (np. wczytane `MaxExp = 9500` → następny próg `1500 − 9500 = −8000`). Od tego momentu każdy zabity wróg to nowy poziom.

### 4.2. Mikstury leczą zawsze do pełna
`Items/Potions/UsingPotions.cs:68-73`: `if (characterClass.Hp > previousHP) Hp = MaxHP` – warunek jest prawdziwy po każdym wypiciu, więc mała mikstura za 20 g leczy do pełna. Powinno być `if (Hp > MaxHP)`.

### 4.3. Po każdej wypitej miksturze pojawia się „Brak opcji.”
`UsingPotions.cs:88-96`: `if (choice == 4) … else NoOption()` wykonuje się także dla poprawnych wyborów 1–3. Menu ma też zły napis „4. Wyjdź z tawerny” (to ekwipunek).

### 4.4. Pusty ekwipunek nic nie wyświetla
`UsingPotions.cs:104-113`: wynik LINQ nigdy nie jest `null`, więc komunikat „Nie posiadasz żadnych mikstur!” nigdy się nie pokaże. Trzeba użyć `.Any()`.

### 4.5. Słabszy wariant trzeciego ataku może leczyć wroga
`Warrior.cs:160`: `RandDmg(MinDmg − Dex − 1, MaxDmg − Dex − 1)`; dla startowego Wojownika to zakres `−1…4`, dla Łucznika (`Archer.cs:165`, `MinDmg − Dex`) `−1…1`. Ujemne obrażenia odejmowane od HP wroga (`enemy.Hp -= realDmg`) go leczą, a komunikat brzmi „Zadałeś −1”.

### 4.6. Pancerz rośnie kwadratowo – dwie klasy są nieśmiertelne
Pancerz jest odejmowany płasko od obrażeń wroga (`Monsters/IEnemy.cs:23`) i rośnie o wartość zależną od `Dex` na każdym poziomie. Symulacja wzorów z kodu (bez exploitów z wodą):

| Poziom | Wojownik: pancerz / obrażenia | Łucznik: pancerz / obrażenia | Asasyn: pancerz / obrażenia / szansa trafienia |
|---|---|---|---|
| 5 | 11 / 22–38 | 40 / 15–17 | 30 / 28–31 / 107 % |
| 10 | 35 / 67–107 | 130 / 45–47 | 110 / 86–89 / 205 % |
| 15 | 71 / 137–213 | 270 / 92–94 | 240 / 178–181 / 352 % |
| 20 | 120 / 232–357 | 460 / 155–157 | 420 / 303–306 / 550 % |

Obrażenia wrogów: Wilk 9–15, Dzik 11–18, Opancerzony złodziej 23–31, Upadły rycerz 40–57, Templariusz 45–57, Płaczący mnich 77–99, Anubis 70–120, Ra 100–150.

Wnioski:
- Łucznik od 5. poziomu jest odporny na wilki i dziki, od 10. na wszystko włącznie z Ra.
- Asasyn od 8. poziomu ignoruje templariuszy, od 12. Ra; jego szansa trafienia przekracza 100 % już na 5. poziomie (`Assassin.cs:120`: `AttakChance += Dex` w `UpdateStats`).
- Wojownik na 20. poziomie ma 825 HP i przyjmuje od Ra 0–30 obrażeń, a zadaje 232–357, więc Ra (2500 HP) pada w ok. 9 ciosach. Finał jest formalnością dla każdej klasy.

### 4.7. Krzywa doświadczenia jest chaotyczna
Wzór `MaxExp = 250·(n−1)·n − MaxExp` daje ciąg progów: 1000, 500, 2500, 2500, 5000, 5500, 8500, 9500, 13000, 14000, … (raz duży skok, raz mały). Pierwsza walka (2 złodziei × 500 EXP) daje od razu poziom 1, druga poziom 2. Postać zaczyna na poziomie 0, co wygląda jak błąd.

### 4.8. Bramkowanie fabuły na 20. poziomie nie działa
`Game/GameStatus.cs:30-36` sprawdza `Level == 20 && GameStatus == 3`, ale `GameState.HandleFinalChoice` (`GameState.cs:142-144`) nie wywołuje `CheckGameStatus` i od razu uruchamia `FinalBoss`. Gracz może pominąć ostatnią rozmowę z barmanem o karawanie. Analogicznie `GenerateEnemies.EnemiesLevel20` (`GenerateEnemies.cs:102-105`) jest nieosiągalne.

### 4.9. Kasyno – drobniejsze
- Automat (`Casino.cs:196-236`): stan konta wypisywany **przed** odjęciem stawki; wypłata za parę liczona z pierwszego bębna nawet gdy para to bębny 2 i 3.
- Blackjack (`Casino.cs:426-431`): `DrawCard` zwraca 2–10, więc asy nie występują i cała logika asów w `CalculateScore` jest martwa; krupier dobiera karty nawet po spaleniu gracza; brak wypłaty 3:2 za blackjacka.
- Kości mają limit stawki 1–100, pozostałe gry nie mają żadnego limitu – niespójne.
- W każdej metodzie `new Random()` (6 miejsc) zamiast `Random.Shared`.

### 4.10. Brak akcji w walce
`GameState.Fight` (`GameState.cs:191-228`): wróg zawsze atakuje pierwszy, nie ma ucieczki, nie ma picia mikstur w trakcie walki (można tylko poza nią), nie widać HP wroga przed wyborem ataku. Walka to wybór 1/2/3 aż ktoś padnie.

### 4.11. Kolejność menu startowego
Konstruktor `GameState` (wybór „Nowa gra / Wczytaj”, intro trwające ok. 32 s, wybór klasy) wykonuje się **zanim** gracz zobaczy menu „1. Rozpocznij grę / 2. Wyjdź z gry”. Wyjście z gry wymaga najpierw stworzenia postaci.

### 4.12. Muzyka
- `Program.cs:40-44`: handler `PlaybackStopped` wywołuje `Play()` także po `Stop()` z `DisposeAudio`, co może rzucić wyjątkiem na zamykanym urządzeniu.
- `WaveOutEvent` działa tylko na Windows; na Linux/macOS gra wypisze błąd i pójdzie dalej (to akurat dobrze), ale nie ma żadnej opcji wyciszenia lub zmiany głośności.
- `StandardFunctions.SoundPlayer` nie jest nigdzie używany, a ciągnie za sobą pakiet `System.Windows.Extensions`.

---

## 5. Code review: architektura i jakość

### 5.1. Duplikacja
- `Warrior.cs`, `Archer.cs`, `Assassin.cs` (razem ~740 linii) różnią się tylko wartościami startowymi, nazwami ataków, wzorami w `UpdateStats`/`LevelUP` i jedną gałęzią trzeciego ataku. Docelowo: `abstract class Character` z 17 wspólnymi właściwościami, wspólnym `Attack` (menu), `DealDmg`, `AddLevel`, `LevelUP`, i trzema `abstract`/`virtual` metodami dla specyfiki klasy. Statystyki startowe i przyrosty jako `record ClassDefinition`.
- 9 plików wrogów to identyczna klasa z innymi liczbami. Wystarczy jedna klasa `Enemy` + tabela definicji (lista rekordów albo `enemies.json`).
- 3 mikstury tak samo: `record Potion(int Id, string Name, int Price, int RestoreHp)`.
- `DataSave.ConvertStatsToSave/ConvertStatsToClass` to ręczne mapowanie 16 pól w obie strony. Po refaktoryzacji zapisywany może być bezpośrednio obiekt postaci (albo DTO generowany jedną metodą).

### 5.2. Splątanie logiki z konsolą
Każda metoda domenowa (`Attack`, `LevelUP`, `BuyPotion`, `DrinkWater`, cała `Casino`) pisze bezpośrednio do `Console` i czyta `Console.ReadLine()`. Skutki: nie da się przetestować ani jednej reguły gry, nie da się zmienić UI, każda zmiana tekstu to zmiana logiki.

Rekomendacja: wprowadzić warstwę `IGameIO` (`Write`, `WriteLine(color)`, `ReadInt(range)`, `Pause(skippable)`) i przekazywać ją przez konstruktor. Logika (`CombatEngine`, `LevelingService`, `CasinoGames`) powinna zwracać wyniki (rekordy: `AttackResult`, `RoundResult`), a warstwa prezentacji je wypisywać.

### 5.3. Losowość
Trzy różne źródła: `ThreadLocal<Random>` w `StandardFunctions`, statyczny `Random` z lockiem w `CollectionExtension`, sześć `new Random()` w `Casino`. W .NET 6+ jest `Random.Shared`. Dla testów: interfejs `IRandom` wstrzykiwany do silnika walki.

### 5.4. Nazewnictwo i typy
- `IClass` → `ICharacter`/`Hero`; `AttakChance` → `AttackChance`; `MaxHP` vs `Hp` (niespójna wielkość liter).
- `GameStatus` (int 0–4) → `enum StoryStage { Start, BanditsCalmed, WolvesCleared, CaravanAnnounced, CaravanReady }`.
- `ClassType` (int 1–3) → `enum CharacterClass`.
- Magiczne liczby: 20 (maks. poziom) w 8 miejscach, progi 5/10/15, ceny w tekstach menu i osobno w kodzie (`Shop.cs:18-20` vs `Shop.cs:34-42` vs `SmallPotion.Price`).
- `[Serializable]` na `ClassState` jest zbędne przy `System.Text.Json`.
- `IItem.RestoreHP` w interfejsie każdego przedmiotu zakłada, że wszystko jest miksturą; `Bar.ShowGoods` to `// To do + ekwipunek`, więc hierarchia przedmiotów będzie potrzebna.
- Przestrzeń nazw `GreatPyramidTreasureConsoleRPG` vs nazwa projektu `PyramidTreasureConsoleRPG`; w `bin/` leżą oba assembly.

### 5.5. Konfiguracja projektu (`PyramidTreasureConsoleRPG.csproj`)
- `RunAnalyzersDuringBuild=false` przy jednoczesnym `Roslynator.Analyzers` – analizator jest zainstalowany, ale wyłączony.
- `TreatWarningsAsErrors` tylko w Debug – Release przepuści ostrzeżenia.
- Brak `<Nullable>enable</Nullable>` i `<ImplicitUsings>` – projekt jest na .NET 9, warto włączyć nullable, bo część opisanych NRE zostałaby złapana przez kompilator.
- `System.Runtime.Extensions 4.3.1` to pakiet z czasów .NET Core 1.x, na .NET 9 zbędny.
- `System.Windows.Extensions` potrzebny tylko dla nieużywanego `SoundPlayer`.
- `<None Remove="ancient_egypt.mp3">` odwołuje się do pliku, którego nie ma.

### 5.6. Testy
Brak jakichkolwiek testów. Po oddzieleniu logiki od konsoli naturalne testy jednostkowe (xUnit): obliczanie obrażeń i pancerza, krzywa poziomów, round-trip zapisu, wypłaty w kasynie, bramkowanie fabuły, niemożność ujemnego zakładu.

---

## 6. Repozytorium i higiena

| Problem | Szczegóły | Działanie |
|---|---|---|
| Śledzone artefakty budowania | 220 plików w `bin/`, `obj/`, `.vs/`; `bin/` waży 243 MB, `.git` 47 MB; są tam buildy na netcoreapp3.1, net5, net6, net8, net9 | Dodać `.gitignore` (szablon VisualStudio), `git rm -r --cached bin obj .vs` |
| Pliki użytkownika | `PyramidTreasureConsoleRPG.csproj.user`, `Properties/PublishProfiles/*.pubxml(.user)` zawierają ścieżki `C:\Users\Paweł\...` | Usunąć z repo, dodać do `.gitignore` |
| README | Jedno zdanie i screenshoty | Opis rozgrywki, wymagania (.NET 9, Windows dla muzyki), `dotnet run`, sterowanie, lokalizacja zapisu |
| CI | Brak | GitHub Actions: `dotnet build -warnaserror` + `dotnet test` na push/PR |
| Licencja | GPL-3.0 | OK, ale wspomnieć w README |
| Formatowanie | Brak `.editorconfig` | Dodać, żeby styl (`this.`, nawiasy) był spójny; teraz część plików używa `this.`, część nie |

---

## 7. UX konsoli

- **Czas**: 60 wywołań `StandardFunctions.Sleep()` po 4 sekundy. Intro to 8 linii = 32 s bez możliwości pominięcia; każda rozmowa z barmanem 20–28 s. Rozwiązanie: „naciśnij dowolny klawisz, aby kontynuować / Esc pomija” z `Console.KeyAvailable`, plus ustawienie prędkości tekstu.
- **Brak ekranu ustawień**: muzyka on/off, głośność, prędkość tekstu.
- **Statystyki**: `ShowStats` nie pokazuje `Str`, `Dex`, `Vit`, szansy trafienia i krytyka, a specjał każe wybrać statystykę do podniesienia, której gracz nie widzi.
- **Walka**: brak nagłówka z HP obu stron przed każdą turą, brak informacji o wrogu (obrażenia), brak logu tury.
- **Wejście**: „Wprowadzono zły znak!” + `Thread.Sleep(700)` przy każdym literówce; czasem `Console.Clear()` czyści wynik zanim gracz go przeczyta (`HandleChoice`).
- **Emoji w automacie** (`🍇`, `7️⃣`) renderują się w konsoli Windows tylko przy UTF-8 i odpowiedniej czcionce; brak `Console.OutputEncoding = Encoding.UTF8` w `Program.Main`, więc polskie znaki też mogą się psuć na starszych terminalach.
- **Nazwa gracza**: brak walidacji pustego imienia.
- **Poziom 0** na starcie.

Literówki i język: „Spokałeś” → „Spotkałeś”, „Uleczłeś” → „Uleczyłeś”, „gre” → „grę”, „imie” → „imię”, „Wynamij” → „Wynajmij”, „z sklepu” → „ze sklepu”, „Marlborka” → „Malborka”, „2 z lini” → „2 z linii”, „miksture” → „miksturę”, „Boje się” → „Boję się”, „szklanę” → „szklankę”, „gral” → „Graal” (konsekwentnie), „Anubisy padają pod twoim mieczem” (Łucznik i Asasyn nie mają miecza), w `AfterGuards` gracz „ściska sztylet z nieznanego złota”, który znajduje dopiero w `Ending`.

---

## 8. Świat, fabuła i wizja „18+”

### 8.1. Co jest teraz
Intro: starożytny Egipt, dynastia faraonów, mapa do Graala w piramidzie Chufu. W praktyce: miasto portowe z tawerną, kasyno (ruletka, jednoręki bandyta, blackjack), szkocka whisky, kawaleria, templariusze, upadli rycerze, płaczący mnich, miód pitny z Malborka „sprzed bitwy pod Grunwaldem”, a na końcu Anubisy i Ra. Zakończenie: w skrzyni nie ma Graala, są „przedmioty z innego świata”, sztylet i kolejna mapa.

To nie musi być wada. Zakończenie samo podsuwa klucz: **piramida jest miejscem, gdzie przenikają się epoki**. Jeśli przyjąć to jako zasadę świata, cały miks staje się cechą, a nie błędem.

### 8.2. Propozycja spójnej ramy świata
„Kair, rok 1928.” Pulpowa, brudna przygoda w stylu ekspedycji archeologicznych: port, dzielnica knajp, kasyno prowadzone przez lewantyńskiego przemytnika, brytyjska kawaleria, Bractwo Płaczącego Mnicha (fanatycy strzegący piramidy), rycerze-templariusze jako duchy tych, którzy szukali Graala 700 lat wcześniej i zostali na zawsze w korytarzach. Anubis i Ra to nie bogowie, lecz to, co piramida robi z ludźmi, którzy weszli zbyt głęboko. Graal to obiekt, który „nie jest z tego świata” i ściąga do siebie wszystkie epoki. To wyjaśnia whisky, Malbork, ruletkę, faraonów i templariuszy jednocześnie.

Alternatywa w tym samym duchu: rok 1410, bohater to zbieg spod Grunwaldu; wtedy miód z Malborka i templariusze są u siebie, a kasyno trzeba zamienić na kości i karty.

### 8.3. Struktura świata (zamiast jednego przycisku „Udaj się w drogę”)
1. **Regiony** odblokowywane etapami fabuły: Port → Stare Miasto → Las/Delta → Pustynia → Oaza → Piramida (poziomy: przedsionek, sala posągów, komnata Ra).
2. Każdy region: własna pula wrogów, 3–5 **losowych zdarzeń z wyborem** (rozbita karawana: obrabować czy pomóc; ranny templariusz; kupiec z lotosem), własny handlarz i sekret.
3. **Zadania od NPC** (barman, kapitan portu, kapłanka, przemytnik) zamiast bramkowania po poziomie. Etap fabuły (`StoryStage`) rośnie po zadaniu, nie po poziomie.
4. **Ekwipunek**: broń, pancerz, amulety (już zapowiedziane przez `ShowGoods`), z prostymi statystykami (obrażenia, pancerz, szansa trafienia). To rozwiązuje też problem płaskiego pancerza: pancerz z przedmiotów, nie z poziomu.
5. **Walka z decyzjami**: tura gracza przed wrogiem przy wyższej `Dex`; akcje: atak (3 warianty), mikstura, ucieczka (szansa od `Dex`), ewentualnie „obrona” (połowa obrażeń, następny cios krytyczny). Pancerz jako procentowa redukcja (`dmg * 100 / (100 + armor)`) zamiast płaskiego odejmowania.
6. **Kilka zakończeń**: Graal zabrany (moc, ale klątwa), Graal zniszczony, Graal oddany Bractwu, zdrada karawany. Obecne zakończenie może zostać jako jedno z nich (cliffhanger do drugiej części).

### 8.4. Warstwa „18+” (dojrzała, nie wulgarna dla samej wulgarności)
Dziś jedyne „dorosłe” elementy to obelgi barmana, alkohol, hazard i lista tekstów „dziewczyn” w `Rest`. Propozycje, które dają treść dla dorosłych i mają wpływ mechaniczny:

- **Bramka wiekowa** przy starcie (potwierdzenie 18+) i ostrzeżenie o treściach.
- **Przemoc opisana dosłownie**: brutalne opisy ciosów krytycznych i śmierci wrogów (losowane z puli, zależne od klasy i broni), egzekucje pokonanych z konsekwencją (reputacja).
- **Używki z mechaniką**: whisky (+trafienie, −pancerz na 3 walki), lotos/opium (+obrażenia, ryzyko uzależnienia: codzienny koszt lub kary), kac.
- **Hazard jako ryzyko**: lichwiarz w kasynie, dług, egzekutorzy długu jako wrogowie, możliwość przegrania ekwipunku.
- **Romanse i „pokoje na górze”**: NPC z imieniem, historią i własną sceną; sceny intymne jako fade-to-black z opisem nastroju, efekt: odpoczynek, plotka fabularna, czasem kradzież sakiewki lub choroba (debuff). Ważne: dorosłość przez kontekst i konsekwencje, bez pisania scen pornograficznych.
- **Moralne wybory bez „dobrej” odpowiedzi**: sprzedać towarzysza Bractwu za mapę, zostawić karawanę na pustyni, zjeść… (motyw przetrwania), tortury jeńca dla informacji (skuteczne, ale ślad w reputacji i snach bohatera).
- **Reputacja i plotki**: barman komentuje czyny gracza, ceny i dostępność zadań zależą od reputacji.
- **Język**: przekleństwa w dialogach NPC, ale z wyczuciem i z możliwością wyłączenia w ustawieniach.

---

## 9. Plan działania (priorytety)

### P0 – naprawy blokujące (1–2 dni)
1. Wyjście z ruletki (`ifRoulette = false` przy wyborze 2).
2. `GetValidBet`/`GetValidCrapsBet`: opcja 0 = rezygnacja; odrzucać ujemne stawki wszędzie; wspólna metoda `ReadBet(min, max, gold)`.
3. Ruletka na kolor: pobierać stawkę przed losowaniem.
4. `ToInt32` → `int.TryParse` (bez wyjątków, bez `Thread.Sleep`).
5. `GameState`: brak postaci (zły wybór, brak zapisu) → wrócić do menu zamiast NRE; `LoadGame` ma raportować brak pliku.
6. Mikstury: `if (Hp > MaxHP) Hp = MaxHP`; usunąć fałszywe „Brak opcji”; `.Any()` w `ShowPotions`.
7. Śmierć → ekran „Koniec gry” z powrotem do menu, bez `Environment.Exit`.
8. `UpdateStats` przestaje być wywoływane przy piciu; wyprostować logikę wody (dostępna do 5. poziomu, potem bez EXP); flaga specjału w stanie gry, nie w `Bar`.
9. Trzeci atak: `Math.Max(1, …)` na dolnym zakresie.
10. `ClassState`: dodać `Inventory`, `GameStatus`, `NextLevel` (lub przeliczać próg z poziomu); walidacja wczytanego JSON.
11. Dodać `.gitignore`, usunąć `bin/obj/.vs/*.user` z repo.

### P1 – balans i refaktoryzacja (1–2 tygodnie)
1. `Character` jako klasa bazowa + `ClassDefinition`; `Enemy` + tabela definicji; `Potion` jako rekord.
2. Nowy model pancerza (redukcja procentowa lub cap) i liniowe przyrosty statystyk; nowa krzywa EXP (np. `MaxExp = 500 · lvl^1.6`); start od poziomu 1.
3. `IGameIO` + wyniesienie tekstów do zasobów (`.resx` lub słownik) – od razu umożliwia wersję EN.
4. Pomijalne opóźnienia, ustawienia (muzyka, prędkość tekstu), `Console.OutputEncoding = UTF8`.
5. Testy xUnit dla silnika walki, poziomów, zapisu, kasyna. CI w GitHub Actions.
6. `Nullable enable`, włączyć analizatory, usunąć zbędne pakiety.
7. Mikstury i ucieczka w walce; inicjatywa z `Dex`; nagłówek walki z HP.

### P2 – treść i świat (ciągłe)
1. Regiony i zdarzenia z wyborami (dane w JSON, żeby dodawać treść bez kompilacji).
2. Ekwipunek (broń/pancerz/amulety) i sklep z asortymentem zależnym od etapu.
3. Zadania od NPC zamiast bramkowania po poziomie.
4. Warstwa 18+ z sekcji 8.4 + bramka wiekowa + reputacja.
5. Kilka zakończeń, ekran podsumowania przygody.
6. Zapisy w wielu slotach i autozapis przed piramidą.

---

## 10. Co warto zachować

- Prosta, czytelna struktura katalogów i podział na tawernę/sklep/walkę.
- Pomysł na fabułę prowadzoną przez barmana i etapy świata (bandyci → wilki → karawana → piramida) – wystarczy przenieść go z poziomów na zadania.
- Trzy różne zestawy ataków na klasę z ryzykiem (szansa trafienia vs obrażenia) – to dobry rdzeń do rozbudowy.
- Cztery gry hazardowe – po naprawie ekonomii to fajny „czasoumilacz” i haczyk na warstwę długów.
- Zakończenie z „przedmiotami z innego świata” – najlepszy hak fabularny w całej grze, warto na nim zbudować tożsamość świata.
