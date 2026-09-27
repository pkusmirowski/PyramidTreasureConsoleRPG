"""Bot grający w PyramidTreasureConsoleRPG przez potok (smoke test).

Użycie: python3 tools/playbot.py <ścieżka do PyramidTreasureConsoleRPG.dll> [klasa 1-3] [limit sekund]
Gra do napisu KONIEC GRY, podejmując proste decyzje na podstawie ostatniego menu.
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
state = dict(started=False, talked=False, hp=0, maxhp=0, gold=0, level=0, potions=0, deaths=0, saved=False, loaded=False, special=False, fights=0, menus=0)

def send(s):
    log.write(f">>> {s}\n"); log.flush()
    p.stdin.write((s + "\n").encode()); p.stdin.flush()

def read_chunk(timeout=0.06):
    r, _, _ = select.select([p.stdout], [], [], timeout)
    if r:
        data = os.read(p.stdout.fileno(), 65536)
        if not data:
            return None
        return data.decode(errors="replace")
    return ""

def decide(text):
    m = re.search(r"poziom (\d+), (\d+)/(\d+) HP, (\d+) złota", text)
    if m:
        state["level"], state["hp"], state["maxhp"], state["gold"] = map(int, m.groups())
    for mm in re.finditer(r"(?:Masz |Tester: |zdrowie \()(\d+)/(\d+)", text):
        state["hp"], state["maxhp"] = int(mm.group(1)), int(mm.group(2))
    m = re.search(r"Mikstury: (\d+)", text)
    if m: state["potions"] = int(m.group(1))
    m = re.search(r"Wypij miksturę \(masz: (\d+)\)", text)
    if m: state["potions"] = int(m.group(1))
    if "Zostałeś pokonany" in text: state["deaths"] += 1
    if "Gra została zapisana" in text: state["saved"] = True
    if "Wczytano:" in text: state["loaded"] = True

    last = text
    def menu(title): return title in last
    if menu("Czy masz ukończone 18 lat?"): return "1"
    if menu("Menu główne"):
        if "Ukończyłeś grę" in text or "Dziękujemy za grę" in text: return "4"
        if state["deaths"] > 0 and not state["loaded"] and state["saved"]: return "2"
        if state["deaths"] > 0 and (state["loaded"] or not state["saved"]): return "4"
        return "1"
    if "Podaj swoje imię" in last: return "Tester"
    if menu("Wybierz klasę"): return CLASS
    if menu("Wyruszyć?"):
        return "1" if ("Karawana rusza" in last or state["hp"] * 2 >= state["maxhp"]) else "2"
    if menu("Niezapisany postęp"): return "2"
    if menu("Nadpisać istniejący zapis?"): return "2"
    if menu("Twoja tura:"):
        if state["hp"] * 100 < state["maxhp"] * 35 and state["potions"] > 0: return "4"
        return "2" if state["level"] >= 8 else "1"
    if menu("Którą miksturę wypić?"):
        opts = re.findall(r"(\d)\. .*\(masz: (\d+)\)", last)
        for num, cnt in opts:
            if int(cnt) > 0: return num
        return str(len(opts) + 1)
    if menu("Co chcesz kupić?"):
        if state["gold"] >= 100: state["gold"] -= 100; state["potions"] += 1; return "3"
        return "4"
    if menu("Co chcesz zrobić?") and "Udaj się w drogę" in last or menu("Wyrusz z karawaną"):
        if not state["saved"] and state["level"] >= 6: return "6"
        need_barman = "barman ma wieści" in last
        need_special = state["level"] >= 15 and not state["special"] and state["gold"] >= 150
        need_rest = state["hp"] * 2 < state["maxhp"]
        if need_barman or need_rest or need_special:
            state["talked"] = False; return "2"
        if state["gold"] >= 120 and state["potions"] < 3: return "3"
        state["fights"] += 1
        return "1"
    if menu("Witaj w tawernie"):
        need_barman = "barman ma wieści" in last
        need_special = state["level"] >= 15 and not state["special"] and state["gold"] >= 150
        if need_barman or need_special: return "1"
        if state["hp"] * 2 < state["maxhp"] and state["gold"] >= 10: return "3"
        return "4"
    if menu("Przy barze:"):
        if not state["talked"]: state["talked"] = True; return "1"
        if state["level"] >= 15 and not state["special"] and state["gold"] >= 150: return "2"
        return "4"
    if menu("Co podać?"):
        if not state["special"]: state["special"] = True; state["gold"] -= 150; return "3"
        return "4"
    if menu("Którą statystykę wzmocnić?"): return "1"
    if menu("Na górze:"):
        if state["hp"] < state["maxhp"] and state["gold"] >= 10: state["hp"] = state["maxhp"]; return "1"
        return "4"
    if menu("Grasz dalej?"): return "2"
    if "Ile stawiasz?" in last: return "0"
    return None

start = time.time()
idle = 0
while time.time() - start < MAX_SECONDS:
    chunk = read_chunk()
    if chunk is None: break
    if chunk:
        log.write(chunk); log.flush(); buf += chunk; idle = 0; continue
    idle += 1
    if idle < 2: continue  # wait for the process to finish printing
    if p.poll() is not None: break
    if buf.strip():
        ans = decide(buf)
        state["menus"] += 1
        if ans is None:
            print("NO DECISION for:\n" + buf[-1500:]); send("1")
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
print(f"class={CLASS} exit={rc} level={state['level']} deaths={state['deaths']} fights={state['fights']} saved={state['saved']} special={state['special']} menus={state['menus']} finished={finished} crashed={crashed}")
sys.exit(0 if finished and not crashed else 1)
