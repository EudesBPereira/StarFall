# Prompts de áudio para o Gemini — Starfall Defense

Como pedir cada um dos 47 arquivos de áudio de `docs/ASSET_SPEC.md` a um modelo de geração de áudio.
Os prompts estão em inglês porque os modelos de música e efeitos respondem melhor assim; o nome do arquivo e a finalidade estão em português.

## Como usar

1. Abra uma conversa nova com o Gemini (para música, use o modo de geração musical, Lyria; para efeitos, o modo de geração de som ou de "audio").
2. Cole o **prompt-base** abaixo uma única vez.
3. Para cada arquivo, cole o prompt individual. Peça um por vez: modelos de áudio misturam pedidos quando recebem vários.
4. Salve o resultado com o **nome exato** indicado e coloque na pasta indicada. O formato pode ser WAV, OGG ou MP3: o projeto converte na importação (efeitos viram mono, músicas ficam estéreo com streaming).
5. Rode `Starfall → Run Full Bootstrap` no Unity. O Console mostra `Final audio: N clip(s)`.

Se o modelo gerar um clipe mais longo que o pedido, corte no editor de áudio antes de salvar. Se gerar mais curto, peça novamente citando a duração.

## Prompt-base (cole uma vez por conversa)

```
You are producing the final sound design and music for "Starfall Defense", a fast 2D vertical space shoot'em up for mobile phones (Android/iOS). Visual style: modern pixel art, sci-fi neon with cyberpunk influence; electric blue, neon purple, energetic red and holographic white. Tone: arcade, intense, readable. The player pilots small fighters against "The Swarm", a mechanical alien civilization; three factions exist: Federation (military, clean, precise), Biomech (organic, wet, alive), Cyber Corp (digital, glitchy, hexagonal).

Rules for every file I ask for:
- Produce exactly ONE sound per request, nothing else in the file (no intro silence longer than 20 ms, no speech, no vocals, no watermark tones).
- Respect the requested duration; short sound effects must end cleanly (no reverb tail longer than the stated length).
- Sound effects: dry, punchy, mono-compatible, peak around -3 dBFS, no background music.
- Music and ambience: must be a SEAMLESS LOOP - the end must connect to the beginning with no click, no fade-out, no ending flourish; stay in one key and one tempo. Target loudness about -14 LUFS. All music tracks share 120 BPM so an extra "Overdrive layer" can play on top of any of them.
- Frequently repeated sounds (laser, enemy shot, graze, UI) must be light and non-fatiguing, with less low end.
- Warnings and rare events can be bigger and louder.
- Deliver as a downloadable audio file (WAV 44.1 kHz preferred; MP3 or OGG also fine).

I will name each request with the target file name. Reply only with the audio file for that request.
```

---

## Efeitos sonoros — salvar em `Assets/_Project/Audio/Final/SFX/`

Cada prompt gera um arquivo. Para ter variações, repita o prompt e salve como `Nome_1.wav`, `Nome_2.wav`, `Nome_3.wav` (o jogo sorteia). Vale a pena para `Laser`, `EnemyShot`, `Impact`, `ExplosionSmall` e `Graze`.

### Armas do jogador

**`Laser.wav`** — tiro básico, o som mais repetido do jogo
```
File: Laser.wav. Sound effect, 0.10 seconds. A small, bright sci-fi laser blip fired by the player's fighter: a quick high-pitched "pew" with a fast pitch drop, thin body, no bass, no echo. It will play up to seven times per second, so keep it light, clean and slightly soft on the attack.
```

**`Spread.wav`** — leque de tiros
```
File: Spread.wav. Sound effect, 0.10 seconds. A short triple-click energy burst, like three tiny plasma pellets leaving a barrel almost at once. Crisp, mid-high frequency, no bass, no tail.
```

**`Plasma.wav`** — plasma e esporos biomecânicos
```
File: Plasma.wav. Sound effect, 0.20 seconds. A wet, rounded plasma bolt launch: low-mid "thoomp" with a soft bubbly texture and a short downward pitch glide. Organic and warm rather than metallic. No long tail.
```

**`Railgun.wav`** — disparo perfurante
```
File: Railgun.wav. Sound effect, 0.40 seconds. A railgun shot: an instant electric crack followed by a fast rising-then-falling metallic whine, like a magnetic slug tearing the air. Sharp transient, medium body, tail ends by 0.4 s.
```

**`Missile.wav`** — lançamento de míssil
```
File: Missile.wav. Sound effect, 0.40 seconds. A small missile launch: a compressed hiss of exhaust with a quick whoosh that moves away from the listener, plus a faint mechanical latch click at the start. No explosion.
```

