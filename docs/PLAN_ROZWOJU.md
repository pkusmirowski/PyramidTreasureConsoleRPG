# Plan rozwoju rozgrywki – jak zrobić z Pyramid Treasure grę ciekawą, złożoną i rozbudowaną

## Kontekst

Po naprawach i modernizacji technicznej gra jest stabilna, testowalna i ma czystą architekturę
Domain / Engine / Ui / Infrastructure (opis w `README.md`, historia w `ANALIZA_I_CODE_REVIEW.md`). Rozgrywka jest jednak płytka: pętla to
„miasto → losowa walka z tabeli → tawerna → powtórz ok. 65 razy → piramida”. Analiza tego, co dziś ogranicza
złożoność, i systemy, które ją zbudują, są poniżej. Założenia: wdrażać całą mapę drogową w kolejności milestone'ów, treść trzymać w katalogach C#
(jak `EnemyCatalog`), warstwę 18+ rozwinąć w pełni, z konsekwencjami mechanicznymi.

## Analiza obecnej rozgrywki – co jest płytkie

| Obszar | Stan dziś | Skutek dla gracza |
|---|---|---|
| Świat | jeden przycisk „Udaj się w drogę”, brak przestrzeni | podróż z intro nie istnieje w mechanice |
| Decyzje | wybór ataku, kiedy pić miksturę, hazard | żadna decyzja poza walką nie ma trwałej konsekwencji |
| Walka | 3 ataki, wróg po wrogu, brak statusów, brak zachowań wrogów | każda walka wygląda tak samo |
| Rozwój postaci | tylko poziom; klasa = zestaw liczb | dwóch Wojowników na 20. poziomie jest identycznych |
| Ekonomia | złoto rośnie do ~27 000 na końcu, wydatki to mikstury i nocleg | złoto przestaje znaczyć po 8. poziomie |
| Fabuła | 4 rozmowy z barmanem, bramkowane poziomem | bohater nie ma powodu, by wracać do miasta poza leczeniem |
| Zakończenie | jedno, cliffhanger | brak powodu do drugiego przejścia |
| 18+ | opisy zgonów, hazard, noc w towarzystwie z losowym skutkiem | klimat jest, konsekwencji brak |

## Filary projektu

1. **Podróż**: mapa regionów, czas (dni), koszt drogi. Gracz wybiera, dokąd idzie i po co.
2. **Wybory z konsekwencjami**: reputacja u trzech frakcji, flagi fabularne, zadania od NPC zamiast poziomów.
3. **Walka taktyczna**: grupa wrogów naraz, wybór celu, statusy, obrona, zachowania wrogów, bossowie z fazami.
4. **Build postaci**: ekwipunek w trzech slotach, talenty co 5 poziomów, łupy z wrogów.
5. **Złoto, które boli**: drogi sprzęt, lichwiarz, łapówki, koszt karawany, nałóg.
6. **Dorosły ton przez mechanikę**: używki, dług, NPC z własnymi wątkami, przesłuchania, koszmary.
7. **Powtarzalność**: trzy zakończenia, podsumowanie, sloty zapisu, nowa gra+.

## Zasady wdrożenia

- Kolejność milestone'ów jest taka, żeby po każdym gra była przechodzalna od startu do Ra (bot i testy zielone).
- Treść w katalogach C# (`Domain/.../*Catalog.cs`) jako rekordy; każdy katalog ma test spójności.
- Nowe reguły w `Engine/` zwracają rekordy wyników; `Ui/` tylko je wyświetla (test architektoniczny pilnuje).
- `SaveData` dostaje wersję 3 z migracją z 2 (brakujące pola = wartości domyślne), żeby zapisy z PR #1 działały.
- Bot `tools/playbot.py` jest aktualizowany w każdym milestone (nowe menu = nowe reguły decyzji).
- Balans strojony skryptem symulacji (jak `scratchpad/balance.py`, przenieść do `tools/balance.py`).
- Jeden milestone = jedna seria commitów; push po zgodzie użytkownika, chyba że grozi utrata pracy.

---

## Milestone 1 – Świat: regiony, podróż, zdarzenia, reputacja, zadania (WDROŻONE)

> Stan: zrobione. Sześć regionów, 24 zdarzenia, 7 zadań (4 główne), reputacja trzech frakcji, licznik dni,
> zapis v3 z migracją z v2, 88 testów, bot przechodzi grę przez mapę i zadania.

