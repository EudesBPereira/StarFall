"""
Generates the final audio set (docs/ASSET_SPEC.md, 47 files) with the ElevenLabs API and drops it straight into
Assets/_Project/Audio/Final/{SFX,Music,Ambient} using the exact names the bootstrap expects.

Usage:
  python generate_audio.py [sfx|ambient|music|all] [--force] [--only NAME[,NAME]]

The API key is read from the ELEVENLABS_API_KEY environment variable or ~/.starfall/elevenlabs.key (never from
the repository). Existing files are skipped unless --force is given, so the script can be re-run to fill gaps.

Endpoints (https://elevenlabs.io/docs/eleven-api):
  POST /v1/sound-generation   text, duration_seconds (0.5-30), prompt_influence, loop  -> audio/mpeg
  POST /v1/music              prompt, music_length_ms, force_instrumental                -> audio/mpeg
"""
import json, os, sys, time, urllib.request, urllib.error
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
DEST = REPO / "Assets" / "_Project" / "Audio" / "Final"
BASE = "https://api.elevenlabs.io/v1"

STYLE = ("Sci-fi neon arcade space shooter for mobile. Clean, punchy, synthetic, no voice, no music in sound effects, "
         "no reverb tail beyond the stated length. ")

# ---- Sound effects: name -> (duration seconds, prompt) ----
SFX = {
    "Laser": (0.5, "small bright sci-fi laser blip fired by a fighter, quick high-pitched pew with fast pitch drop, thin, no bass, light and non-fatiguing"),
    "Spread": (0.5, "short triple-click energy burst, three tiny plasma pellets leaving a barrel almost at once, crisp mid-high, no bass"),
    "Plasma": (0.6, "wet rounded plasma bolt launch, low-mid thoomp with soft bubbly texture and short downward pitch glide, organic and warm"),
    "Railgun": (0.8, "railgun shot: instant electric crack then a fast rising-falling metallic whine, a magnetic slug tearing the air, sharp transient"),
    "Missile": (0.8, "small missile launch: compressed exhaust hiss with a quick whoosh moving away, faint mechanical latch click at the start, no explosion"),
    "EnergyCannon": (1.0, "heavy charged energy cannon release: deep resonant boom-whoom with bright electric crackle on top, strong impact, quick tail"),
    "Charge": (1.0, "energy charge-up: rising tone with growing electric buzz and shimmer, quiet start, bright tense end, cut clean, no release"),
    "EnemyShot": (0.5, "short dull enemy plasma shot, low thup with slight downward pitch, dark and soft, stays in the background, no bass boom"),
    "Impact": (0.5, "tiny metallic impact, a bullet pinging off an armored hull, sharp click with a very short metallic ring, very light"),
    "ShieldHit": (0.5, "energy shield absorbing a hit: glassy electric tink with a soft resonant hum that fades fast, bright, synthetic"),
    "ExplosionSmall": (0.6, "small arcade explosion of a light spacecraft: crisp noise burst with a short low thump and crackling debris, compact"),
    "ExplosionLarge": (1.6, "big cinematic sci-fi explosion: deep sub-bass impact, wide roaring mid burst, metallic shrapnel, rumbling tail that fully decays"),
    "PowerUp": (0.6, "power-up pickup: four quick ascending bright synth notes with a sparkle, cheerful arcade chime"),
    "PlayerHit": (0.5, "player hull taking damage: hard metallic crunch with a low thud and a brief distorted alarm-like buzz, unpleasant on purpose"),
    "Ultimate": (1.5, "massive screen-wide energy release: quick charge-up sweep then a huge shockwave blast with shimmering harmonics and a deep sub hit, decaying to silence"),
    "LaserCharge": (1.2, "boss weapon charging before a giant laser: menacing rising hum building electric tension with a growing high whine, cut clean, no release"),
    "Alarm": (1.0, "spaceship cockpit critical alarm: two fast beeps of a harsh electronic klaxon, mid-high, slightly distorted, urgent, dry"),
    "BossWarning": (1.4, "dramatic warning siren for a boss arrival: descending two-tone alarm blast with a low ominous swell and a metallic hit at the start, cinematic, no words"),
    "WebShot": (0.6, "mechanical spider firing an energy web: wet elastic thwip with brief crackling static, mid frequency"),
    "Summon": (0.7, "portal opening to release minions: reversed-sounding whoosh resolving into a hollow resonant bwom with a dark alien shimmer"),
    "Graze": (0.5, "extremely short delicate high glass ping with a tiny upward sweep, very quiet and airy, no bass, subtle near-miss cue"),
    "RiskUp": (0.5, "tension step-up cue: two-note rising electronic blip, minor third up, bright and short, clean"),
    "RiskDown": (0.5, "relief step-down cue: two-note falling electronic blip, minor third down, softer and duller, clean"),
    "OverdriveStart": (1.0, "engine kicking into overdrive: fast upward turbine spool with a bright electric surge and a punchy hit at the peak, exciting"),
    "OverdriveEnd": (0.8, "turbine spinning down with falling pitch and a soft power-off thud at the end, neutral"),
    "ComboUp": (0.5, "combo multiplier tick-up: bright short arpeggio of three quick ascending notes, arcade style, cheerful, no bass"),
    "RankReveal": (0.8, "rank letter stamped on a results screen: quick impact thunk followed by a bright triumphant chime ringing half a second"),
    "Achievement": (1.0, "achievement unlocked fanfare: four ascending bright synth notes in a major key with shimmering sparkle, ending on a held chord that fades"),
    "Purchase": (0.6, "purchase confirmation: short mechanical latch click-clack followed by a two-note positive chime, a module locking into a ship"),
    "UiSelect": (0.5, "minimal UI hover tick: a single soft high digital click, almost inaudible, no tone, no tail"),
    "UiConfirm": (0.5, "UI confirm: clean two-note rising digital chime, holographic and light"),
    "UiError": (0.5, "UI error buzz: short low double bzz-bzz digital denial tone, muted, clearly negative but not harsh"),
}

