"""Bot grający w PyramidTreasureConsoleRPG przez potok (smoke test).

Użycie: python3 tools/playbot.py <ścieżka do PyramidTreasureConsoleRPG.dll> [klasa 1-3] [limit sekund]
Gra do napisu KONIEC GRY, podejmując proste decyzje na podstawie ostatniego menu:
robi zadania barmana, podróżuje po regionach stosownie do poziomu, pije mikstury, leczy się w tawernie.
Kod wyjścia 0 = zakończenie osiągnięte bez wyjątku; 1 = brak zakończenia lub wyjątek w grze.
"""
import os, re, subprocess, sys, time, select

DLL = sys.argv[1]
CLASS = sys.argv[2] if len(sys.argv) > 2 else "1"
MAX_SECONDS = int(sys.argv[3]) if len(sys.argv) > 3 else 900
TOUR = bool(os.environ.get("PLAYBOT_TOUR"))  # wycieczka po usługach pobocznych (kasyno, sprzedaż, zapis/wczytanie)
home = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".playbot-home")
os.makedirs(home, exist_ok=True)
env = dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT="1", HOME=home, APPDATA=home)
GAME_ARGS = os.environ.get("PLAYBOT_GAME_ARGS", "").split()  # np. "--seed 7" dla powtarzalnego przebiegu
p = subprocess.Popen(["dotnet", DLL] + GAME_ARGS, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, env=env)
log = open(os.path.join(home, "playbot_%s.log" % CLASS), "w", encoding="utf-8")
buf = ""

REGIONS = ["Port Sokoła", "Stare Miasto", "Delta i las", "Szlak Karawan", "Oaza Siwa", "Piramida Chufu"]
GIVER_REGION = {"Barman": "Port Sokoła", "Kapitan portu": "Port Sokoła", "Przemytnik Hasan": "Stare Miasto", "Kapłanka Neferet": "Oaza Siwa"}
GIVER_PLACE = {"Barman": "Tawerna", "Kapitan portu": "Kapitanat portu", "Przemytnik Hasan": "Melina Hasana", "Kapłanka Neferet": "Świątynia Bractwa"}

CLASS_NAME = {"1": "Wojownik", "2": "Łucznik", "3": "Asasyn"}[CLASS]
state = dict(level=1, hp=1, maxhp=1, gold=0, potions=0, region="Port Sokoła", deaths=0, saved=False, fights=0, menus=0,
             pending=set(), giver=None, locked=set(), want_bar=False, went_temple=False, last_target=None,
             bag_tried=False, owned=set(), equipped=set(), bought_tier=0, shop_mode=None, bag=0, talents=0, npc_visits={})

def send(s):
    log.write(f">>> {s}\n"); log.flush()
    p.stdin.write((s + "\n").encode()); p.stdin.flush()

def read_chunk(timeout=0.06):
    r, _, _ = select.select([p.stdout], [], [], timeout)
    if r:
        data = os.read(p.stdout.fileno(), 65536)
        return data.decode(errors="replace") if data else None
    return ""

def options_of(text, title):
    """Zwraca listę (numer, tekst) opcji menu o danym tytule (ostatnie wystąpienie)."""
    idx = text.rfind(title)
    if idx < 0:
        return []
    return [(int(n), t.strip()) for n, t in re.findall(r"^(\d+)\. (.*)$", text[idx:], re.M)]

def pick(opts, *needles, avoid=()):
    for n, t in opts:
        if any(t.startswith(x) or x in t for x in needles) and not any(a in t for a in avoid):
            return str(n)
    return None