### Model (Domain/World)
- `enum RegionId { Port, OldTown, Delta, Desert, Oasis, Pyramid }`.
- `record RegionDefinition(RegionId Id, string Name, IReadOnlyList<string> Arrival, StoryStage RequiredStage,
  int TravelDays, int TravelCost, EnemyDefinition[][] Encounters, IReadOnlyList<EventId> Events,
  IReadOnlyList<ItemId> ShopStock, bool HasTavern)`. `RegionCatalog` z sześcioma regionami; tabele z dzisiejszego
  `Engine/Encounters.cs` przechodzą do regionów (Port: złodzieje; Stare Miasto: opancerzeni, egzekutorzy;
  Delta: wilki, dziki; Pustynia: upadli rycerze, templariusze; Oaza: Płaczący Mnich; Piramida: Anubis, Ra).
- `enum Faction { Town, Underworld, Brotherhood }`; `Hero.Reputation: Dictionary<Faction,int>` (−100..100).
- `Hero.Day` (licznik dni), `Hero.Flags: HashSet<string>` (flagi fabularne), `Hero.CurrentRegion`.

### Zdarzenia (Domain/Events)
- `record GameEvent(EventId Id, RegionId Region, IReadOnlyList<string> Text, IReadOnlyList<EventChoice> Choices,
  EventCondition? Condition)`; `record EventChoice(string Text, SkillCheck? Check, IReadOnlyList<EventEffect> OnSuccess,
  IReadOnlyList<EventEffect> OnFailure, string ResultText)`.
- `EventEffect` jako rekord z rodzajem: `Gold`, `Hp`, `Exp`, `Reputation(Faction, delta)`, `Item(ItemId)`,
  `Fight(EnemyDefinition[])`, `SetFlag`, `Addiction`, `Debt`, `Status`. `SkillCheck(StatKind, int Difficulty)`:
  rzut `stat + k20 ≥ trudność`.
- `Engine/EventEngine`: `Pick(hero, region, rng) → GameEvent?` (warunki, flagi „raz na grę”),
  `Resolve(hero, ev, choiceIndex, rng) → EventResult(bool Success, string Text, IReadOnlyList<EventEffect> Applied,
  EnemyDefinition[]? Fight)`. Walkę wywołuje ekran przez `CombatScreen`.
- Treść startowa: 4–5 zdarzeń na region (ok. 25), np. rozbita karawana (obrabować / pomóc / minąć), ranny
  templariusz (dobić / opatrzyć / przesłuchać), kupiec z lotosem, celnik (łapówka / walka / zapłata),
  studnia z ciałem, koczownicy z mapą, mnich proszący o „ofiarę”.

### Zadania (Domain/Quests)
- `record QuestDefinition(QuestId Id, string Giver, string Title, IReadOnlyList<string> Intro, QuestObjective Objective,
  QuestReward Reward, StoryStage? UnlocksStage, Faction? RequiresFaction, int RequiredReputation)`.
- `QuestObjective`: `KillCount(EnemyDefinition, int)`, `ReachRegion(RegionId)`, `DeliverItem(ItemId, RegionId)`,
  `HaveFlag(string)`. `Hero.Quests: Dictionary<QuestId, QuestProgress>`.
- `Engine/QuestEngine`: `Available(hero)`, `Accept`, `Track(hero, event)` (wywoływane po walce, podróży, zdarzeniu),
  `Complete → QuestResult`. Etapy `StoryStage` przesuwają zadania, nie poziom (`Story.RequiredStageForTravel`
  zostaje tylko jako fallback dla piramidy).
- Dawcy: barman (główna linia: bandyci → wilki → karawana → piramida), kapitan portu (Town), przemytnik w Starym
  Mieście (Underworld), kapłanka w Oazie (Brotherhood). Po 2–3 zadania na dawcę.

### Podróż (Engine/TravelEngine)
- `Travel(hero, target, rng) → TravelResult(days, cost, IReadOnlyList<TravelStep>)`; krok = walka, zdarzenie albo
  spokojny dzień. Liczba kroków = `TravelDays`. Dzień zwiększa `Hero.Day`, tyka dług i nałóg (M4).
- Regiony z tawerną: Port i Oaza (Oaza ma inną obsadę baru i NPC).

### Ui
- `MapScreen` (lista regionów z wymaganiami i kosztem), `EventScreen` (tekst, wybory, wynik), `QuestLogScreen`,
  `TownScreen` dostaje „Mapa”, „Dziennik zadań”, a barman „Zadania”.