# ---- Ambient beds: seamless loops via sound-generation (name -> prompt) ----
AMBIENT = {
    "Space": "deep open space ambience: very low soft rumble, faint cosmic wind, occasional distant sparse electronic pings from far satellites, sparse and calm, no music",
    "Asteroids": "asteroid field ambience: muffled distant rock collisions, low rolling rumbles, gritty dust hiss, occasional deep thud, no music",
    "Nebula": "inside an ionized nebula: slowly shifting electric hum, soft crackling static, airy shimmering drones with no defined pitch, mysterious, no music",
    "Fortress": "inside a giant mechanical fortress: distant industrial machinery, rhythmic hydraulic thumps, steam vents, faint far-off alarms, metallic groans, no music",
    "Hive": "alien biomechanical hive ambience: slow organic heartbeat pulse, wet squelching textures, distant insect-like chittering, low breathing drone, unsettling, no music",
}
AMBIENT_SECONDS = 22.0

# ---- Music: name -> (seconds, prompt). All 120 BPM, D minor, instrumental, seamless loop. ----
MUSIC_BASE = "Instrumental video game music, 120 BPM, key of D minor, no vocals, no lyrics, seamless loop with no intro and no ending cadence. "
MUSIC = {
    "Menu": (90, "calm ambient sci-fi main menu: slow evolving synth pads, soft arpeggiated pulse, distant reverberated plucks, subtle neon-retro tone, hopeful but quiet, no drums"),
    "Stage1": (90, "light orchestral-hybrid space adventure: strings ostinato, brass accents, driving but not heavy electronic beat, heroic, moderate intensity"),
    "Stage2": (90, "spacey electronic action: pulsing analog bass sequence, glittering arpeggios, wide pads, tight electronic drums, futuristic and slightly mysterious"),
    "Stage3": (90, "synthwave chase: retro 80s synth lead, gated reverb snare, driving sawtooth bass, bright chorus chords, neon cyberpunk energy, fast-feeling"),
    "Stage4": (90, "industrial dark electronic: heavy distorted kick, metallic percussion hits, grinding bass, eerie detuned pads, glitchy textures, tense and relentless"),
    "Stage5": (100, "epic cinematic hybrid: full orchestra with choir-like synth pads, powerful percussion, soaring brass theme, electronic bass underneath, climax of a space war"),
    "Boss": (80, "intense boss battle: aggressive electronic-orchestral hybrid, pounding double-time percussion, urgent string stabs, distorted synth riff, relentless tension"),
    "FinalBoss": (100, "final boss climax: massive orchestral hits, choir-like synths, frantic electronic percussion, a menacing recurring motif that keeps escalating, the most epic track"),
    "Survival": (90, "endless-waves electronic: hypnotic driving beat, layered arpeggios that keep building, tense bass pulse, neon energy that never stops"),
    "OverdriveLayer": (64, "rhythm-only percussion layer meant to be mixed on top of other tracks: double-time hi-hats, driving sixteenth-note percussive synth pulse on a single D note, risers, white-noise sweeps, energetic transient hits, NO melody, NO chords"),
}


