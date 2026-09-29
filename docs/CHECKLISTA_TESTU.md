# Checklista testu ręcznego (Windows)

Rzeczy, których nie sprawdzi ani bot, ani testy w kontenerze Linux. Każdy punkt: OK / błąd + krótka notatka.
Uruchomienie: `dotnet run -c Release` (Spectre) albo `dotnet run -c Release -- --plain` (zwykła konsola).

## Konsola i wygląd
- [ ] Windows Terminal: rysunek tytułowy, portret klasy, rysunek regionu i wroga są kolorowe i nie łamią się.
- [ ] Stary `cmd.exe`: to samo w trybie `--plain`; polskie znaki czytelne (jeśli nie: `chcp 65001`).
- [ ] Ustawienia → „Rysunki ASCII: wyłączone” faktycznie ukrywa rysunki, a po ponownym uruchomieniu ustawienie zostaje.
- [ ] Prędkość tekstu: 0 = bez pauz, 3 = wolno; `Esc` pomija całą narrację, dowolny klawisz jedną pauzę.

## Muzyka (tylko Windows, NAudio)
- [ ] Muzyka gra po starcie, „Muzyka: wyłączona” ją zatrzymuje, głośność 0–100 działa.
- [ ] Brak pliku muzyki nie wywala gry (komunikat, gra idzie dalej).

## Nowa gra i trudność
- [ ] Po klasie pojawia się wybór trudności; statystyki bohatera pokazują „Trudność: … (wrogowie X %, nagrody Y %)”.
- [ ] Na trudnym pierwszy złodziej ma 27 HP zamiast 22; na łatwym 17.

## Zapisy
- [ ] „Zapisz grę” pokazuje trzy sloty; nadpisanie pyta o potwierdzenie.
- [ ] Zapis z poprzedniej wersji gry (`%APPDATA%\PyramidTreasureConsoleRPG\DataSave.json`) wczytuje się jako slot 1.
- [ ] Przed wejściem do piramidy pojawia się „Autozapis wykonany”; po śmierci u Ra da się wczytać autozapis.

## Usługi poboczne
- [ ] Kasyno: po jednej rundzie ruletki, automatu, blackjacka i kości; stan konta zgadza się z wynikiem.
- [ ] Lichwiarz: pożyczka 100 g, po 5 dniach bez spłaty w porcie lub Starym Mieście zdarzenie „Egzekutorzy”.
- [ ] Sklep: kupno i sprzedaż wyposażenia, ceny zależne od reputacji Miasta.
- [ ] Używki: whisky i lotos z Sakwy przed walką; po trzech dawkach lotosu i dwóch dniach bez – komunikat o głodzie.
- [ ] Jeniec po walce z ludźmi: przesłuchanie daje koszmary (nocleg leczy do 90 %).

## Fabuła
- [ ] Zadania barmana otwierają Oazę i piramidę; dziennik zadań pokazuje postęp.
- [ ] Trzy zakończenia: Graal zawsze, Bractwo po ślubie Neferet lub reputacji ≥ 30, zniszczenie po mapie koczowników.
- [ ] Nowa gra+ startuje na 5. poziomie z bronią i talentami, wrogowie widocznie silniejsi.