- `ConsoleGameIO`/`SpectreGameIO`: bez zmian interfejsu; ewentualnie `ShowMap` z tabelą.

### Zapis
- `SaveData v3`: `Day`, `Region`, `Reputation`, `Flags`, `Quests`; migracja v2→v3 w `JsonFileSaveStore` (domyślne).

### Testy i bot
- `EventEngineTests` (skill check deterministyczny przez `ScriptedRandomSource`, efekty), `QuestEngineTests`,
  `TravelEngineTests`, `CatalogTests` rozszerzone (każde zdarzenie ma ≥2 wybory, każdy region ≥1 grupę wrogów,
  zadania tworzą ciąg do `CaravanReady`). Bot: reguły „Mapa → region z najniższym wymaganiem”, „zdarzenie → opcja 1”.

---

## Milestone 2 – Ekwipunek, łupy, talenty (WDROŻONE)

> Stan: zrobione. 20 przedmiotów w trzech slotach, torba na 8, łupy w tabelach wrogów, sklepy regionalne z cenami
> od reputacji i amuletu, 12 talentów (poziomy 5 i 10), zapis v4. Mikstury zostały osobnym typem `Potion`
> (zamiast `Consumable`), bo przenoszenie ich nie dawało graczowi nic, a dotykało zapisu, zdarzeń i bota.
> Bossowie przestrojeni: bez sprzętu Ra wygrywa, z tierem 3 i talentami jest do przejścia (test pilnuje).

### Model (Domain/Items)
- `abstract record Item(ItemId Id, string Name, int Price, ItemSlot Slot)`; `Weapon(MinDmgBonus, MaxDmgBonus,
  CritBonus)`, `Armor(ArmorBonus, EvasionBonus)`, `Trinket(TrinketEffect)` (np. +10 % złota, +5 uniki, odporność
  na truciznę), `Consumable(ConsumableKind, Power)` – mikstury przechodzą tu (`Potion` staje się `Consumable`).
- `ItemCatalog`: 3 tiery broni i pancerza na klasę (ok. 20 przedmiotów), 6 amuletów, 6 konsumowalnych.
- `Hero.Equipment` (Weapon/Armor/Trinket), statystyki pochodne dodają bonusy; `Hero.Inventory` staje się
  `List<Item>` z limitem miejsc (12) – decyzja, co nieść.
- `EnemyDefinition.Loot: IReadOnlyList<(ItemId, int chancePercent)>`; `CombatEngine.Reward` losuje łup.
- Sklepy per region (`RegionDefinition.ShopStock`), ceny × mnożnik z reputacji `Town` (0,8–1,3).
- Sprzedaż przedmiotów za 40 % ceny.

### Talenty (Domain/Talents)
- `record TalentDefinition(TalentId, HeroClass, int Level, string Name, string Description, StatModifier Modifier)`;
  na poziomach 5/10/15/20 wybór 1 z 2 na klasę (12 talentów). Przykłady: Wojownik „Szał” (+25 % obrażeń poniżej
  30 % HP) / „Mur” (+15 pancerza); Łucznik „Celne oko” (+10 krytyk) / „Cień” (+10 uniki); Asasyn „Trucizna”
  (dłuższe zatrucie) / „Złodziejski fach” (+20 % złota z łupów). `Hero.Talents: HashSet<TalentId>`.

### Ui
- `EquipmentScreen` (załóż/zdejmij/sprzedaj/wypij), `ShopScreen` dla przedmiotów, wybór talentu w `CombatScreen`
  po awansie (przez `LevelUpEvent`).

### Balans
- Skrypt `tools/balance.py` z ekwipunkiem: Ra ma być wygrywalny w ≥50 % prób z tierem 3 i talentami, ≤20 % z bronią
  startową. Wrogowie i EXP przestrojone, bo część doświadczenia da zadania i zdarzenia.

---

## Milestone 3 – Walka taktyczna

- **Walka grupowa**: `CombatEngine` trzyma listę żywych wrogów; w turze bohater wybiera cel; wszyscy żywi wrogowie
  atakują w swojej turze. Kolejność z inicjatywy (Dex vs Agility) liczona raz na walkę. Zdarzenia dostają `Target`.
- **Statusy**: `record StatusEffect(StatusKind Kind, int Turns, int Power)` na bohaterze i wrogach; `StatusKind`:
  `Bleed` (obrażenia co turę), `Poison` (obrażenia + −trafienie), `Stun` (traci turę), `Guard` (−50 % obrażeń,
  następny cios krytyczny), `Fear` (−20 trafienie), `Berserk`. Tyka na początku tury właściciela w silniku.
  Źródła: Trzystronne cięcie → Bleed, Zatrute ostrze → Poison, Skupiony strzał → 25 % Stun, wrogowie z umiejętności.