**`EnergyCannon.wav`** — canhão carregado disparando
```
File: EnergyCannon.wav. Sound effect, 0.50 seconds. A heavy charged energy cannon releasing: a deep resonant "boom-whoom" with a bright electric crackle on top, strong impact, tail resolving quickly. This is the most powerful player weapon sound.
```

**`Charge.wav`** — início da carga do canhão
```
File: Charge.wav. Sound effect, 0.60 seconds. An energy charge-up: a rising tone with growing electric buzz and shimmer, starting quiet and ending bright and tense, cut clean at 0.6 s (the release sound is a separate file).
```

### Inimigos e impactos

**`EnemyShot.wav`** — tiro inimigo, muito frequente
```
File: EnemyShot.wav. Sound effect, 0.10 seconds. A short, dull enemy plasma shot: a low "thup" with a slight downward pitch, darker and softer than the player's laser so it stays in the background. No bass boom, no tail.
```

**`Impact.wav`** — tiro acertando casco
```
File: Impact.wav. Sound effect, 0.10 seconds. A tiny metallic impact: a bullet pinging off an armored hull, sharp click with a quick metallic ring that dies immediately. Very light.
```

**`ShieldHit.wav`** — tiro absorvido por escudo
```
File: ShieldHit.wav. Sound effect, 0.15 seconds. An energy shield absorbing a hit: a glassy electric "tink" with a soft resonant hum that fades in 0.15 s. Bright, clean, slightly synthetic.
```

**`ExplosionSmall.wav`** — inimigo comum destruído
```
File: ExplosionSmall.wav. Sound effect, 0.30 seconds. A small arcade explosion of a light spacecraft: crisp burst of noise with a short low thump, some crackling debris, ending by 0.3 s. Satisfying but compact; it plays many times per minute.
```

**`ExplosionLarge.wav`** — chefe ou jogador destruído
```
File: ExplosionLarge.wav. Sound effect, 1.0 second. A big cinematic sci-fi explosion: deep sub-bass impact, wide roaring mid burst, metallic shrapnel and a rumbling tail that fully decays by 1.0 s. This is for boss deaths and the player's ship being destroyed.
```

**`PlayerHit.wav`** — jogador recebe dano no casco
```
File: PlayerHit.wav. Sound effect, 0.20 seconds. The player's hull taking damage: a hard metallic crunch with a low thud and a brief distorted alarm-like buzz, unpleasant on purpose so the player notices. Ends by 0.2 s.
```

### Especiais, avisos e chefes

**`Ultimate.wav`** — ativação da Ultimate
```
File: Ultimate.wav. Sound effect, 1.0 second. A massive energy release covering the whole screen: a quick charge-up sweep rising for 0.2 s, then a huge shockwave blast with bright shimmering harmonics and a deep sub hit, decaying to silence by 1.0 s. Epic and clean.
```

**`LaserCharge.wav`** — aviso do laser frontal do chefe
```
File: LaserCharge.wav. Sound effect, 1.0 second. A boss weapon charging before firing a giant laser: a menacing rising hum that builds electric tension with growing high-frequency whine, so the player learns to dodge. Cut clean at 1.0 s, no release.
```

**`Alarm.wav`** — casco crítico e tempestade solar
```
File: Alarm.wav. Sound effect, 0.80 seconds. A spaceship cockpit critical alarm: two fast beeps of a harsh electronic klaxon, mid-high frequency, slightly distorted, urgent. Dry, no reverb.
```

**`BossWarning.wav`** — "WARNING" na entrada do chefe
```
File: BossWarning.wav. Sound effect, 1.0 second. A dramatic "WARNING" siren for a boss arrival: a descending two-tone alarm blast with a low ominous swell underneath and a metallic hit at the start. Cinematic, wide, tail ends by 1.0 s. No spoken words.
```

**`WebShot.wav`** — Widow lança teia
```
File: WebShot.wav. Sound effect, 0.30 seconds. A mechanical spider firing an energy web: a wet elastic "thwip" with a brief crackling static texture, mid frequency, ending quickly.
```

**`Summon.wav`** — chefe invoca minions / abre fendas
```
File: Summon.wav. Sound effect, 0.40 seconds. A portal or hive opening to release minions: a reversed-sounding whoosh that resolves into a hollow, resonant "bwom" with a dark alien shimmer. Ends by 0.4 s.
```

### Zona de risco e pontuação

**`Graze.wav`** — projétil passando raspando, sutil e frequente
```
File: Graze.wav. Sound effect, 0.08 seconds. A near-miss "graze" cue: an extremely short, delicate high glass ping with a tiny upward sweep, very quiet and airy, no bass. It can trigger several times per second, so it must be subtle.
```

**`RiskUp.wav`** — zona de risco subiu
```
File: RiskUp.wav. Sound effect, 0.20 seconds. A tension step-up cue: a two-note rising electronic blip (minor third up), bright and short, signaling danger is increasing. Clean, no tail.
```

