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
home = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".playbot-home")
os.makedirs(home, exist_ok=True)
env = dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT="1", HOME=home, APPDATA=home)
p = subprocess.Popen(["dotnet", DLL], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, env=env)
log = open(os.path.join(home, "playbot_%s.log" % CLASS), "w", encoding="utf-8")
buf = ""

REGIONS = ["Port Sokoła", "Stare Miasto", "Delta i las", "Szlak Karawan", "Oaza Siwa", "Piramida Chufu"]
GIVER_REGION = {"Barman": "Port Sokoła", "Kapitan portu": "Port Sokoła", "Przemytnik Hasan": "Stare Miasto", "Kapłanka Neferet": "Oaza Siwa"}
GIVER_PLACE = {"Barman": "Tawerna", "Kapitan portu": "Kapitanat portu", "Przemytnik Hasan": "Melina Hasana", "Kapłanka Neferet": "Świątynia Bractwa"}

state = dict(level=1, hp=1, maxhp=1, gold=0, potions=0, region="Port Sokoła", deaths=0, saved=False, fights=0, menus=0,
             pending=set(), giver=None, locked=set(), want_bar=False, went_temple=False, last_target=None, explores=0)

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
    for m in re.finditer(r"^(" + "|".join(map(re.escape, REGIONS)) + r"), dzień", text, re.M):
        state["region"] = m.group(1)
    for m in re.finditer(r"(?:Masz |Tester: |zdrowie \()(\d+)/(\d+)", text):
        state["hp"], state["maxhp"] = int(m.group(1)), int(m.group(2))
    for m in re.finditer(r"Mikstury(?: \| Wypij miksturę)?[^\d]*(\d+)", text):
        state["potions"] = int(m.group(1))
    for m in re.finditer(r"Wypij miksturę \(masz: (\d+)\)", text):
        state["potions"] = int(m.group(1))
    for m in re.finditer(r"Wróć do zleceniodawcy \((.*?)\)", text):
        state["pending"].add(m.group(1))
    if "Zadanie ukończone:" in text and state["giver"]:
        state["pending"].discard(state["giver"])
    if "Zostałeś pokonany" in text: state["deaths"] += 1
    if "Gra została zapisana" in text: state["saved"] = True
    if "Nie możesz tam teraz jechać" in text and state["last_target"]:
        state["locked"].add(state["last_target"])
    if "Nowy etap wyprawy" in text:
        state["locked"].clear()
    for g in GIVER_REGION:
        if re.search(r"^=== " + re.escape(g) + r" ===$|^" + re.escape(g) + r"$", text, re.M):
            state["giver"] = g

def target_region():
    for giver in list(state["pending"]):
        return GIVER_REGION[giver]
    lvl = state["level"]
    if lvl >= 20: wish = ["Piramida Chufu", "Oaza Siwa"]
    elif lvl >= 13: wish = ["Oaza Siwa", "Szlak Karawan"]
    elif lvl >= 8: wish = ["Szlak Karawan", "Delta i las"]
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
    if "Menu główne" in last:
        if "Dziękujemy za grę" in text or state["deaths"] > 0: return "4"
        return "1"
    if "Podaj swoje imię" in last: return "Tester"
    if "Wybierz klasę" in last: return CLASS
    if "Wyruszyć?" in last or "Wejść?" in last:
        return "1" if ("Wejść?" in last or state["hp"] * 2 >= state["maxhp"] or state["potions"] > 0) else "2"
    if "Niezapisany postęp" in last: return "2"
    if "Nadpisać istniejący zapis?" in last: return "2"
    if "Przyjąć zadanie?" in last: return "1"
    if "Twoja tura:" in last:
        opts = options_of(last, "Twoja tura:")
        if state["hp"] * 100 < state["maxhp"] * 35 and state["potions"] > 0:
            return pick(opts, "Wypij")
        return "2" if state["level"] >= 8 else "1"
    if "Którą miksturę wypić?" in last:
        for n, t in options_of(last, "Którą miksturę wypić?"):
            m = re.search(r"masz: (\d+)", t)
            if m and int(m.group(1)) > 0: return str(n)
        return str(len(options_of(last, "Którą miksturę wypić?")))
    if "Co robisz?" in last and "Co chcesz zrobić?" not in last:
        opts = options_of(last, "Co robisz?")
        for n, t in opts:
            if "niedostępne" not in t and "[test" not in t: return str(n)
        for n, t in opts:
            if "niedostępne" not in t: return str(n)
        return "1"
    if "Rozmowa:" in last:
        opts = options_of(last, "Rozmowa:")
        return pick(opts, "Oddaj:") or pick(opts, "Przyjmij:") or str(len(opts))
    if "Dokąd?" in last:
        opts = options_of(last, "Dokąd?")
        target = target_region()
        state["last_target"] = target
        choice = pick(opts, target, avoid=("tu jesteś",))
        if choice and "–" not in [t for n, t in opts if str(n) == choice][0].split(")")[-1]:
            return choice
        state["locked"].add(target)
        return str(len(opts))
    if "Co chcesz kupić?" in last:
        opts = options_of(last, "Co chcesz kupić?")
        if state["gold"] >= 130 and state["potions"] < 4:
            state["gold"] -= 100; state["potions"] += 1
            return pick(opts, "Duża")
        return str(len(opts))
    if "Witaj w tawernie" in last:
        opts = options_of(last, "Witaj w tawernie")
        if state["want_bar"]:
            state["want_bar"] = False
            return pick(opts, "Podejdź do baru")
        if state["hp"] < state["maxhp"] and state["gold"] >= 10: return pick(opts, "Zapytaj o pokój")
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
        return str(len(opts))
    if "Co podać?" in last: return str(len(options_of(last, "Co podać?")))
    if "Grasz dalej?" in last: return "2"
    if "Ile stawiasz?" in last: return "0"
    if "Co chcesz zrobić?" in last:
        opts = options_of(last, "Co chcesz zrobić?")
        if "Sakwa" in last and "Wypij miksturę" in last and len(opts) == 2:
            return "2"  # ekran sakwy – wyjdź
        region = state["region"]
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
        # 2. leczenie
        if state["hp"] * 2 < state["maxhp"]:
            if "Tawerna" in last and state["gold"] >= 10: return pick(opts, "Tawerna")
            if state["potions"] > 0: return pick(opts, "Sakwa")
        # 3. zakupy w porcie/oazie
        if "Sklep" in last and state["gold"] >= 130 and state["potions"] < 4: return pick(opts, "Sklep")
        # 4. zapis
        if not state["saved"] and state["level"] >= 6: return pick(opts, "Zapisz grę")
        # 5. świątynia w oazie (zadanie główne)
        if region == "Oaza Siwa" and not state["went_temple"] and "Świątynia" in last:
            state["went_temple"] = True
            return pick(opts, "Świątynia")
        # 6. podróż albo eksploracja
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
while time.time() - start < MAX_SECONDS:
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