- **Obrona** jako akcja (nakłada `Guard` na bohatera).
- **Umiejętności i zachowania wrogów** (`EnemyDefinition.Behavior`, `EnemyDefinition.Ability`):
  Złodziej – kradnie 5 % złota przy trafieniu, ucieka poniżej 30 % HP; Wilk – +20 % obrażeń, gdy żyje inny wilk;
  Dzik – szarża (podwójne obrażenia co 3. turę); Opancerzony złodziej – blok co drugą turę; Upadły rycerz –
  Fear przy pierwszym ciosie; Templariusz – ogłuszenie tarczą 20 %; Płaczący Mnich – lament (Fear na 2 tury,
  leczy się 10 %); Anubis – wskrzesza się raz z 30 % HP, gdy drugi Anubis żyje; Ra – fazy.
- **Bossowie z fazami**: Ra 100–50 % normalnie; 50 % wzywa 2 „Cienie Anubisa” (50 % statystyk); 20 % „Oko Słońca”
  – zapowiedziany cios ×3 w następnej turze (Obrona ratuje). `Engine/BossScripts` jako switch po `EnemyDefinition`.
- Ui: nagłówek walki z listą wrogów i statusami (Spectre: tabela z paskami, plain: linie).
- Testy: statusy tykają i wygasają, Guard halbuje, złodziej ucieka, Ra przechodzi fazy w skrypcie z `ScriptedRandomSource`.

---

## Milestone 4 – Warstwa dla dorosłych z konsekwencjami

- **Używki** (`Consumable`): whisky (+10 trafienie, −10 uniki na następną walkę), lotos (+25 % obrażeń na walkę,
  `Hero.Addiction += 1`; ≥3 → głód: −15 % statystyk, dopóki nie zażyje lub nie odczeka 5 dni z kacem),
  odtrutka. `Engine/AddictionService.Tick(hero)` przy każdym dniu.
- **Lichwiarz** w kasynie: pożyczka do 500 g, 10 % dziennie (`Hero.Debt`, `Hero.DebtDay`). Po 5 dniach niespłacony
  dług → zdarzenie „Egzekutorzy” w mieście (2 Opancerzonych złodziei + „Łamacz” – nowy wróg); przegrana
  = utrata 50 % złota i przedmiotu; wygrana = dług anulowany, `Underworld` −30.
- **NPC na górze**: 3 postacie (`NpcCatalog`): Zoja (uciekinierka, wątek zemsty na bandytach), Neferet (kapłanka
  Bractwa incognito), Ptaszek (złodziejka). `DialogueTree` jako katalog węzłów (`NodeId`, tekst, opcje z warunkami na
  reputację/flagi/etap, efekty). Romans = kolejne rozmowy odblokowują scenę (fade-to-black, tekst nastroju) i
  konsekwencję: plotka odsłania zdarzenie, wspólna noc = kradzież lub dar, „choroba” = `Status.Weak` na 2 walki,
  wątek Neferet wpływa na zakończenie.
- **Przesłuchanie jeńca** po walce z ludzkim wrogiem: „dobij / puść / przesłuchaj”. Przesłuchanie: rzut Str,
  odsłania łup lub zdarzenie w regionie; `Town` −10, `Underworld` +5, status „Koszmary” (−5 % HP przy każdym
  noclegu przez 3 noce).
- **Egzekucja/łaska** wpływa na reputację; barman i NPC komentują (linie zależne od reputacji w `Dialogues`).
- **Przekleństwa**: `GameSettings.ProfanityEnabled`; `Dialogues` ma warianty łagodne i ostre dla ok. 30 linii.

---

## Milestone 5 – Zakończenia, statystyki, sloty, nowa gra+

- **Trzy zakończenia** po Ra (`EndingScreen`): zabrać Graal (moc: NG+ z bonusem, klątwa: koszmary na stałe),
  oddać Bractwu (jeśli `Brotherhood ≥ 30` lub wątek Neferet), zniszczyć (jeśli flaga „mapa koczowników”).
  `Engine/EndingEngine.Available(hero)`.
- **Statystyki** (`Hero.Stats`: walki, zabici, złoto zdobyte/przegrane w kasynie, dni, wybory mroczne/jasne) +
  ekran podsumowania; zapisywane.