**`RiskDown.wav`** — zona de risco caiu
```
File: RiskDown.wav. Sound effect, 0.20 seconds. A relief step-down cue: a two-note falling electronic blip (minor third down), softer and duller than the rising version. Clean, no tail.
```

**`OverdriveStart.wav`** — Overdrive ativado
```
File: OverdriveStart.wav. Sound effect, 0.80 seconds. Overdrive engaging: an engine kicking into overdrive, a fast upward turbine spool with a bright electric surge and a punchy hit at the peak, then cut to a short tail. Exciting, high energy.
```

**`OverdriveEnd.wav`** — Overdrive terminou
```
File: OverdriveEnd.wav. Sound effect, 0.50 seconds. Overdrive powering down: a turbine spinning down with a falling pitch and a soft "power off" thud at the end. Neutral, not sad.
```

**`ComboUp.wav`** — multiplicador subiu
```
File: ComboUp.wav. Sound effect, 0.15 seconds. A combo multiplier tick-up: a bright, short arpeggiated blip of three quick ascending notes, arcade style, cheerful, no bass.
```

**`RankReveal.wav`** — letra do rank aparece
```
File: RankReveal.wav. Sound effect, 0.60 seconds. A rank letter being stamped on the results screen: a quick impact "thunk" followed by a bright triumphant chime that rings for half a second. Rewarding.
```

### Interface e progressão

**`Achievement.wav`** — conquista desbloqueada
```
File: Achievement.wav. Sound effect, 0.80 seconds. An achievement unlocked fanfare: four ascending bright synth notes in a major key with a shimmering sparkle on top, ending on a held chord that fades by 0.8 s. Celebratory but not loud.
```

**`Purchase.wav`** — compra ou módulo instalado
```
File: Purchase.wav. Sound effect, 0.40 seconds. A purchase confirmation: a short mechanical latch "click-clack" followed by a two-note positive chime, like a module locking into a ship. Clean.
```

**`UiSelect.wav`** — botão focado ou tocado
```
File: UiSelect.wav. Sound effect, 0.05 seconds. A minimal UI hover/focus tick: a single soft high digital click, almost inaudible, no tone, no tail.
```

**`UiConfirm.wav`** — confirmação de menu
```
File: UiConfirm.wav. Sound effect, 0.15 seconds. A UI confirm: a clean two-note rising digital chime, holographic and light, ending by 0.15 s.
```

**`UiError.wav`** — ação inválida
```
File: UiError.wav. Sound effect, 0.20 seconds. A UI error buzz: a short low double "bzz-bzz" digital denial tone, muted and slightly muffled, clearly negative but not harsh.
```

---

## Músicas — salvar em `Assets/_Project/Audio/Final/Music/`

Todas em **120 BPM**, loop perfeito, 60 a 120 segundos, sem vocais. Se o modelo só gerar 30 s, peça "extend to a 64-bar loop" ou gere duas partes na mesma tonalidade e emende no editor.

**`Menu.ogg`** — menu, hangar, ranking
```
File: Menu.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Calm ambient sci-fi for a space game main menu: slow evolving synth pads, a soft arpeggiated pulse, distant reverberated plucks, subtle neon-retro tone, hopeful but quiet, in D minor. No drums or only a very soft heartbeat kick. Must loop perfectly.
```

**`Stage1.ogg`** — fases 1 Iron Belt e 2 Silent Colony
```
File: Stage1.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Light orchestral-hybrid for the opening space stages: strings ostinato, brass accents, a driving but not heavy electronic beat, adventurous and heroic, in D minor, moderate intensity so later stages can grow. Must loop perfectly with no ending.
```

**`Stage2.ogg`** — fases 3 Orbital Wall e 4 Living Nebula
```
File: Stage2.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Spacey electronic action: pulsing analog bass sequence, glittering arpeggios, wide pads, tight electronic drums, futuristic and slightly mysterious for a nebula, in D minor, intensity a step above a calm opening stage. Must loop perfectly.
```

**`Stage3.ogg`** — fases 5 Pirate Corridor e 6 Autonomous Factory
```
File: Stage3.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Synthwave chase: retro 80s synth lead, gated reverb snare, driving sawtooth bass, bright chorus-drenched chords, neon cyberpunk energy, confident and fast-feeling, in D minor. Must loop perfectly.
```

**`Stage4.ogg`** — fases 7 Aether Ruins e 8 Dimensional Rift
```
File: Stage4.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Industrial dark electronic: heavy distorted kick, metallic percussion hits, grinding bass, eerie detuned pads and glitchy textures suggesting ancient ruins and a dimensional rift, tense and relentless, in D minor. Must loop perfectly.
```

