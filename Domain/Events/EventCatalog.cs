namespace PyramidTreasureConsoleRPG.Domain;

/// <summary>Zdarzenia losowe z wyborami. Treść dla dorosłych: przemoc, używki, moralnie szare decyzje.</summary>
public static class EventCatalog
{
    private static readonly GameEvent[] Events =
    [
        // ---------------- PORT ----------------
        new(
            "port_drunk_sailor", RegionId.Port, "Pijany marynarz",
            [
                "Na nabrzeżu leży marynarz, zbyt pijany, by dojść do statku. Sakwa przy pasie wygląda na ciężką.",
                "Dwóch portowych szczurów już go obchodzi jak sępy.",
            ],
            [
                new("Odgoń szczury i zanieś go na statek", [new ReputationEffect(Faction.Town, 5), new GoldEffect(10)],
                    "Bosman wciska ci 10 sztuk złota. \"Za fatygę. I za to, że nie jesteś jak oni.\"",
                    new SkillCheck(StatKind.Strength, 11), [new HpPercentEffect(-10), new ReputationEffect(Faction.Town, 3)],
                    "Szczury nie odpuszczają bez bójki. Dostajesz w twarz, ale marynarz trafia na pokład."),
                new("Wyprzedź szczury i obrób go sam", [new GoldEffect(18), new ReputationEffect(Faction.Underworld, 5), new ReputationEffect(Faction.Town, -5)],
                    "Sakwa jest twoja. Szczury patrzą z uznaniem, a strażnik przy bramie odwraca wzrok.",
                    new SkillCheck(StatKind.Dexterity, 10), [new ReputationEffect(Faction.Town, -8)],
                    "Marynarz budzi się w połowie i wrzeszczy na cały port. Uciekasz z pustymi rękami."),
                new("Idź dalej", [], "Nie twoja sprawa. Wrzask za plecami cichnie po chwili."),
            ]),
        new(
            "port_customs", RegionId.Port, "Celnik",
            [
                "Celnik w za ciasnym mundurze zatrzymuje cię przy bramie. \"Opłata za noszenie broni w porcie. Dwadzieścia sztuk.\"",
                "Za nim stoi dwóch \"pomocników\" o twarzach, które widziałeś już na listach gończych.",
            ],
            [
                new("Zapłać 20 sztuk złota", [new ReputationEffect(Faction.Underworld, 3)], "Celnik chowa złoto do własnej kieszeni. \"Miłego dnia.\"", GoldCost: 20),
                new("Odmów", [new FightEffect([EnemyCatalog.Thief, EnemyCatalog.Thief])], "\"Pomocnicy\" wyciągają noże. Celnik znika za rogiem."),
                new("Zapytaj, od kiedy złodzieje noszą mundury", [new ReputationEffect(Faction.Town, 5), new ExpEffect(120)],
                    "Celnik blednie. Prawdziwy strażnik przy bramie słyszał wszystko i bierze go pod ramię.",
                    new SkillCheck(StatKind.Dexterity, 12), [new FightEffect([EnemyCatalog.Thief, EnemyCatalog.Thief])],
                    "Nikt nie słyszy. \"Pomocnicy\" wyciągają noże."),
            ]),
        new(
            "port_fisherman", RegionId.Port, "Rybak z plotką",
            [
                "Stary rybak naprawia sieć i mruczy pod nosem o skrytce przemytników w Starym Mieście.",
                "\"Za dziesięć sztuk złota powiem ci, gdzie. Za darmo powiem tylko, że tam śmierdzi.\"",
            ],
            [
                new("Zapłać 10 sztuk złota", [new FlagEffect("rumor:oldtown"), new ExpEffect(100)], "\"Za paserem, trzecie drzwi, pukaj dwa razy.\" Zapamiętujesz.", GoldCost: 10),
                new("Postaw mu rum zamiast płacić", [new FlagEffect("rumor:oldtown"), new ExpEffect(60), new ReputationEffect(Faction.Town, 3)],
                    "Po trzeciej szklance mówi wszystko i jeszcze więcej.", new SkillCheck(StatKind.Vitality, 11), [new HpPercentEffect(-5)],
                    "Po trzeciej szklance to ty mówisz wszystko. Rano boli cię głowa, a rybaka nie ma."),
                new("Machnij ręką", [], "Plotki są tanie. Sieć rybaka i tak nie ma dziur."),
            ],
            OncePerGame: true),
        new(
            "port_brawl", RegionId.Port, "Bójka w porcie",
            [
                "Przed tawerną dwóch tragarzy okłada trzeciego. Krew na bruku, gapie obstawiają zakłady.",
            ],
            [
                new("Rozdziel ich", [new ReputationEffect(Faction.Town, 10), new ExpEffect(80)], "Jeden cios w szczękę, jeden w brzuch. Gapie buczą, ale tragarz dziękuje.",
                    new SkillCheck(StatKind.Strength, 13), [new HpPercentEffect(-12)], "Wpadasz między pięści. Rozdzielasz ich, ale własną twarzą."),
                new("Dołącz do zabawy", [new FightEffect([EnemyCatalog.Thief, EnemyCatalog.Thief]), new ReputationEffect(Faction.Underworld, 5)], "Tragarze odwracają się do ciebie. Gapie podnoszą stawki."),
                new("Obstaw zakład i patrz", [new GoldEffect(15)], "Trafiasz. Trzeci tragarz kończy w rynsztoku, ty z zyskiem.",
                    new SkillCheck(StatKind.Dexterity, 11), [new GoldEffect(-10)], "Pudło. Bukmacher zgarnia twoje dziesięć sztuk i nawet nie mrugnie."),
            ]),

        new(
            "debt_collectors_port", RegionId.Port, "Egzekutorzy",
            [
                "Trzech mężczyzn czeka przy bramie. Dwóch w kradzionych zbrojach, trzeci z młotem o rękojeści owiniętej drutem.",
                "\"Lichwiarz kłania się. Mówi, że termin minął. My nie jesteśmy od gadania.\"",
            ],
            [
                new("Walcz", [new FightEffect([EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief, EnemyCatalog.Breaker]), new ClearDebtEffect(), new ClearFlagEffect("debt:overdue"), new ReputationEffect(Faction.Underworld, -30)],
                    "Jeśli przeżyjesz, dług przestanie istnieć. Razem z lichwiarzem, który go udzielił."),
                new("Spłać dług na miejscu", [new ClearDebtEffect(), new ClearFlagEffect("debt:overdue")], "Łamacz liczy monety dwa razy. Odchodzą bez słowa.", RequiresFlag: "debt:canpay"),
            ],
            RequiresFlag: "debt:overdue", Forced: true),
        new(
            "debt_collectors_oldtown", RegionId.OldTown, "Egzekutorzy",
            [
                "W zaułku zastępują ci drogę: dwóch w kradzionych zbrojach i Łamacz z młotem.",
                "\"Lichwiarz kłania się. Termin minął.\"",
            ],
            [
                new("Walcz", [new FightEffect([EnemyCatalog.ArmoredThief, EnemyCatalog.ArmoredThief, EnemyCatalog.Breaker]), new ClearDebtEffect(), new ClearFlagEffect("debt:overdue"), new ReputationEffect(Faction.Underworld, -30)],
                    "Jeśli przeżyjesz, dług przestanie istnieć."),
                new("Spłać dług na miejscu", [new ClearDebtEffect(), new ClearFlagEffect("debt:overdue")], "Łamacz liczy monety dwa razy. Odchodzą bez słowa.", RequiresFlag: "debt:canpay"),
            ],
            RequiresFlag: "debt:overdue", Forced: true),

        // ---------------- STARE MIASTO ----------------
        new(
            "oldtown_beggar", RegionId.OldTown, "Żebrak",
            [
                "Żebrak bez nogi wyciąga rękę. \"Na chleb, panie. Albo na lotos, żeby nie bolało.\"",
            ],
            [
                new("Daj 5 sztuk złota", [new ReputationEffect(Faction.Town, 5)], "Nie pytasz, na co pójdą. On nie udaje wdzięczności.", GoldCost: 5),
                new("Kopnij i idź dalej", [new ReputationEffect(Faction.Underworld, 4), new ReputationEffect(Faction.Town, -6)], "Zaułek zapamiętuje takie rzeczy. Przemytnicy też."),
                new("Minąć bez słowa", [], "Ręka opada. Następny."),
            ]),
        new(
            "oldtown_ambush", RegionId.OldTown, "Zasadzka",
            [
                "Zaułek kończy się ślepo. Za tobą stają trzej: dwóch z nożami i jeden w kradzionej zbroi.",
                "\"Sakwa albo flaki. Wybieraj szybko.\"",
            ],
            [
                new("Walcz", [new FightEffect([EnemyCatalog.ArmoredThief, EnemyCatalog.Thief, EnemyCatalog.Thief])], "Nikt tu nie usłyszy krzyków. Dobrze, bo będą."),
                new("Rzuć 30 sztuk złota i przejdź", [new ReputationEffect(Faction.Underworld, 2)], "Zbierają monety z błota. Pozwalają ci przejść.", GoldCost: 30),
                new("Przebij się przez lukę w murze", [new ExpEffect(150)], "Cegły sypią się za tobą. Nie gonią.",
                    new SkillCheck(StatKind.Dexterity, 15), [new FightEffect([EnemyCatalog.ArmoredThief, EnemyCatalog.Thief, EnemyCatalog.Thief])], "Luka jest za wąska. Odwracasz się z bronią w ręku."),
            ]),
        new(
            "oldtown_lotus_dealer", RegionId.OldTown, "Diler lotosu",
            [
                "W bramie chudy chłopak z rozszerzonymi źrenicami pokazuje ci woreczek suszonych płatków.",
                "\"Lotos. Pierwsza fajka gratis. Po niej nie czujesz ciosów. Ani niczego innego.\"",
            ],
            [
                new("Zapal z nim", [new HpPercentEffect(20), new ExpEffect(50), new FlagEffect("lotus:tried"), new ReputationEffect(Faction.Underworld, 5)],
                    "Świat mięknie. Ból znika. Budzisz się po godzinie z posmakiem popiołu i dziwnym głodem."),
                new("Wydaj go strażnikom", [new ReputationEffect(Faction.Town, 10), new ReputationEffect(Faction.Underworld, -12), new GoldEffect(15)],
                    "Strażnik płaci nagrodę i łamie chłopakowi palce na twoich oczach. Miasto jest wdzięczne."),
                new("Odejdź", [], "Chłopak wzrusza ramionami. \"Wrócisz.\""),
            ]),
        new(
            "oldtown_gambler", RegionId.OldTown, "Hazardzista z długiem",
            [
                "Mężczyzna z podbitym okiem chwyta cię za rękaw. \"Lichwiarz wysłał po mnie dwóch. Pomóż, oddam podwójnie.\"",
                "Dwóch zbirów właśnie wchodzi w zaułek.",
            ],
            [
                new("Stań w jego obronie", [new FightEffect([EnemyCatalog.Thief, EnemyCatalog.Thief]), new ReputationEffect(Faction.Town, 5), new GoldEffect(25)], "Zbiry nie spodziewały się towarzystwa."),
                new("Ściągnij dług dla lichwiarza", [new GoldEffect(40), new ReputationEffect(Faction.Underworld, 10), new ReputationEffect(Faction.Town, -10)],
                    "Zbiry patrzą z podziwem, jak wytrząsasz z niego ostatnie monety. Lichwiarz zapamięta.",
                    new SkillCheck(StatKind.Strength, 12), [new GoldEffect(12)], "Wyrywa się i ucieka. Zostajesz z połową."),
                new("Wyrwij rękaw i idź", [], "Krzyk za plecami urywa się szybko."),
            ]),
        new(
            "oldtown_fence", RegionId.OldTown, "Paser",
            [
                "Paser ogląda twój ekwipunek. \"Mam towar, który spadł z wozu. Ty masz twarz, której nikt nie zna. Wymienimy się?\"",
            ],
            [
                new("Sprzedaj mu \"znalezione\" rzeczy", [new GoldEffect(60), new ReputationEffect(Faction.Underworld, 8), new ReputationEffect(Faction.Town, -8)], "Sześćdziesiąt sztuk. Nie pytasz, skąd wóz."),
                new("Odmów", [new ReputationEffect(Faction.Town, 2)], "\"Uczciwy. Ci giną pierwsi.\""),
            ],
            OncePerGame: true, RequiresFlag: "rumor:oldtown"),

        // ---------------- DELTA ----------------
        new(
            "delta_wounded_templar", RegionId.Delta, "Ranny templariusz",
            [
                "Pod cyprysem leży templariusz z wilczymi śladami na udzie. Krew sączy się przez palce.",
                "\"Dobij albo pomóż. Tylko nie stój tak.\"",
            ],
            [
                new("Dobij go i zabierz sakwę", [new GoldEffect(35), new ReputationEffect(Faction.Town, -10), new ReputationEffect(Faction.Brotherhood, -10)], "Nie krzyczy. Sakwa jest ciężka, sumienie lżejsze, niż myślałeś."),
                new("Opatrz ranę", [new ReputationEffect(Faction.Brotherhood, 15), new FlagEffect("templar:saved"), new ExpEffect(150)],
                    "Zaciskasz pas nad raną. \"Bractwo nie zapomina długów.\" Odchodzi kulejąc.",
                    new SkillCheck(StatKind.Vitality, 12), [new HpPercentEffect(-8), new ReputationEffect(Faction.Brotherhood, 5)], "Krew tryska na ciebie, zanim opanujesz krwotok. Przeżyje, ale nie dzięki twoim rękom."),
                new("Przesłuchaj go", [new ExpEffect(300), new FlagEffect("rumor:desert")],
                    "Wystarczy nacisnąć ranę. Mówi o patrolach na szlaku i o tym, czego strzegą w oazie.",
                    new SkillCheck(StatKind.Strength, 13), [new ReputationEffect(Faction.Brotherhood, -10)], "Mdleje, zanim cokolwiek powie. Zostawiasz go wilkom."),
            ]),
        new(
            "delta_caravan_wreck", RegionId.Delta, "Rozbita karawana",
            [
                "Przewrócone wozy, martwe wielbłądy, ślady wilczych łap. Wśród skrzyń jęczy kilku ocalałych.",
            ],
            [
                new("Przeszukaj skrzynie", [new GoldEffect(50), new ReputationEffect(Faction.Town, -15), new ReputationEffect(Faction.Underworld, 10)], "Ocalali patrzą, jak ładujesz ich dobytek do sakwy. Nie mają siły protestować."),
                new("Pomóż ocalałym (20 sztuk złota na opatrunki)", [new ReputationEffect(Faction.Town, 15), new ExpEffect(200)], "Kupiec obiecuje pamiętać. Kupcy czasem pamiętają.", GoldCost: 20),
                new("Omiń wrak", [], "Jęki cichną za plecami. Wilki wrócą przed nocą."),
            ]),
        new(
            "delta_wolf_den", RegionId.Delta, "Legowisko wilków",
            [
                "Jaskinia cuchnie mokrą sierścią. W środku słychać skomlenie szczeniąt i warczenie matki.",
            ],
            [
                new("Wybij stado", [new FightEffect([EnemyCatalog.Wolf, EnemyCatalog.Wolf, EnemyCatalog.Wolf]), new ExpEffect(300)], "Matka atakuje pierwsza."),
                new("Zakradnij się po skóry ze ścian jaskini", [new GoldEffect(30), new ExpEffect(100)], "Wychodzisz z trzema skórami, zanim wilki cię zwietrzą.",
                    new SkillCheck(StatKind.Dexterity, 14), [new FightEffect([EnemyCatalog.Wolf, EnemyCatalog.Wolf])], "Gałązka pęka pod stopą. Dwa wilki wyskakują z ciemności."),
                new("Odejdź", [], "Szczenięta dorosną. Ktoś inny będzie miał problem."),
            ]),
        new(
            "delta_well", RegionId.Delta, "Studnia z ciałem",
            [
                "W przydrożnej studni pływa napuchnięte ciało. Przy cembrowinie leży tobołek.",
            ],
            [
                new("Wyłów ciało i przeszukaj", [new GoldEffect(25), new ExpEffect(50)], "Kilka monet i list, którego nie da się odczytać.",
                    new SkillCheck(StatKind.Vitality, 10), [new GoldEffect(25), new HpPercentEffect(-15)], "Smród i to, co pływa w wodzie, zostają z tobą na dłużej. Wymiotujesz, ale monety są twoje."),
                new("Pochowaj go", [new ReputationEffect(Faction.Town, 5), new ReputationEffect(Faction.Brotherhood, 5), new ExpEffect(100)], "Kopanie zajmuje godzinę. Nikt nie widzi. Ty widzisz."),
                new("Zabierz tobołek i idź", [new GoldEffect(10)], "W tobołku suchary i dziesięć sztuk. Studnia zostaje zatruta."),
            ]),
        new(
            "delta_hunter", RegionId.Delta, "Myśliwy",
            [
                "Myśliwy przy ognisku obdziera dzika. \"Siadaj. Mam miksturę na sprzedaż i historię za darmo.\"",
            ],
            [
                new("Kup miksturę za 12 sztuk złota", [new PotionEffect(PotionKind.Small, 1)], "Taniej niż u alchemika. Pachnie ziołami i dzikiem.", GoldCost: 12),
                new("Posłuchaj historii", [new ExpEffect(120), new FlagEffect("hunter:met")], "Opowiada o rycerzach, którzy wyszli z piasku i nie mieli twarzy. Nie śmieje się."),
                new("Idź dalej", [], "Ognisko zostaje za tobą."),
            ]),

        // ---------------- PUSTYNIA ----------------
        new(
            "desert_sandstorm", RegionId.Desert, "Burza piaskowa",
            [
                "Horyzont brązowieje. Za kwadrans nie będziesz widział własnej dłoni.",
            ],
            [
                new("Zakop się przy skale i przeczekaj", [new ExpEffect(80)], "Piasek grzebie cię po szyję, ale burza mija.",
                    new SkillCheck(StatKind.Vitality, 13), [new HpPercentEffect(-25)], "Piasek wciska się w oczy, usta, rany. Wychodzisz z tego na wpół żywy."),
                new("Idź dalej przez burzę", [new HpPercentEffect(-15), new ExpEffect(200)], "Każdy krok to walka. Wygrywasz ją, ale skóra schodzi ci płatami."),
            ]),
        new(
            "desert_nomads", RegionId.Desert, "Koczownicy z mapą",
            [
                "Koczownicy przy studni rozkładają na kocu mapę piramidy. Stare, bardzo stare rysunki komnat.",
                "\"Osiemdziesiąt sztuk. Albo twoja krew, jeśli spróbujesz inaczej.\"",
            ],
            [
                new("Kup mapę za 80 sztuk złota", [new FlagEffect("map:nomads"), new ExpEffect(200)], "Mapa pokazuje komnatę pod tronem, o której nie wspominają żadne legendy.", GoldCost: 80),
                new("Ukradnij mapę nocą", [new FlagEffect("map:nomads"), new ReputationEffect(Faction.Underworld, 10), new ExpEffect(250)],
                    "Wartownik śpi. Mapa jest twoja, a koczownicy rano będą przeklinać wiatr.",
                    new SkillCheck(StatKind.Dexterity, 16), [new FightEffect([EnemyCatalog.FallenKnight, EnemyCatalog.FallenKnight])], "Wartownik nie spał. Koczownicy budzą coś w piasku, co walczy za nich."),
                new("Pogadaj o szlaku", [new ExpEffect(150)], "Opowiadają o karawanach, które doszły do piramidy i nie wróciły. Wszystkie."),
            ],
            OncePerGame: true),
        new(
            "desert_templar_patrol", RegionId.Desert, "Patrol templariuszy",
            [
                "Dwóch templariuszy zastępuje ci drogę. Krzyże na płaszczach wyblakły, oczy nie.",
                "\"Szlak jest zamknięty dla obcych. Zawróć albo zapłać za przejście.\"",
            ],
            [
                new("Zapłać 50 sztuk złota", [], "Biorą złoto bez słowa. Krzyże nie przeszkadzają im w interesach.", GoldCost: 50),
                new("Walcz", [new FightEffect([EnemyCatalog.Templar, EnemyCatalog.Templar])], "Dobywają mieczy jednocześnie, jak jeden człowiek."),
                new("Powołaj się na Bractwo", [new ExpEffect(200), new ReputationEffect(Faction.Brotherhood, 5)], "Wymieniają spojrzenia i schodzą z drogi. Bractwo ma długie ręce.",
                    RequiresFaction: Faction.Brotherhood, RequiredReputation: 15),
                new("Zblefuj, że jesteś kurierem kapłanki", [new ExpEffect(180)], "\"Neferet nie wspominała.\" Ale przepuszczają.",
                    new SkillCheck(StatKind.Dexterity, 15), [new FightEffect([EnemyCatalog.Templar, EnemyCatalog.Templar])], "\"Neferet nie ma kurierów.\" Miecze."),
            ]),
        new(
            "desert_mirage", RegionId.Desert, "Fatamorgana",
            [
                "Na horyzoncie miasto z białymi wieżami i wodą. Wiesz, że go tam nie ma. Nogi i tak chcą iść.",
            ],
            [
                new("Idź w stronę miasta", [new HpPercentEffect(-10), new ExpEffect(60)], "Godzinę później miasto jest tak samo daleko. Woda w bukłaku się skończyła."),
                new("Odpocznij w cieniu skały", [new HpPercentEffect(10)], "Cień jest prawdziwy. Miasto znika o zmierzchu."),
            ]),
        new(
            "desert_dying_man", RegionId.Desert, "Umierający",
            [
                "Człowiek bez sandałów pełznie po piasku. Wargi ma czarne. \"Wody... zapłacę... wszystko...\"",
            ],
            [
                new("Oddaj mu wodę i zapasy (10 sztuk złota)", [new ReputationEffect(Faction.Town, 10), new ReputationEffect(Faction.Brotherhood, 5), new ExpEffect(120)], "Pije jak zwierzę. Nie ma czym zapłacić. Wiedziałeś.", GoldCost: 10),
                new("Zabierz mu sakwę", [new GoldEffect(35), new ReputationEffect(Faction.Town, -12)], "Sakwa jest cięższa niż on. Nie protestuje. Nie może."),
                new("Idź dalej", [], "Pustynia zamyka za tobą ślady."),
            ]),

        // ---------------- OAZA ----------------
        new(
            "oasis_monk", RegionId.Oasis, "Mnich prosi o ofiarę",
            [
                "Mnich w zgrzebnym habicie, z oczami mokrymi od ciągłego płaczu, wyciąga miskę.",
                "\"Ofiara dla Płaczącego. Pięćdziesiąt sztuk, a On zapamięta twoją twarz.\"",
            ],
            [
                new("Złóż ofiarę (50 sztuk złota)", [new ReputationEffect(Faction.Brotherhood, 20), new FlagEffect("offering:made")], "Mnich dotyka twojego czoła mokrym palcem. Czujesz zimno przez resztę dnia.", GoldCost: 50),
                new("Odmów grzecznie", [new ReputationEffect(Faction.Brotherhood, -5)], "\"On i tak zapamięta.\""),
                new("Wyśmiej go", [new FightEffect([EnemyCatalog.CryingMonk]), new ReputationEffect(Faction.Brotherhood, -20)], "Płacz zmienia się w lament. Lament w coś gorszego."),
            ]),
        new(
            "oasis_bath", RegionId.Oasis, "Kąpiel w oazie",
            [
                "Woda w oazie jest zimna i czysta. Kobiety piorące na brzegu udają, że nie patrzą.",
            ],
            [
                new("Wykąp się i odpocznij", [new HpPercentEffect(30)], "Piasek schodzi z ciebie razem ze zmęczeniem. Ktoś zostawił ci na brzegu świeży chleb."),
                new("Nie ma czasu", [], "Woda zostaje za tobą."),
            ]),
        new(
            "oasis_smuggler_cache", RegionId.Oasis, "Skrytka przemytników",
            [
                "Pod palmą, dokładnie tam, gdzie mówił rybak, leży zakopana skrzynia z pieczęcią Hasana.",
            ],
            [
                new("Zabierz zawartość", [new GoldEffect(120), new ReputationEffect(Faction.Underworld, -15), new FlagEffect("cache:looted")], "Sto dwadzieścia sztuk. Hasan dowie się do tygodnia."),
                new("Zgłoś skrytkę kapłance", [new ReputationEffect(Faction.Brotherhood, 12), new ReputationEffect(Faction.Underworld, -5), new ExpEffect(200)], "Kapłanka każe skrzynię spalić. Nie pyta, skąd wiedziałeś."),
                new("Zakop z powrotem", [new ReputationEffect(Faction.Underworld, 5)], "Hasan doceni dyskrecję. Albo nigdy się nie dowie."),
            ],
            OncePerGame: true, RequiresFlag: "rumor:oldtown"),
        new(
            "oasis_ritual", RegionId.Oasis, "Rytuał Bractwa",
            [
                "Nocą w świątyni Bractwo odprawia rytuał. Śpiew, płacz, zapach palonego lotosu i coś, co nie jest śpiewem.",
            ],
            [
                new("Patrz z cienia", [new ExpEffect(400), new FlagEffect("ritual:seen")], "Widzisz, jak mnich wchodzi w ogień i wychodzi bez twarzy. Rozumiesz, kim są Płaczący Mnisi.",
                    new SkillCheck(StatKind.Dexterity, 13), [new FightEffect([EnemyCatalog.CryingMonk, EnemyCatalog.Templar])], "Kamyk pod stopą. Śpiew urywa się. Odwracają się wszyscy naraz."),
                new("Przerwij rytuał", [new FightEffect([EnemyCatalog.CryingMonk, EnemyCatalog.Templar]), new ReputationEffect(Faction.Brotherhood, -25), new ReputationEffect(Faction.Town, 10)], "Wchodzisz z bronią. Płacz zmienia się w wycie."),
                new("Odejdź", [], "Niektóre rzeczy lepiej wiedzieć z opowieści."),
            ]),
        new(
            "oasis_pilgrim", RegionId.Oasis, "Pielgrzym",
            [
                "Pielgrzym z pękniętą laską prosi o eskortę do świątyni. \"Templariusze zabierają takich jak ja na szlaku.\"",
            ],
            [
                new("Odprowadź go", [new ReputationEffect(Faction.Town, 5), new ReputationEffect(Faction.Brotherhood, 10), new GoldEffect(20), new ExpEffect(150)], "Dochodzicie bez przygód. Pielgrzym płaci więcej, niż ma.",
                    new SkillCheck(StatKind.Strength, 12), [new FightEffect([EnemyCatalog.Templar]), new ReputationEffect(Faction.Brotherhood, 10)], "W połowie drogi z piasku wstaje templariusz. Pielgrzym chowa się za tobą."),
                new("Odmów", [], "Idzie sam. Nie oglądasz się."),
            ]),
    ];

    public static IReadOnlyList<GameEvent> All => Events;

    public static IEnumerable<GameEvent> InRegion(RegionId region) => Events.Where(e => e.Region == region);
}