- **Sloty zapisu** (3) + autozapis przed piramidą: `ISaveStore` dostaje `slot`; menu wczytywania listuje sloty.
- **Nowa gra+**: start na 5. poziomie, wrogowie +30 % statystyk, talent bonusowy, jeden przedmiot z poprzedniej gry.
- Osiągnięcia proste (10 flag w ustawieniach) – opcjonalnie.

---

## Kolejność i szacunek

| Milestone | Zakres | Rozmiar | Kluczowe pliki |
|---|---|---|---|
| M1 | regiony, podróż, zdarzenia, reputacja, zadania, save v3 | duży (największy) | `Domain/World/*`, `Domain/Events/*`, `Domain/Quests/*`, `Engine/TravelEngine.cs`, `Engine/EventEngine.cs`, `Engine/QuestEngine.cs`, `Ui/MapScreen.cs`, `Ui/EventScreen.cs`, `Ui/QuestLogScreen.cs`, `Infrastructure/JsonFileSaveStore.cs` (migracja) |
| M2 | przedmioty, łupy, talenty, sklepy regionalne | średni | `Domain/Items/*`, `Domain/Talents/*`, `Domain/Hero.cs`, `Engine/CombatEngine.cs` (łup), `Ui/EquipmentScreen.cs`, `Ui/ShopScreen.cs` |
| M3 | walka grupowa, statusy, obrona, umiejętności wrogów, fazy bossów | duży | `Engine/CombatEngine.cs`, `Engine/BossScripts.cs`, `Domain/Enemy.cs`, `Ui/CombatScreen.cs`, `Ui/*GameIO.cs` (nagłówek walki) |
| M4 | używki, lichwiarz, NPC z dialogami, przesłuchania, przekleństwa | średni | `Domain/Npc/*`, `Engine/AddictionService.cs`, `Engine/DebtService.cs`, `Engine/DialogueEngine.cs`, `Ui/NpcScreen.cs`, `Ui/RestScreen.cs`, `Ui/CasinoScreen.cs` |
| M5 | zakończenia, statystyki, sloty, NG+ | mały–średni | `Engine/EndingEngine.cs`, `Ui/EndingScreen.cs`, `Infrastructure/JsonFileSaveStore.cs`, `Ui/MainMenuScreen.cs` |

M1 przed M2, bo sklepy regionalne i łupy potrzebują regionów. M3 po M2, bo statusy i umiejętności balansuje się
z ekwipunkiem. M4 po M3, bo używki i egzekutorzy to statusy i walki. M5 na końcu.

## Istniejący kod do ponownego użycia

- `CombatEngine` (zdarzenia, tury) – rozszerzany, nie przepisywany; `CombatEvent` dostaje nowe rekordy.
- `EventEngine` używa `ScriptedRandomSource`/`SeededRandomSource` z `Domain/RandomSource.cs` do testów.
- `Story.RequiredStageForTravel` i `StoryStage` zostają; zadania tylko przesuwają etap.
- `Dialogues` (czysty tekst) rozszerza się o warianty reputacyjne i wulgarne.
- `ScriptedGameIO` w testach do ekranów mapy, zdarzeń i dialogów NPC.
- `tools/playbot.py` i `verify.sh` jako smoke po każdym milestone.

## Weryfikacja

1. Po każdym milestone: `dotnet build -warnaserror`, `dotnet test` (nowe testy silników i katalogów),
   `dotnet format --verify-no-changes`, bot trzema klasami do napisu końcowego, test pseudoterminala Spectre.
2. Symulacja balansu (`tools/balance.py`) po M2 i M3: krzywa HP-loss na walkę 30–70 % na poziomie przedziału,
   Ra wygrywalny ≥50 % z pełnym buildem.
3. Test migracji zapisu: plik v2 z PR #1 wczytuje się w v3 z domyślnymi wartościami.
4. Ręczne przejście przez użytkownika na Windows po M1 i po M3 (największe zmiany w UI).

## Ryzyka

- Objętość tekstu (zdarzenia, dialogi NPC, warianty) to główny koszt; katalogi w C# ułatwiają test spójności,
  ale każda linia i tak musi zostać napisana.
- Walka grupowa zmienia balans całej gry – dlatego symulacja i bot są obowiązkowe przed commitem M3.
- Bot musi rozumieć nowe menu; reguły są proste (pierwsza dostępna opcja), ale trzeba je dopisać w każdym milestone.
- Zgodność zapisów: wersja 3 z migracją; starsze niż 2 nadal odrzucane.
