# Pyramid Treasure – szczegółowy opis

Skrócony opis i uruchomienie są w `README.md`. Tu jest pełna lista mechanik, struktura kodu i narzędzia.

## Rozgrywka w szczegółach

- Trzy poziomy trudności wybierane przy tworzeniu bohatera: **Łatwy** (wrogowie −20 %, nagrody +25 %), **Normalny**, **Trudny** (wrogowie +25 %, nagrody −15 %). Nowa gra+ zachowuje wybrany poziom i dokłada swoje +30 % na cykl.
- Kolorowe rysunki ASCII w konsoli: tytuł, portret klasy, każdy region po przybyciu, przeciwnik na początku walki (bossowie mają własne), tawerna, kasyno, śmierć i Graal. Wyłącza się je w ustawieniach.
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
- Na 20. poziomie, po zadaniach barmana, karawana zabiera bohatera pod piramidę: dwaj Anubisi, a potem bóg Ra. Przed wejściem gra robi autozapis.
- Trzy zakończenia: zabrać Graal (moc i klątwa), oddać go Bractwu (reputacja Bractwa albo słowo dane Neferet) albo zniszczyć w komnacie z mapy koczowników. Po napisach podsumowanie wyprawy: walki, zabici, złoto, kasyno, wybory mroczne i jasne.
- Nowa gra+: ten sam bohater rusza ponownie od 5. poziomu z bronią i talentami, wrogowie są o 30 % silniejsi na każdy cykl, a bonus na start zależy od wybranego zakończenia.
- Trzy sloty zapisu; zapis z poprzednich wersji gry wczytuje się jako slot 1.
- W narracji dowolny klawisz pomija pauzę, `Esc` pomija cały tekst. Prędkość tekstu i muzykę ustawia się w menu głównym.

## Struktura kodu

| Katalog | Zawartość |
|---|---|
| `Domain/` | Modele i czyste reguły: `Hero`, definicje klas (`HeroClasses`), katalog wrogów z łupami (`EnemyCatalog`), regiony (`World/RegionCatalog`), zdarzenia (`Events/EventCatalog`), zadania (`Quests/QuestCatalog`), przedmioty (`Items/ItemCatalog`), talenty (`Talents/TalentCatalog`), postacie z dialogami (`Npc/NpcCatalog`), mikstury i używki, fabuła, teksty, losowość |
| `Engine/` | Logika bez UI: `CombatEngine` (walka jako zdarzenia), `TravelEngine`, `EventEngine`, `QuestEngine`, `CasinoEngine`, `DialogueEngine`, `DayService` (dług, nałóg), `DebtService`, `InterrogationService`, `EndingEngine`, serwisy baru, sklepu i noclegu |
| `Ui/` | Ekrany konsolowe rozmawiające tylko z `IGameIO` (region, mapa, zdarzenia, zleceniodawcy, dziennik, walka, tawerna, kasyno, pokoje i postacie na górze, sklep, sakwa, zakończenie z podsumowaniem); dwie implementacje: `SpectreGameIO` (domyślna) i `ConsoleGameIO` (`--plain`) |
| `Infrastructure/` | Zapis gry w trzech slotach plus autozapis i ustawienia (JSON z generatorem źródeł), odtwarzacz muzyki (NAudio) |
| `Program.cs` | Rejestracja zależności (Microsoft.Extensions.DependencyInjection) i start gry |
| `tests/` | Testy xUnit: wzory walki, awanse, silnik walki i kasyna, zapis/odczyt, ekrany z dublerem konsoli, reguły architektury |
| `tools/` | `playbot.py` – bot grający przez potok od startu do napisu końcowego (smoke test w CI); `BalanceSim/` – symulacja balansu walk na prawdziwym silniku |
| `docs/` | Analiza i code review, plan rozwoju, checklista testu ręcznego na Windows |

## Testy

```bash
dotnet test PyramidTreasureConsoleRPG.sln
```

Bot grający całą grę (wymaga Pythona 3):

```bash
dotnet build -c Release
python3 tools/playbot.py bin/Release/net9.0/PyramidTreasureConsoleRPG.dll 1
```

Symulacja balansu na prawdziwym silniku walki (szansa wygranej i utrata HP dla każdej klasy, regionu i puli wrogów; drugi argument to cykl nowej gry+):

```bash
dotnet run --project tools/BalanceSim -c Release -- 200
```

Reguły stylu kolekcji (IDE0300–IDE0305) są tylko sugestią: analizator z SDK 9 zgłasza ich więcej niż SDK 8, a formatowanie w CI nie powinno być czerwone przez wersję narzędzi.

CI (GitHub Actions) sprawdza formatowanie, buduje projekt z ostrzeżeniami jako błędami, uruchamia testy i przechodzi grę botem.