**`Stage5.ogg`** — fases 9 Stellar Core e 10 Omega Fortress
```
File: Stage5.ogg. Instrumental music, 120 BPM, seamless loop of about 100 seconds, no vocals. Epic cinematic hybrid for the final stages: full orchestra with choir-like synth pads (no real vocals), powerful percussion, soaring brass theme, electronic bass underneath, the climax of a space war, in D minor. Must loop perfectly with no final cadence.
```

**`Boss.ogg`** — todos os chefes e mini-chefes
```
File: Boss.ogg. Instrumental music, 120 BPM, seamless loop of about 80 seconds, no vocals. Intense boss battle: aggressive electronic-orchestral hybrid, pounding double-time percussion, urgent string stabs, distorted synth riff, sirens-like accents, relentless tension, in D minor. Must loop perfectly.
```

**`FinalBoss.ogg`** — Omega Core
```
File: FinalBoss.ogg. Instrumental music, 120 BPM, seamless loop of about 100 seconds, no vocals. Final boss climax: the most epic and dramatic track of the game, massive orchestral hits, choir-like synths (no real vocals), frantic electronic percussion, a menacing recurring motif that keeps escalating, in D minor. Must loop perfectly.
```

**`Survival.ogg`** — Sobrevivência e Desafio Diário
```
File: Survival.ogg. Instrumental music, 120 BPM, seamless loop of about 90 seconds, no vocals. Endless-waves electronic track: hypnotic driving beat, layered arpeggios that keep building, tense bass pulse, neon energy that feels like it never stops, in D minor. Must loop perfectly.
```

**`OverdriveLayer.ogg`** — camada tocada por cima da música da fase
```
File: OverdriveLayer.ogg. Instrumental music layer, exactly 120 BPM, seamless loop of 32 bars (64 seconds), no vocals, NO melody and NO chords: only rhythm and texture. Purpose: it will be mixed ON TOP of other tracks in D minor, so it must not clash harmonically. Content: extra double-time hi-hats, a driving sixteenth-note percussive synth pulse on the root note D only, risers, white-noise sweeps and energetic transient hits. Must loop perfectly.
```

---

## Ambientes — salvar em `Assets/_Project/Audio/Final/Ambient/`

Loop perfeito de 20 a 60 segundos, sem música, sem melodia, volume baixo. Tocam sob a trilha.

**`Space.ogg`** — espaço aberto
```
File: Space.ogg. Ambient sound bed, seamless loop of 45 seconds, no music, no melody. Deep open space: a very low soft rumble, faint cosmic wind, occasional distant sparse electronic pings from far-away satellites, sparse and calm. Must loop perfectly.
```

**`Asteroids.ogg`** — campo de rochas
```
File: Asteroids.ogg. Ambient sound bed, seamless loop of 45 seconds, no music, no melody. An asteroid field: muffled distant rock collisions, low rolling rumbles, gritty dust hiss, occasional deep thud. Must loop perfectly.
```

**`Nebula.ogg`** — gás ionizado
```
File: Nebula.ogg. Ambient sound bed, seamless loop of 45 seconds, no music, no melody. Inside an ionized nebula: slowly shifting electric hum, soft crackling static, airy shimmering drones with no defined pitch, mysterious and enveloping. Must loop perfectly.
```

**`Fortress.ogg`** — máquinas e alarmes
```
File: Fortress.ogg. Ambient sound bed, seamless loop of 45 seconds, no music, no melody. Inside a giant mechanical fortress: distant industrial machinery, rhythmic hydraulic thumps, steam vents, faint far-off alarms and metallic groans. Must loop perfectly.
```

**`Hive.ogg`** — orgânico e pulsante
```
File: Hive.ogg. Ambient sound bed, seamless loop of 45 seconds, no music, no melody. An alien biomechanical hive: a slow organic heartbeat pulse, wet squelching textures, insect-like chittering far away, low breathing drone, unsettling. Must loop perfectly.
```

---

## Depois de gerar

- Ouça cada loop emendado (toque duas vezes seguidas). Se houver clique ou salto na emenda, peça de novo citando "the loop point clicks; make the end connect to the beginning at the same phase" ou faça um crossfade curto no editor de áudio.
- Efeitos com silêncio no começo: corte o silêncio, senão o tiro soa atrasado.
- Confira que o nome do arquivo está exatamente igual ao pedido (maiúsculas e minúsculas), inclusive `_1`, `_2` para variações.
- Se o Gemini entregar apenas MP3, pode salvar assim; o Unity converte. Só não use MP3 para os loops de música se o arquivo tiver silêncio codificado no início (o MP3 costuma ter): nesse caso converta para WAV/OGG antes ou corte o silêncio.