def parse(text):
    for m in re.finditer(r"dzień \d+ – .*?, .*? (\d+) lvl, (\d+)/(\d+) HP, (\d+) złota", text):
        state["level"], state["hp"], state["maxhp"], state["gold"] = int(m.group(1)), int(m.group(2)), int(m.group(3)), int(m.group(4))
    for m in re.finditer(r"=== (" + "|".join(map(re.escape, REGIONS)) + r"), dzień", text):
        state["region"] = m.group(1)
    for m in re.finditer(r"(?:Masz |Tester: |zdrowie \()(\d+)/(\d+)", text):
        state["hp"], state["maxhp"] = int(m.group(1)), int(m.group(2))
    for m in re.finditer(r"Mikstury(?: \| Wypij miksturę)?[^\d]*(\d+)", text):
        state["potions"] = int(m.group(1))
    for m in re.finditer(r"Wypij miksturę \(masz: (\d+)\)", text):
        state["potions"] = int(m.group(1))
    for m in re.finditer(r"Torba(?: \((\d+)/\d+\))?", text):
        if m.group(1): state["bag"] = int(m.group(1))
    for m in re.finditer(r"Kupiłeś: (.*?) za", text):
        state["owned"].add(m.group(1))
    for m in re.finditer(r"Łup: (.*?)(?: \(| –|,|\.)", text):
        state["owned"].add(m.group(1))
    if "Wybrano talent" in text: state["talents"] += 1
    for m in re.finditer(r"Wróć do zleceniodawcy \((.*?)\)", text):
        state["pending"].add(m.group(1))
    for m in re.finditer(r"GOTOWE – oddaj u: (.*?)$", text, re.M):
        state["pending"].add(m.group(1).strip())
    if "Zadanie ukończone:" in text or "Przyjęto zadanie" in text: state["since_log"] = 0
    if "Zadanie ukończone:" in text and state["giver"]:
        state["pending"].discard(state["giver"])
    if "Nie posiadasz żadnych mikstur" in text or "Nie masz żadnych mikstur" in text: state["potions"] = 0
    if "Zostałeś pokonany" in text: state["deaths"] += 1
    if "– do torby." in text or "Kupiłeś:" in text: state["loot_pending"] = True
    if "Gra została zapisana" in text: state["saved"] = True
    if "Nie możesz tam teraz jechać" in text and state["last_target"] and "brak złota" not in text:
        state["locked"].add(state["last_target"])
    if "Nowy etap wyprawy" in text:
        state["locked"].clear()
    for g in GIVER_REGION:
        if re.search(r"^=== " + re.escape(g) + r" ===$|^" + re.escape(g) + r"$", text, re.M):
            state["giver"] = g

def target_region():
    if state.get("heal_trip"):
        return "Port Sokoła"
    for giver in list(state["pending"]):
        return GIVER_REGION[giver]
    lvl = state["level"]
    if lvl >= 20: wish = ["Piramida Chufu", "Oaza Siwa"]
    elif lvl >= 13: wish = ["Oaza Siwa", "Szlak Karawan"]
    elif lvl >= 9: wish = ["Szlak Karawan", "Delta i las"]
    elif lvl >= 4: wish = ["Delta i las", "Stare Miasto"]
    else: wish = ["Port Sokoła"]
    for w in wish:
        if w not in state["locked"]:
            return w
    return state["region"]