def api_key() -> str:
    key = os.environ.get("ELEVENLABS_API_KEY", "").strip()
    if not key:
        f = Path.home() / ".starfall" / "elevenlabs.key"
        if f.exists():
            key = f.read_text(encoding="utf-8").strip()
    if not key:
        sys.exit("No API key: set ELEVENLABS_API_KEY or create ~/.starfall/elevenlabs.key")
    return key


def post(path: str, payload: dict, key: str, query: str = "") -> bytes:
    """POST JSON, return audio bytes. Retries on 429/5xx with backoff; raises on other errors with the API message."""
    url = f"{BASE}{path}{query}"
    body = json.dumps(payload).encode("utf-8")
    for attempt in range(6):
        req = urllib.request.Request(url, data=body, method="POST", headers={
            "xi-api-key": key, "Content-Type": "application/json", "Accept": "audio/mpeg",
        })
        try:
            with urllib.request.urlopen(req, timeout=600) as r:
                data = r.read()
                if r.headers.get("Content-Type", "").startswith("application/json"):
                    raise RuntimeError(f"unexpected JSON response: {data[:300]!r}")
                return data
        except urllib.error.HTTPError as e:
            msg = e.read().decode("utf-8", "replace")[:400]
            if e.code in (429, 500, 502, 503, 504) and attempt < 5:
                wait = 10 * (attempt + 1)
                print(f"    HTTP {e.code}, retrying in {wait}s: {msg[:120]}")
                time.sleep(wait)
                continue
            raise RuntimeError(f"HTTP {e.code}: {msg}") from None
        except (urllib.error.URLError, TimeoutError) as e:
            if attempt < 5:
                print(f"    network error, retrying: {e}")
                time.sleep(10 * (attempt + 1))
                continue
            raise
    raise RuntimeError("gave up")


def save(dest: Path, data: bytes):
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_bytes(data)
    print(f"  saved {dest.relative_to(REPO)}  ({len(data) // 1024} KB)")


def gen_sfx(key: str, only, force: bool):
    for name, (seconds, prompt) in SFX.items():
        if only and name not in only:
            continue
        dest = DEST / "SFX" / f"{name}.mp3"
        if dest.exists() and not force:
            print(f"  skip {name} (exists)")
            continue
        print(f"SFX {name} ({seconds}s)")
        data = post("/sound-generation", {"text": STYLE + prompt, "duration_seconds": seconds, "prompt_influence": 0.55, "loop": False},
                    key, "?output_format=mp3_44100_128")
        save(dest, data)


def gen_ambient(key: str, only, force: bool):
    for name, prompt in AMBIENT.items():
        if only and name not in only:
            continue
        dest = DEST / "Ambient" / f"{name}.mp3"
        if dest.exists() and not force:
            print(f"  skip {name} (exists)")
            continue
        print(f"AMBIENT {name} ({AMBIENT_SECONDS}s loop)")
        data = post("/sound-generation", {"text": "Seamless looping ambient sound bed, very low volume. " + prompt, "duration_seconds": AMBIENT_SECONDS,
                                          "prompt_influence": 0.5, "loop": True}, key, "?output_format=mp3_44100_128")
        save(dest, data)


def gen_music(key: str, only, force: bool):
    for name, (seconds, prompt) in MUSIC.items():
        if only and name not in only:
            continue
        dest = DEST / "Music" / f"{name}.mp3"
        if dest.exists() and not force:
            print(f"  skip {name} (exists)")
            continue
        print(f"MUSIC {name} ({seconds}s)")
        try:
            data = post("/music", {"prompt": MUSIC_BASE + prompt, "music_length_ms": seconds * 1000, "force_instrumental": True},
                        key, "?output_format=mp3_44100_128")
        except RuntimeError as e:
            print(f"  FAILED {name}: {e}")
            continue
        save(dest, data)


def main():
    args = [a for a in sys.argv[1:]]
    force = "--force" in args
    only = None
    if "--only" in args:
        only = set(args[args.index("--only") + 1].split(","))
    what = next((a for a in args if a in ("sfx", "ambient", "music", "all")), "all")
    key = api_key()
    if what in ("sfx", "all"): gen_sfx(key, only, force)
    if what in ("ambient", "all"): gen_ambient(key, only, force)
    if what in ("music", "all"): gen_music(key, only, force)
    print("done")


if __name__ == "__main__":
    main()
