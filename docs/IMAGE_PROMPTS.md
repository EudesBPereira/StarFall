# Prompts de imagem para o Gemini — Starfall Defense

Como pedir cada uma das 59 imagens de `docs/ASSET_SPEC.md` a um gerador de imagens.
Os prompts estão em inglês porque os modelos de imagem respondem melhor assim; o nome do arquivo, a finalidade e a pasta estão em português.

## Como usar

1. Abra uma conversa nova com o Gemini no modo de geração de imagem.
2. Cole o **prompt-base** uma única vez.
3. Para cada imagem, cole o prompt individual. Uma imagem por pedido.
4. Peça variações ("give me 4 variations") e escolha a melhor silhueta: em pixel art pequena, a silhueta importa mais que o detalhe.
5. Salve com o **nome exato**, na pasta indicada, e siga a seção "Depois de gerar" (fundo transparente, canvas quadrado, orientação).
6. Rode `Starfall → Run Full Bootstrap` no Unity. O Console lista `Final art: <nome>` para cada imagem reconhecida.

## Prompt-base (cole uma vez por conversa)

```
You are the pixel artist of "Starfall Defense", a 2D vertical space shoot'em up for mobile phones. Art direction: modern pixel art (clean clusters, limited palette, 1-pixel dark outline where it helps readability, no anti-aliasing blur), sci-fi neon with cyberpunk influence. Palette: electric blue #2D6BFF, neon purple #B44CFF, energetic red #FF3B4E, holographic white #E8F6FF, deep space navy #0B1030, plus accent cyan #23D7FF and magenta #FF5AC8.

Three factions with distinct looks:
- Federation (player, human military): sleek angular fighters, white/steel hulls with electric-blue stripes and cyan engine glow.
- Biomech (organic aliens): asymmetric living hulls, green-teal flesh with purple veins, glowing spores, tendrils.
- Cyber Corp (digital corporation): hard hexagonal geometry, black chrome with magenta/cyan neon edges, holographic panels.
- The Swarm (enemies): mechanical alien drones and warships, dark gunmetal with orange or red glowing cores and eyes.

Rules for every image I ask for:
- ONE single sprite, centered, on a plain solid MAGENTA (#FF00FF) background so it can be keyed out (no gradients, no shadow on the ground, no scene, no stars).
- Square canvas. The subject fills the canvas as I describe (typically 85-95% of the height).
- Top-down view (seen from directly above), flat lighting from the top, no perspective, no 3D render look.
- Orientation exactly as I specify: player ships point UP; enemies and bosses point DOWN; projectiles point UP.
- No text, no letters, no logos, no watermark, no signature, no UI, no border.
- Crisp pixel edges, consistent pixel size, no blur, no photo textures.
- Reply with the image only.
```

## Depois de gerar (vale para todas as imagens)

- **Fundo transparente:** remova o magenta com a varinha mágica ou "remover fundo" em qualquer editor (Photopea, GIMP, Aseprite, Paint.NET). Salve como PNG 32 bits. O jogo não faz chroma key: fundo magenta no arquivo final apareceria na tela.
- **Canvas quadrado** e o sprite centrado. Se o gerador entregar retangular, recorte.
- **Tamanho:** o importador ajusta a escala pela largura do arquivo, então qualquer resolução funciona. Se o gerador entregar 1024×1024, reduza para o canvas sugerido com filtro "nearest/vizinho mais próximo" para manter o pixel nítido (ou entregue em 2× do sugerido).
- **Orientação:** confira nariz para cima (jogador), frente para baixo (inimigos e chefes), projéteis para cima. Rotacione se precisar.
- **Cor:** a imagem já vai colorida. Só `circle.png` (power-up) deve ser branca/cinza clara.
- **Nome exato** em minúsculas, na pasta indicada.

---

## 1. Naves do jogador — `Assets/_Project/Art/Final/Ships/`

**`ship.png`** — SF-01 Vanguard
```
File: ship.png. Federation player fighter "SF-01 Vanguard": a balanced arrowhead interceptor pointing UP, symmetrical, white-steel hull with electric-blue stripes, small cockpit canopy glowing cyan, two swept wings, twin engine nozzles at the bottom. Fills 90% of the canvas height. Top-down pixel art sprite on magenta background.
```

