# PyramidTreasureConsoleRPG

Konsolowa gra RPG o wyprawie po świętego Graala ukrytego w piramidzie Chufu. Gra dla dorosłych (18+).

Wybierasz klasę i poziom trudności, podróżujesz przez sześć regionów, wykonujesz zadania, walczysz z grupami wrogów,
zdobywasz sprzęt i talenty, a na końcu stajesz przed bogiem Ra i jednym z trzech zakończeń. Po napisach można zacząć
nową grę+.

## Uruchomienie

Potrzebny jest [.NET SDK 9](https://dotnet.microsoft.com/download).

```bash
dotnet run -c Release
```

Jeśli konsola źle rysuje menu lub kolory, użyj zwykłego trybu tekstowego:

```bash
dotnet run -c Release -- --plain
```

Zapisy trafiają do `%APPDATA%\GreatPyramidTreasureRPG_DataSave` (Windows) lub `~/.config/GreatPyramidTreasureRPG_DataSave` (Linux).
Muzyka działa tylko na Windows.

## Testy

```bash
dotnet test PyramidTreasureConsoleRPG.sln
```

## Więcej

- [Szczegółowy opis rozgrywki, struktura kodu i narzędzia](docs/OPIS_GRY.md)
- [Plan rozwoju](docs/PLAN_ROZWOJU.md)
- [Checklista testu ręcznego](docs/CHECKLISTA_TESTU.md)

## Licencja

GPL-3.0 – patrz `LICENSE`.
