# Plan rozwoju – co dalej z Pyramid Treasure

Stan po tej iteracji: wszystkie błędy blokujące i exploity z `ANALIZA_I_CODE_REVIEW.md` są naprawione, kod jest
zrefaktoryzowany (jedna klasa bazowa bohatera i wroga, logika oddzielona od menu tam, gdzie to było tanie), jest
48 testów i CI. Gra jest przechodzalna każdą klasą bez błędów (sprawdzone botem grającym przez potok).

Poniżej propozycja kolejnych kroków, uporządkowana tak, żeby każdy etap dawał coś graczowi.

## Etap 1 – świat zamiast przycisku „Udaj się w drogę” (2–3 tygodnie)

Cel: podróż ma być podróżą. Zamiast losowej walki z tabeli, gracz wybiera region.

1. **Regiony** (`Regions/`): Port, Stare Miasto, Delta i las, Pustynia, Oaza, Piramida. Każdy ma własną pulę
   wrogów, handlarza, kilka opisów tła i wymagany etap fabuły. Tabela `Encounters` staje się częścią regionu.
2. **Zdarzenia z wyborem** (dane w `events.json`, ładowane przy starcie): 3–5 na region, np. rozbita karawana
   (obrabować / pomóc / minąć), ranny templariusz (dobić / opatrzyć / przesłuchać), kupiec z lotosem. Każdy wybór
   zmienia złoto, HP, reputację lub daje przedmiot. Szkielet: `record GameEvent(string Text, List<EventChoice>)`,
   `record EventChoice(string Text, Func<Hero, string> Apply)`.
3. **Reputacja** (`int Reputation` w `Hero`, zapisywana): barman komentuje czyny gracza, ceny w sklepie i dostępność
   zadań zależą od reputacji. To tani sposób, żeby wybory 18+ (egzekucje, zdrady) miały konsekwencje.
4. **Zadania od NPC** zamiast bramkowania po poziomie: barman, kapitan portu, kapłanka, przemytnik. Etap fabuły
   przesuwa się po wykonaniu zadania („oczyść las z wilków: 3 walki w regionie Las”), nie po zdobyciu poziomu.
   `StoryStage` zostaje, zmienia się tylko warunek przejścia.

## Etap 2 – ekwipunek i głębsza walka (2 tygodnie)

1. **Broń, pancerz, amulety**: `abstract class Item` z podklasami `Weapon` (bonus do obrażeń, szansa krytyka),
   `Armor` (pancerz, uniki), `Trinket` (jedna cecha, np. +10 % złota). Sloty w `Hero`, statystyki pochodne biorą
   pod uwagę wyposażenie. Sklep w każdym regionie ma inny asortyment. To odblokowuje puste dziś „Pokaż mi swoje towary”.
2. **Statusy w walce**: krwawienie (od trzystronnego cięcia), trucizna (zatrute ostrze Asasyna), ogłuszenie
   (skupiony strzał). Prosta lista `List<StatusEffect>` na wrogu i bohaterze, tykająca na początku tury.
3. **Obrona** jako czwarta akcja: połowa obrażeń w tej turze, następny cios gwarantowany krytyk.
4. **Wrogowie z zachowaniem**: prosty `enum EnemyBehavior { Aggressive, Defensive, Coward }` – tchórz ucieka przy
   20 % HP (i zabiera złoto), obrońca co drugą turę blokuje.
5. **Bossowie z fazami**: Ra przy 50 % HP wzywa dwa Anubisy albo zmienia atak. Kilka linii w `Combat`, duży efekt.

## Etap 3 – warstwa dla dorosłych z konsekwencjami (1–2 tygodnie)

Dziś jest bramka wiekowa, brutalne opisy zgonów i krytyków, noc w towarzystwie z losowym skutkiem. Kolejne kroki:

1. **Używki z mechaniką**: whisky = +10 % trafienia i −10 % uników na następną walkę; lotos = +25 % obrażeń, ale po
   trzech dawkach uzależnienie (kara do statystyk, dopóki gracz nie kupi kolejnej dawki). Wymaga listy „buffów na
   następną walkę” w `Hero`.
2. **Dług w kasynie**: lichwiarz pożycza do 500 g; niespłacony dług po 5 wyprawach oznacza egzekutorów jako losowe
   spotkanie w mieście. Kasyno przestaje być bezpiecznym miejscem.
3. **NPC z imieniem i historią** na górze tawerny: 2–3 postacie, każda z 3–4 rozmowami odblokowywanymi reputacją
   i etapem fabuły. Sceny intymne pozostają „za zamkniętymi drzwiami” (fade-to-black), liczy się kontekst i skutek:
   plotka, kradzież, dar, choroba (debuff na jedną walkę).
4. **Tortury jeńca** jako opcja po walce z ludzkim wrogiem: informacja (odkrycie zdarzenia w regionie) kosztem
   reputacji i „koszmarów” (−5 % HP przy każdym noclegu przez 3 noce).
5. **Przekleństwa w dialogach** z przełącznikiem w ustawieniach (`ProfanityEnabled`).

## Etap 4 – zakończenia i powtarzalność (1–2 tygodnie)

1. **Trzy zakończenia**: Graal zabrany (moc i klątwa), Graal oddany Bractwu Płaczącego Mnicha (spokój, mniej złota),
   Graal zniszczony (wolność). Wybór po pokonaniu Ra, zależny od reputacji i przedmiotów.
2. **Ekran podsumowania**: liczba walk, zabitych wrogów, złota, wyborów moralnych, czas gry. Dane zbiera prosty
   `GameStats` w `Hero`, zapisywany razem z resztą.
3. **Kilka slotów zapisu** i autozapis przed piramidą (`SaveSystem` już ma podmienialny folder; wystarczy dodać
   nazwę pliku jako parametr).
4. **Nowa gra+**: start z 5. poziomem, wrogowie +30 % statystyk, nowy przedmiot.

## Rzeczy techniczne, które warto zrobić po drodze

- Wynieść teksty do plików zasobów (`.resx` lub `texts.pl.json`) – wtedy wersja angielska to tylko tłumaczenie.
- Zastąpić statyczne `GameIO` interfejsem `IGameIO` przekazywanym do `Combat`, `Tavern`, `Shop`. Pozwoli to
  testować pełne scenariusze (walka od początku do końca) bez konsoli, tak jak dziś robi to zewnętrzny bot.
- Dodać `dotnet format --verify-no-changes` do CI.
- Rozważyć bibliotekę `Spectre.Console` (tabele, paski HP, kolorowe menu) – działa na każdym systemie i nie wymaga
  zmiany architektury, bo cały dostęp do konsoli jest w `GameIO`.
- Muzyka poza Windows: NAudio nie ma backendu dla Linux/macOS; alternatywą jest `LibVLCSharp` lub wywołanie
  systemowego odtwarzacza. Na razie gra po prostu wyłącza muzykę.

## Spójna rama świata (propozycja narracyjna)

Zakończenie gry sugeruje, że w skrzyni są „przedmioty z innego świata”. Warto z tego zrobić zasadę: piramida Chufu
jest miejscem, w którym przenikają się epoki. Wtedy templariusze, whisky, miód z Malborka, kasyno i bogowie Egiptu
przestają być niespójnością, a stają się cechą świata. Najprościej osadzić akcję w pulpowym Kairze lat 20. XX wieku
(ekspedycje, port, przemytnicy, brytyjska kawaleria), a wszystko starsze traktować jako to, co piramida „wyrzuca”.