**`falcon.png`** — SF-02 Falcon
```
File: falcon.png. Federation fighter "SF-02 Falcon": a very narrow, needle-like fast interceptor pointing UP, long thin fuselage, small swept-back wings near the tail, white hull with blue racing stripes, single large engine glowing cyan. Looks faster and more fragile than a standard fighter. Fills 90% of the canvas height, but narrow. Top-down pixel art on magenta background.
```

**`titan.png`** — SF-03 Titan
```
File: titan.png. Federation heavy fighter "SF-03 Titan": a wide, blocky, heavily armored gunship pointing UP, thick hull plates with rivets, two bulky side pods, short wings reaching the canvas edges, steel-grey with blue trim, four engine nozzles. Looks slow and tanky. Fills 90% of canvas height and 95% of width. Top-down pixel art on magenta background.
```

**`phantom.png`** — SF-04 Phantom
```
File: phantom.png. Cyber Corp stealth fighter "SF-04 Phantom": a sleek angular dart pointing UP with sharp faceted wings, black chrome hull with thin magenta and cyan neon edge lines, a narrow glowing visor cockpit, engines glowing magenta. Looks precise and predatory. Fills 90% of canvas height. Top-down pixel art on magenta background.
```

**`novax.png`** — Nova-X
```
File: novax.png. Legendary hybrid prototype "Nova-X": a four-pointed star-shaped ship pointing UP, blending Federation steel, biomech green tendrils and cyber neon edges, a bright white-gold energy core in the center, radiant and slightly unstable looking. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`symbiont.png`** — BX-01 Symbiont
```
File: symbiont.png. Biomech player ship "BX-01 Symbiont": an organic living hull pointing UP, teardrop body of green-teal flesh with purple veins and a glowing green eye-like cockpit, two pairs of curling tendrils sweeping back and outward, small spore sacs glowing green, no straight lines. Fills 90% of the canvas height. Top-down pixel art on magenta background.
```

**`nexus.png`** — CX-7 Nexus
```
File: nexus.png. Cyber Corp command fighter "CX-7 Nexus": a hexagonal core hull pointing UP with a pointed nose, two blade-like angular wings, black chrome with cyan hexagonal neon panels and magenta edge lights, a holographic ring glowing in the center. Fills 90% of the canvas height. Top-down pixel art on magenta background.
```

**`companiondrone.png`** — drone da Nexus
```
File: companiondrone.png. A tiny Cyber Corp combat drone seen from above: a small hexagon with a glowing cyan center and a single tiny cannon pointing UP, black chrome with cyan neon edges. Very simple, readable at 24 pixels. Fills 85% of the canvas. Top-down pixel art on magenta background.
```

**`flame.png`** — chama do motor
```
File: flame.png. An engine exhaust flame sprite: a teardrop flame with the WIDE base at the TOP of the canvas and the thin pointed tip at the BOTTOM, bright white-cyan core fading to blue edges, soft glow, drawn in white and light blue tones only (it will be recolored per ship). Fills 90% of the canvas height. Pixel art on magenta background.
```

## 2. Projéteis e efeitos — `Assets/_Project/Art/Final/Projectiles/`

**`projectile.png`** — laser do jogador
```
File: projectile.png. A player laser bolt: a vertical capsule pointing UP, bright white core with cyan-blue glowing edges, slightly longer than wide (about 1:3 width to height), crisp. Centered, fills 85% of the canvas height. Pixel art on magenta background.
```

**`bullet.png`** — tiro inimigo
```
File: bullet.png. A small round energy pellet: a glowing orb with a bright white center fading to orange-red edges, perfectly round, soft glow at the rim. Fills 80% of the canvas. Pixel art on magenta background.
```

**`plasma.png`** — plasma
```
File: plasma.png. A large plasma orb: a round ball of purple-violet energy with a white-hot center, crackling bright edges and a few small sparks around it, glowing. Fills 85% of the canvas. Pixel art on magenta background.
```

**`missile.png`** — míssil
```
File: missile.png. A small rocket missile pointing UP: slim grey-white body, red nose cone, three small fins at the bottom, a tiny orange exhaust flame under the fins. Fills 90% of the canvas height, narrow. Top-down pixel art on magenta background.
```

**`rail.png`** — railgun
```
File: rail.png. A railgun slug trail: a very thin, long vertical streak pointing UP, bright white core with pale blue electric edges, tapered at both ends. Fills 95% of the canvas height and about 20% of the width. Pixel art on magenta background.
```

**`spore.png`** — esporo biomecânico
```
File: spore.png. A biomech spore projectile: a cluster of three small glowing green bubbles of different sizes, organic, with darker green rims and a faint purple vein. Fills 80% of the canvas. Pixel art on magenta background.
```

**`web.png`** — teia da Widow
```
File: web.png. An energy spider web: concentric octagonal rings connected by radial threads, pale cyan-white glowing strands on a dark center, symmetrical, like a mechanical spider web made of light. Fills 95% of the canvas. Pixel art on magenta background.
```

**`dot.png`** — brilho genérico
```
File: dot.png. A soft round glow sprite: a white circle with a bright solid center and edges fading smoothly to transparent-looking magenta background, no outline, no detail. Pure white only (it will be recolored). Fills 90% of the canvas. On magenta background.
```

**`spark.png`** — faísca
```
File: spark.png. A single spark particle: a short vertical white streak with a bright center, soft ends, pure white only (it will be recolored). Fills 70% of the canvas height, very thin. On magenta background.
```

**`circle.png`** — corpo do power-up (branco)
```
File: circle.png. A power-up capsule body: a perfect filled circle in light grey-white (#E8E8E8) with a slightly brighter center and a thin lighter rim, no other color (the game tints it per power-up type). Fills 90% of the canvas. Pixel art on magenta background.
```

**`ring.png`** — anel de escudo
```
File: ring.png. An energy shield ring: a thin circular ring of white-cyan light with a soft glow inside and outside the line, hollow center, thickness about 10% of the diameter, pure white and light cyan only. Fills 92% of the canvas. Pixel art on magenta background.
```

## 3. Inimigos — `Assets/_Project/Art/Final/Enemies/`

Todos apontando para BAIXO. Paleta Swarm: metal escuro com brilho laranja ou vermelho.

**`drone.png`** — Drone
```
File: drone.png. Swarm scout drone pointing DOWN: a small hexagonal mechanical drone, dark gunmetal plates, one glowing orange eye-core in the center, two tiny gun prongs at the bottom, simple and readable at 48 pixels. Fills 85% of the canvas. Top-down pixel art on magenta background.
```

**`interceptor.png`** — Interceptor
```
File: interceptor.png. Swarm interceptor pointing DOWN: a fast dart-shaped fighter with forward-swept wings, dark gunmetal with orange glowing engine slits at the top (its rear), a red sensor eye at the nose (bottom). Aggressive, sharp. Fills 90% of the canvas height. Top-down pixel art on magenta background.
```

**`bomber.png`** — Bomber
```
File: bomber.png. Swarm bomber pointing DOWN: a wide, heavy hexagonal hull with thick armor, a large bomb bay glowing red-orange on its underside center, stubby wings reaching the canvas edges, dark gunmetal, looks slow and heavy. Fills 95% of the canvas width and 80% of the height. Top-down pixel art on magenta background.
```

**`kamikaze.png`** — Kamikaze
```
File: kamikaze.png. Swarm kamikaze drone pointing DOWN: a four-pointed spiky star-shaped mine-drone with a pulsing red core, jagged dark metal spikes, small red warning lights, dangerous looking. Fills 90% of the canvas. Top-down pixel art on magenta background.
```

**`shielddrone.png`** — Shield Drone
```
File: shielddrone.png. Swarm shield drone pointing DOWN: a squarish rounded mechanical drone with a small pointed nose at the bottom, dark gunmetal with teal-cyan glowing shield emitters at the four corners. Fills 85% of the canvas. Top-down pixel art on magenta background.
```

**`turret.png`** — Turret
```
File: turret.png. Swarm automated turret seen from above: an octagonal armored base plate, dark gunmetal, a central rotating cannon with three short barrels pointing DOWN, orange glowing seams. Fills 90% of the canvas. Top-down pixel art on magenta background.
```

**`asteroid.png`** — Asteroide
```
File: asteroid.png. A space asteroid seen from above: an irregular lumpy rock, grey-brown with darker craters and a few lighter highlights, some small orange mineral veins, no glow, natural and rocky. Fills 90% of the canvas. Pixel art on magenta background.
```

## 4. Mini-chefes — `Assets/_Project/Art/Final/Bosses/`

**`sentinelx.png`** — Sentinel-X
```
File: sentinelx.png. Swarm mini-boss "Sentinel-X" pointing DOWN: a giant octagonal war robot with four rotating cannon pods at the four sides (top, bottom, left, right), a large purple glowing core ring in the center, dark gunmetal with purple energy lines. Menacing, symmetrical. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`widow.png`** — Widow
```
File: widow.png. Swarm mini-boss "Widow" pointing DOWN: a mechanical space spider seen from above, round armored abdomen at the top, smaller head at the bottom with glowing cyan eyes, eight articulated metal legs spread to the canvas edges, dark chrome with pale cyan web-energy glowing between the leg joints. Fills 95% of the canvas. Top-down pixel art on magenta background.
```

**`reaperwing.png`** — Reaper Wing
```
File: reaperwing.png. Swarm mini-boss "Reaper Wing" pointing DOWN: an elite ace fighter with huge scythe-shaped wings sweeping upward and outward, narrow blade-like fuselage, dark red and black hull with red glowing engine trails at the top, a single menacing red visor. Fast and lethal. Fills 95% of the canvas. Top-down pixel art on magenta background.
```

## 5. Chefes — `Assets/_Project/Art/Final/Bosses/`

Todos apontando para BAIXO. As partes destrutíveis (brocas, baterias, fabricadoras, cristais) são arquivos separados: desenhe o corpo com os encaixes vazios ou discretos.

**`ironwarden.png`** — Iron Warden (fase 1)
```
File: ironwarden.png. Boss "Iron Warden" pointing DOWN: a massive industrial mining warship, rust-brown and iron-grey plating, a wide boxy body with a large crusher jaw at the bottom, two big shoulder mounts on the left and right where drill arms attach (draw the mounts as sockets, NOT the drills), orange furnace glow from vents, dust and rivets. Heavy and brutal. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`destroyer.png`** — The Destroyer (fases 2 e 10)
```
File: destroyer.png. Boss "The Destroyer" pointing DOWN: a long military Swarm cruiser, dark gunmetal with red glowing lines, a tapered hull with a large laser emitter at the bottom tip, two wide side wings each carrying a heavy cannon and a missile pod, engine array glowing red at the top. Imposing capital ship. Fills 92% of the canvas height and 95% of width. Top-down pixel art on magenta background.
```

**`bastion.png`** — Bastion (fase 3)
```
File: bastion.png. Boss "Bastion" pointing DOWN: a wide orbital defense wall segment, a horizontal armored bulwark spanning the full canvas width at the top half, with three empty turret sockets (left, center-top, right) drawn as recessed circular mounts, a central command block below with a laser aperture at the bottom, dark gunmetal with orange warning lights. Fills 95% of the canvas width. Top-down pixel art on magenta background.
```

**`leviathan_head.png`** — Leviathan, cabeça (fase 4)
```
File: leviathan_head.png. Boss "Leviathan" head pointing DOWN: the head of a giant biomechanical space serpent, wedge-shaped skull of green-teal flesh fused with dark metal plates, open jaws with glowing teal energy inside, two purple glowing eyes, two curved horns sweeping upward toward the canvas top corners. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`leviathan_segment.png`** — Leviathan, segmento
```
File: leviathan_segment.png. One body segment of the biomechanical serpent "Leviathan": a rounded barrel-shaped vertebra seen from above, green-teal flesh with a dark metal ring, a purple glowing spine line down the center, two small fins on the left and right. Must tile visually with copies of itself above and below. Fills 85% of the canvas. Pixel art on magenta background.
```

**`corsairqueen.png`** — Corsair Queen (fase 5)
```
File: corsairqueen.png. Boss "Corsair Queen" pointing DOWN: a pirate flagship, a sleek raked hull like a space galleon, dark crimson and black plating with gold trim, two large angular wings with cannon rows, a skull-like sensor cluster at the nose (bottom), orange engine glow at the top, mine launchers on the flanks. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`assembler.png`** — The Assembler (fase 6)
```
File: assembler.png. Boss "The Assembler" pointing DOWN: a flying automated factory, a wide rectangular industrial hull with three smokestacks at the top, conveyor rails, two empty circular docking sockets on the left and right sides (where fabricator modules attach; draw only the sockets), a central assembly bay glowing orange at the bottom. Grey-yellow industrial metal with hazard stripes. Fills 95% of the canvas width. Top-down pixel art on magenta background.
```

**`aetherguardian.png`** — Aether Guardian (fase 7)
```
File: aetherguardian.png. Boss "Aether Guardian" pointing DOWN: an ancient alien guardian construct, a three-pointed triangular body of pale stone-white and gold with glowing purple runes, a hovering ring around the center and a bright violet core, no visible engines, mystical and precise. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`riftwalker.png`** — Rift Walker (fase 8)
```
File: riftwalker.png. Boss "Rift Walker" pointing DOWN: a dimensional creature, an elongated dark violet body with four long jointed limbs stretching to the canvas corners, a glowing magenta rift-eye in the center, edges that look unstable and glitched with purple energy. Alien, unsettling. Fills 95% of the canvas. Top-down pixel art on magenta background.
```

**`hivequeen.png`** — Hive Queen (fase 9)
```
File: hivequeen.png. Boss "Hive Queen" pointing DOWN: a huge biomech insect queen seen from above, bulbous purple-pink abdomen at the top, armored thorax, a crowned head at the bottom with glowing pink eyes, four hexagonal egg-sacs glowing on the sides, chitinous purple-black plates with magenta glow. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`omegacore.png`** — Omega Core, forma 1 (fase 10)
```
File: omegacore.png. Final boss "Omega Core" form 1: a giant octagonal AI core seen from above, black chrome plating with cyan and white neon circuitry, a bright glowing blue-white central eye-core surrounded by a rotating ring, symmetrical, cold and clean. Fills 92% of the canvas. Top-down pixel art on magenta background.
```

**`omegacore2.png`** — Omega Core, forma 2
```
File: omegacore2.png. Final boss "Omega Core" form 2: the same AI core transformed into an eight-pointed star of black chrome blades, cyan circuitry now glowing red-orange, the central core cracked and blazing white-red. More aggressive than form 1, same size. Fills 95% of the canvas. Top-down pixel art on magenta background.
```

**`omegacore3.png`** — Omega Core, forma 3
```
File: omegacore3.png. Final boss "Omega Core" form 3: a six-pointed star core with a large glowing ring orbiting it and a bright violet laser emitter at the bottom point, black chrome with violet-magenta circuitry, unstable energy arcs between the points. Fills 95% of the canvas. Top-down pixel art on magenta background.
```

## 6. Partes destrutíveis — `Assets/_Project/Art/Final/Bosses/`

**`turretpart.png`** — brocas do Iron Warden e baterias do Bastion
```
File: turretpart.png. A detachable heavy gun turret seen from above: an octagonal armored base with a single thick barrel pointing DOWN, dark gunmetal with an orange glowing seam ring. Simple, readable at 40 pixels. Fills 88% of the canvas. Top-down pixel art on magenta background.
```

**`fabricator.png`** — fabricadoras do Assembler
```
File: fabricator.png. A detachable factory module seen from above: a rounded rectangular industrial pod with a glowing orange circular port in the center and hazard stripes on the edges, grey-yellow metal. Fills 88% of the canvas. Top-down pixel art on magenta background.
```

**`crystal.png`** — cristais do Aether Guardian
```
File: crystal.png. A floating energy crystal: a tall faceted gem pointing UP, translucent violet-purple with bright white highlights and a glowing core, thin gold cap at the top. Fills 90% of the canvas height. Pixel art on magenta background.
```

## 7. Itens e interface — `Assets/_Project/Art/Final/Items/`, `UI/`, `Icon/`

**`circle.png`** — power-up (já descrito na seção 2; salvar em `Items/` ou `Projectiles/`, tanto faz)

**`panel.png`** — painel 9-slice
```
File: panel.png. A UI panel tile for 9-slice scaling: a rounded rectangle filling the whole canvas, dark navy fill (#101838) with a 2-pixel holographic cyan border and slightly lighter inner bevel, corners rounded with a radius of about 12 pixels on a 64x64 canvas, the center area perfectly flat and uniform so it can stretch. No text. Pixel art. (Background outside the rounded corners: magenta.)
```

**`logo.png`** — símbolo do estúdio
```
File: logo.png. A studio emblem for a splash screen: a minimalist five-pointed star inside a thin circular ring, holographic cyan and white on magenta background, clean vector-like pixel art, no letters, no text. Fills 90% of the canvas.
```

**`Icon/icon.png`** — ícone do aplicativo
```
File: icon.png. App icon for a space shooter game, 1024x1024, FULL-BLEED with NO transparency and NO magenta: a deep navy-to-purple space gradient background with a few stars, the Federation fighter "SF-01 Vanguard" (white-steel arrowhead with electric-blue stripes and cyan engine glow) pointing UP in the center, a bright orange enemy explosion glow behind it, bold and readable at small size, no text, no border, square corners (the stores round them).
```

## 8. Cenários — `Assets/_Project/Art/Final/Environment/`

Estes são silhuetas grandes desenhadas por trás da ação. Podem ter bordas suaves. Fundo magenta continua obrigatório, exceto onde indicado.

**`planet.png`** — planeta
```
File: planet.png. A planet seen from space: a perfect circle filling 90% of the canvas, blue oceans and green-brown continents under thin white clouds, a soft glowing atmosphere rim, slightly darker on the bottom-right. Pixel art with smooth shading, on magenta background.
```

**`colony.png`** — domos da colônia
```
File: colony.png. A silent space colony silhouette: a cluster of three glass domes of different sizes on a flat metal base spanning the canvas bottom, dim teal-grey structures with a few tiny warm windows, dark and quiet, semi-transparent feel. Fills the bottom 70% of the canvas. Pixel art on magenta background.
```

**`fortresswall.png`** — muralha mecânica
```
File: fortresswall.png. A mechanical fortress wall segment seen from above: a full-width horizontal band of dark gunmetal armor with vertical pillars, vents, hazard stripes and orange lights, tileable left-to-right and top-to-bottom. Fills the entire canvas (no empty area). Pixel art. No magenta needed: fill the whole canvas.
```

**`ruins.png`** — ruínas de Aether
```
File: ruins.png. Ancient alien ruins: four tall broken stone pillars of pale white-gold with glowing purple rune lines, a cracked lintel across the top, floating fragments, mystical. Fills 90% of the canvas height. Pixel art on magenta background.
```

**`rift.png`** — fenda dimensional
```
File: rift.png. A dimensional rift seen from above: three concentric glowing violet-magenta rings with a bright white-pink center vortex, thin electric arcs between the rings, dark space between. Fills 92% of the canvas. Pixel art on magenta background.
```

**`starcore.png`** — núcleo estelar
```
File: starcore.png. A stellar core glow: a huge soft-edged orange-white sun disc with a blazing white center and orange-red corona fading outward, no hard outline. Fills 80% of the canvas. Pixel art with smooth glow, on magenta background.
```

**`hivewall.png`** — favos da colmeia
```
File: hivewall.png. A biomech hive wall: a honeycomb of hexagonal cells of purple-black chitin with pink glowing cores inside some cells, wet organic texture, tileable in all directions. Fills the entire canvas (no empty area). Pixel art. No magenta needed: fill the whole canvas.
```

**`satellite.png`** — destroço de satélite
```
File: satellite.png. A wrecked satellite seen from above: a small boxy central body with two rectangular solar panels on the left and right, one panel bent and broken, a snapped antenna dish at the top, grey metal with dark blue panels. Fills 85% of the canvas width. Top-down pixel art on magenta background.
```

---

## Conferência final

| Verifique | Como |
|---|---|
| Fundo transparente de verdade | Abra o PNG sobre um fundo escuro: não pode haver magenta ou halo rosa nas bordas (use "defringe" ou apague 1 px de borda se houver) |
| Orientação | Nave do jogador para cima; inimigos e chefes para baixo; projéteis para cima |
| Canvas quadrado e centrado | Recorte simétrico; sprites tortos ficam deslocados da hitbox no jogo |
| Nome e pasta | Igual à lista, em minúsculas; `icon.png` em `Icon/`, fonte em `Fonts/` |
| Legibilidade | Reduza a imagem ao tamanho sugerido em `ASSET_SPEC.md` e veja se a silhueta ainda se reconhece; se não, simplifique |

Depois de colocar os arquivos, `Starfall → Run Full Bootstrap` e me avise para gerar um APK novo e instalar no telefone.