def decide(text):
    parse(text)
    last = text
    if "Czy masz ukończone 18 lat?" in last: return "1"
    if "Co robisz z Graalem?" in last: state["finished"] = True
    if "Menu główne" in last:
        if TOUR and state["saved"] and not state.get("tour_reloaded") and not state.get("finished"):
            state["tour_reloaded"] = True
            return "2"
        if state.get("finished") or state["deaths"] > 0 or state.get("started"): return "4"
        return "1"
    if "Podaj swoje imię" in last:
        state["started"] = True
        return "Tester"
    if "Wybierz klasę" in last: return CLASS
    if "Poziom trudności:" in last: return os.environ.get("PLAYBOT_DIFFICULTY", "2")
    if "Wyruszyć?" in last or "Wejść?" in last:
        desperate = state["gold"] < 25 and state["potions"] == 0 and state["hp"] * 4 >= state["maxhp"]
        return "1" if ("Wejść?" in last or state["hp"] * 10 >= state["maxhp"] * 7 or state["potions"] > 0 or desperate) else "2"
    if "Niezapisany postęp" in last: return "2"
    if "Nadpisać istniejący zapis?" in last: return "2"
    if "Zapisz w slocie:" in last: return "2" if TOUR else "1"
    if "Który zapis wczytać?" in last:
        opts = options_of(last, "Który zapis wczytać?")
        return "1" if TOUR and state.get("tour_reloaded") else str(len(opts))
    if "Co robisz z Graalem?" in last:
        opts = options_of(last, "Co robisz z Graalem?")
        for n, t in reversed(opts):
            if "niedostępne" not in t: return str(n)
        return "1"
    if "Nowa gra+?" in last:
        if os.environ.get("PLAYBOT_NGPLUS") and not state.get("ngplus_done"):
            state["ngplus_done"] = True; state["finished"] = False; state["saved"] = False; state["locked"].clear(); state["pending"].clear()
            state["owned"].clear(); state["equipped"].clear(); state["went_temple"] = False
            return "1"
        return "2"
    if "Przyjąć zadanie?" in last: return "1"
    if "Twoja tura:" in last:
        opts = options_of(last, "Twoja tura:")
        if state["hp"] * 100 < state["maxhp"] * 35 and state["potions"] > 0:
            return pick(opts, "Wypij")
        if "Broń się!" in last or "zbiera moc" in last:
            return pick(opts, "Obrona")
        alive = last.count(" HP\n") - 1
        if state["hp"] * 100 < state["maxhp"] * 20 and state["potions"] == 0 and "Uciekaj" in last:
            return pick(opts, "Uciekaj")
        return "2" if state["level"] >= 8 else "1"
    if "Cel:" in last and "Twoja tura:" not in last.split("Cel:")[-1]:
        opts = options_of(last, "Cel:")
        best = None
        for n, t in opts[:-1]:
            m = re.search(r"– (\d+)/(\d+) HP", t)
            if m and (best is None or int(m.group(1)) < best[0]): best = (int(m.group(1)), n)
        return str(best[1]) if best else "1"
    if "Którą miksturę wypić?" in last:
        for n, t in reversed(options_of(last, "Którą miksturę wypić?")):
            m = re.search(r"masz: (\d+)", t)
            if m and int(m.group(1)) > 0 and "lecz" in t: return str(n)
        return str(len(options_of(last, "Którą miksturę wypić?")))
    if "Co robisz?" in last and "Co chcesz zrobić?" not in last:
        opts = options_of(last, "Co robisz?")
        if pick(opts, "Pasuj"): return pick(opts, "Pasuj")
        risky = state["hp"] * 10 < state["maxhp"] * 7 or state["potions"] == 0
        for n, t in opts:
            if "niedostępne" not in t and "[test" not in t and not (risky and "walka:" in t): return str(n)
        for n, t in opts:
            if "niedostępne" not in t and "[test" not in t: return str(n)
        for n, t in opts:
            if "niedostępne" not in t: return str(n)
        return "1"
    if "Wyposażenie:" in last and "Kup" not in last.split("Wyposażenie:")[-1][:5]:
        opts = options_of(last, "Wyposażenie:")
        for n, t in opts:
            if t.startswith("Załóż:") and "nie dla twojej klasy" not in t:
                name = t[len("Załóż: "):].split(" [")[0]
                if name not in state["equipped"]:
                    state["equipped"].add(name)
                    return str(n)
        return str(len(opts))
    if "Który talent?" in last: return "1"
    if "Co mówisz?" in last:
        opts = options_of(last, "Co mówisz?")
        for n, t in opts:
            if "niedostępne" not in t: return str(n)
        return str(len(opts))
    if "Jeden z nich jeszcze dyszy" in last or "Co robisz z jeńcem?" in last:
        return pick(options_of(last, "Co robisz z jeńcem?"), "Puść") or "2"
    if "Rozmowa:" in last:
        opts = options_of(last, "Rozmowa:")
        return pick(opts, "Oddaj:") or pick(opts, "Przyjmij:") or str(len(opts))
    if "Dokąd?" in last:
        opts = options_of(last, "Dokąd?")
        target = target_region()
        state["last_target"] = target
        choice = pick(opts, target, avoid=("tu jesteś",))
        label = [t for n, t in opts if str(n) == choice][0] if choice else ""
        if choice and " – " not in label.split(")", 1)[-1]:
            return choice
        if "brak złota" not in label:
            state["locked"].add(target)
        else:
            state["earn"] = True  # zarób na drogę walcząc na miejscu
        return str(len(opts))
    if "Mikstury:" in last:
        opts = options_of(last, "Mikstury:")
        if state["potions"] < 4:
            for name, cost in (("Duża", 130), ("Średnia", 70), ("Mała", 25)):
                if state["gold"] >= cost:
                    state["gold"] -= cost - 5; state["potions"] += 1
                    return pick(opts, name)
        return str(len(opts))
    if "Wyposażenie:" in last and "Torba:" in last:
        opts = options_of(last, "Wyposażenie:")
        best = None
        for n, t in opts:
            m = re.search(r"^(.*?) \[(broń|pancerz|amulet)\].*– (\d+) g", t)
            if not m or "nie dla twojej klasy" in t: continue
            name, slot, price = m.group(1), m.group(2), int(m.group(3))
            if name in state["owned"] or price > state["gold"] - 120: continue
            if slot == "broń" and CLASS_NAME not in t: continue
            best = (price, n, name) if best is None or price > best[0] else best
        if best:
            state["gold"] -= best[0]; state["owned"].add(best[2])
            return str(best[1])
        return str(len(opts))
    if "Co sprzedajesz?" in last:
        opts = options_of(last, "Co sprzedajesz?")
        if TOUR and len(opts) > 1 and not state.get("tour_sold"):
            state["tour_sold"] = True
            return "1"
        return str(len(opts))
    if "=== Sklep" in last and "Co chcesz zrobić?" in last:
        opts = options_of(last, "Co chcesz zrobić?")
        if state["shop_mode"] is None:
            state["shop_mode"] = "gear"
            return pick(opts, "Kup wyposażenie")
        if state["shop_mode"] == "sell":
            state["shop_mode"] = "gear"
            return pick(opts, "Sprzedaj")
        if state["shop_mode"] == "gear":
            state["shop_mode"] = "potions"
            return pick(opts, "Kup mikstury")
        state["shop_mode"] = None
        return pick(opts, "Wyjdź")
    if "Witaj w tawernie" in last:
        opts = options_of(last, "Witaj w tawernie")
        if state.get("want_loan") or state.get("want_repay") or state.get("tour_casino"):
            if state.pop("tour_casino", False):
                state["tour_games"] = ["Ruletka", "Jednoręki", "Blackjack", "Kości"]
                state["tour_bet"] = True
            return pick(opts, "kasyna")
        if state["want_bar"]:
            state["want_bar"] = False
            return pick(opts, "Podejdź do baru")
        if state["hp"] < state["maxhp"] and state["gold"] >= 10: return pick(opts, "Zapytaj o pokój")
        if state.pop("npc_trip", False): return pick(opts, "Zapytaj o pokój")
        return str(len(opts))
    if "Przy barze:" in last:
        opts = options_of(last, "Przy barze:")
        if "ma wieści" in last: return pick(opts, "Zapytaj barmana")
        return str(len(opts))
    if "Na górze:" in last:
        opts = options_of(last, "Na górze:")
        if state["hp"] < state["maxhp"] and state["gold"] >= 10:
            state["hp"] = state["maxhp"]
            return pick(opts, "Wynajmij")
        if state["gold"] >= 80:
            for n, t in opts:
                if t.startswith("Odwiedź:"):
                    name = t.split(":")[1].split("–")[0].strip()
                    if state["npc_visits"].get(name, 0) < 4:
                        state["npc_visits"][name] = state["npc_visits"].get(name, 0) + 1
                        state["gold"] -= 40
                        return str(n)
        return str(len(opts))
    if "Co podać?" in last: return str(len(options_of(last, "Co podać?")))
    if "W co grasz?" in last:
        opts = options_of(last, "W co grasz?")
        if state.pop("want_loan", False) or state.pop("want_repay", False):
            return pick(opts, "Lichwiarz")
        if TOUR and state.get("tour_games"):
            return pick(opts, state["tour_games"].pop(0))
        return str(len(opts))
    if "Na co stawiasz?" in last: return "1"
    if "Numer (0-36)" in last: return "7"
    if "Ile pożyczasz" in last:
        state["debt"] = 150; state["gold"] += 150
        return "150"
    if "Ile spłacasz" in last:
        m = re.search(r"Ile spłacasz \(0-(\d+)\)", last)
        amount = int(m.group(1)) if m else 0
        state["gold"] -= amount; state["debt"] = 0
        return str(amount)
    if "Grasz dalej?" in last:
        state["tour_bet"] = bool(state.get("tour_games"))
        return "2"
    if "Ile stawiasz?" in last: return "1" if TOUR and state.get("tour_bet") else "0"
    if "Co chcesz zrobić?" in last:
        opts = options_of(last, "Co chcesz zrobić?")
        if "=== Sakwa ===" in last and len(opts) == 3:
            if state["hp"] * 2 < state["maxhp"] and "Nie posiadasz" not in last and not state["bag_tried"]:
                state["bag_tried"] = True
                return "1"
            if "Torba (" in last and "pusta" not in last and not state.get("gear_tried"):
                state["gear_tried"] = True
                return "2"
            state["gear_tried"] = False
            return "3"
        region = state["region"]
        t = pick(opts, "Wybierz talent")
        if t: return t
        state["since_log"] = state.get("since_log", 0) + 1
        if state["since_log"] >= 15 and "Dziennik zadań" in last:
            state["since_log"] = 0
            return pick(opts, "Dziennik zadań")
        if state.get("loot_pending"):
            state["loot_pending"] = False
            return pick(opts, "Sakwa")
        # 1. zlecenia i wieści
        if "(barman ma wieści!)" in last:
            state["want_bar"] = True
            return pick(opts, "Tawerna")
        z = pick(opts, "(zlecenie!)")
        if z: return z
        for giver in list(state["pending"]):
            if GIVER_REGION[giver] == region:
                if giver == "Barman":
                    state["want_bar"] = True
                    return pick(opts, "Tawerna")
                return pick(opts, GIVER_PLACE[giver])
        # 2. leczenie (bieda: lichwiarz w kasynie)
        if state["hp"] * 2 < state["maxhp"] and state["gold"] < 10 and state["potions"] == 0 and "Tawerna" in last and not state.get("debt"):
            state["want_loan"] = True
            return pick(opts, "Tawerna")
        if state.get("debt") and state["gold"] >= state["debt"] + 200 and "Tawerna" in last:
            state["want_repay"] = True
            return pick(opts, "Tawerna")
        if state["hp"] * 2 < state["maxhp"]:
            if "Tawerna" in last and state["gold"] >= 10: return pick(opts, "Tawerna")
            if state["gold"] < 25 and state["potions"] == 0 and state["hp"] * 4 >= state["maxhp"]:
                state["fights"] += 1
                return "1"  # bieda: walcz ze słabymi wrogami, żeby zarobić na nocleg
            if state["potions"] > 0 and not state["bag_tried"]: return pick(opts, "Sakwa")
            if "Sklep" in last and state["gold"] >= 25 and not state.get("heal_shop"):
                state["heal_shop"] = True; state["shop_mode"] = "gear"
                return pick(opts, "Sklep")
            if region != "Port Sokoła" and state["gold"] >= 10:
                state["heal_trip"] = True
                return pick(opts, "Mapa")
        else:
            state["bag_tried"] = False; state["heal_shop"] = False; state["heal_trip"] = False
        # 2a. wycieczka po usługach (PLAYBOT_TOUR=1): kasyno raz, sprzedaż raz, powrót do menu i wczytanie po zapisie
        if TOUR and "Tawerna" in last and state["gold"] >= 60 and not state.get("tour_casino_done"):
            state["tour_casino_done"] = True; state["tour_casino"] = True
            return pick(opts, "Tawerna")
        if TOUR and "Sklep" in last and state["owned"] and not state.get("tour_sold") and not state.get("tour_shop_sell"):
            state["tour_shop_sell"] = True; state["shop_mode"] = "sell"
            return pick(opts, "Sklep")
        if TOUR and state["saved"] and not state.get("tour_reloaded") and not state.get("tour_left"):
            state["tour_left"] = True
            return pick(opts, "Wróć do menu głównego")
        # 2b. odwiedziny na górze (wątki NPC) – raz na jakiś czas, gdy jest złoto
        if "Tawerna" in last and state["gold"] >= 150 and state["menus"] - state.get("last_npc_trip", -999) > 60:
            state["last_npc_trip"] = state["menus"]; state["npc_trip"] = True
            return pick(opts, "Tawerna")
        # 3. zakupy (przed drogą zawsze choć jedna mikstura)
        if "Sklep" in last and state["potions"] == 0 and state["gold"] >= 45 and not state.get("potion_stop"):
            state["potion_stop"] = True; state["shop_mode"] = "gear"
            return pick(opts, "Sklep")
        if state["potions"] > 0: state["potion_stop"] = False
        if "Sklep" in last and (state["gold"] >= 130 and state["potions"] < 4 or state["gold"] >= 200 and not state.get("shopped_at") == (region, state["gold"] // 200)):
            state["shopped_at"] = (region, state["gold"] // 200)
            return pick(opts, "Sklep")
        # 4. zapis
        if not state["saved"] and state["level"] >= 6: return pick(opts, "Zapisz grę")
        # 5. świątynia w oazie (zadanie główne)
        if region == "Oaza Siwa" and not state["went_temple"] and "Świątynia" in last:
            state["went_temple"] = True
            return pick(opts, "Świątynia")
        # 6. podróż albo eksploracja
        if state.pop("earn", False):
            state["fights"] += 1
            return "1"
        target = target_region()
        if target != region:
            return pick(opts, "Mapa")
        state["fights"] += 1
        return "1"
    if "Sakwa" in last and "Co chcesz zrobić?" not in last:
        return "2"
    return None

start = time.time()
idle = 0
progress_level, progress_menus = 1, 0
STALL_MENUS = 1500  # tyle decyzji bez awansu = bot się zapętlił; kończymy z diagnostyką zamiast czekać na limit
while time.time() - start < MAX_SECONDS:
    if state["level"] > progress_level:
        progress_level, progress_menus = state["level"], state["menus"]
    if state["menus"] - progress_menus > STALL_MENUS and not state.get("finished"):
        print("NO PROGRESS: %d decyzji bez awansu (poziom %d). Ostatni ekran:\n%s" % (STALL_MENUS, state["level"], buf[-2000:] or "(pusty bufor)"))
        p.kill()
        break
    chunk = read_chunk()
    if chunk is None: break
    if chunk:
        log.write(chunk); log.flush(); buf += chunk; idle = 0; continue
    idle += 1
    if idle < 2: continue
    if p.poll() is not None: break
    if buf.strip():
        ans = decide(buf)
        state["menus"] += 1
        if ans is None:
            print("NO DECISION for:\n" + buf[-1200:]); send("1")
        else:
            send(ans)
        buf = ""
    idle = 0
try:
    rc = p.wait(timeout=5)
except subprocess.TimeoutExpired:
    p.kill(); rc = "killed(timeout)"
log.close()
text = open(log.name, encoding="utf-8").read()
finished = "KONIEC GRY" in text
crashed = "Unhandled exception" in text or "   at " in text
print(f"class={CLASS} exit={rc} level={state['level']} region={state['region']} deaths={state['deaths']} fights={state['fights']} saved={state['saved']} menus={state['menus']} finished={finished} crashed={crashed}")
sys.exit(0 if finished and not crashed else 1)
