# Especificação de arte e áudio finais — Starfall Defense

Lista completa do que substitui os placeholders, por grupo, com finalidade, formato, nome de arquivo e pasta.
O projeto integra os arquivos **automaticamente**: basta colocar cada um na pasta indicada com o nome exato e
executar `Starfall → Run Full Bootstrap` no Unity (ou o comando de linha do README). O que não for entregue
continua usando o placeholder, então dá para entregar aos poucos.

## Regras gerais (valem para tudo)

| Regra | Detalhe |
|---|---|
| Estilo | Pixel art moderna, sci-fi neon com influência cyberpunk (GDD §24). Paleta: azul elétrico, roxo neon, vermelho energético, branco holográfico. Cada facção com identidade: Federação (azul/branco, militar), Biomecânica (verde/orgânico), Cyber (ciano/magenta, hexágonos) |
| Formato de imagem | PNG 32 bits com transparência (RGBA). Sem fundo, sem sombra projetada fora do sprite |
| Canvas | Sempre **quadrado**, o desenho centrado e ocupando o canvas como descrito na coluna "Ocupação". O importador calcula a escala pela largura do arquivo, por isso o tamanho em pixels é livre; a coluna "Canvas sugerido" é para pixel art a 64 px por unidade do jogo. Entregar em 2x (o dobro) também funciona |
| Cor | Entregar já na cor final. Placeholders são brancos e coloridos por código; a arte final **não** recebe tinta, com uma exceção: versões Elite dos inimigos recebem um tom dourado automático por cima |
| Nome | Exatamente como na coluna "Arquivo", em minúsculas, sem espaços. O nome é a chave de integração |
| Pasta | Qualquer subpasta de `Assets/_Project/Art/Final/` funciona, mas mantenha a organização sugerida |
| Orientação | Nave do jogador com o nariz para **cima**. Inimigos e chefes com a frente para **baixo** (eles descem em direção ao jogador). Projéteis apontando para **cima** (o jogo rotaciona) |
| Filtro | O importador usa filtro *Point* (pixel nítido) e sem compressão. Se a arte não for pixel art, avise para trocar para *Bilinear* |
| Formato de áudio | Efeitos em **WAV** 44,1 kHz 16 bits **mono**. Músicas e ambientes em **OGG Vorbis** 44,1 kHz estéreo, com **loop perfeito** (fim emenda no começo sem clique) |
| Nível de áudio | Efeitos com pico em −3 dBFS; músicas normalizadas por volta de −14 LUFS |

Tamanho na tela: o jogo mostra 10 unidades de largura na tela do celular. Uma nave de 1 unidade ocupa 10 % da largura.

---

## 1. Naves do jogador — `Art/Final/Ships/`

Uma imagem por nave. Nariz para cima. Ocupação: o casco preenche a altura do canvas; asas até as bordas laterais.

| Arquivo | Serve para | Facção / identidade | Canvas sugerido | Tamanho na tela |
|---|---|---|---|---|
| `ship.png` | SF-01 Vanguard, nave inicial | Federação, interceptor equilibrado | 64×64 | 0,96 u |
| `falcon.png` | SF-02 Falcon | Federação, estreita e rápida | 64×64 | 0,96 u |
| `titan.png` | SF-03 Titan | Federação, larga e blindada | 64×64 | 0,96 u |
| `phantom.png` | SF-04 Phantom | Cyber, furtiva e angular | 64×64 | 0,96 u |
| `novax.png` | Nova-X, lendária | Híbrida, silhueta em estrela | 72×72 | 1,12 u |
| `symbiont.png` | BX-01 Symbiont | Biomecânica, casco orgânico com tentáculos | 64×64 | 0,96 u |
| `nexus.png` | CX-7 Nexus | Cyber, hexágono com lâminas | 64×64 | 0,96 u |
| `companiondrone.png` | Drone da Nexus, orbita a nave | Cyber, hexágono pequeno | 24×24 | 0,4 u |
| `flame.png` | Chama do motor (todas as naves e inimigos) | Base larga em cima, ponta fina embaixo, brilho aditivo | 24×24 | 0,32 u |

A nave também aparece no Hangar em tamanho grande com efeito de holograma, então capriche na silhueta.

## 2. Projéteis e efeitos — `Art/Final/Projectiles/`

Apontando para cima. Fundo transparente. Os de brilho ("soft") podem ter bordas suaves.

| Arquivo | Serve para | Canvas sugerido | Observação |
|---|---|---|---|
| `projectile.png` | Laser do jogador (cápsula vertical) | 24×24 | também usado pelo Laser Duplo |
| `bullet.png` | Tiro inimigo comum (esfera) e Spread Shot | 24×24 | brilho suave |
| `plasma.png` | Plasma e Canhão de Energia (orbe grande) | 32×32 | brilho suave |
| `missile.png` | Mísseis do jogador e dos chefes | 24×24 | cápsula com aletas |
| `rail.png` | Railgun (traço fino e longo) | 24×24 | brilho suave |
| `spore.png` | Esporos da Symbiont e da Ultimate biomecânica | 24×24 | bolhas orgânicas |
| `web.png` | Teia da Widow (deixa a nave lenta) | 64×64 | anéis concêntricos com raios |
| `dot.png` | Brilho genérico: estrelas, explosão, flash, névoa | 48×48 | círculo com borda suave |
| `spark.png` | Partícula de faísca | 24×24 | traço vertical suave |
| `circle.png` | Corpo dos power-ups | 80×80 | disco cheio |
| `ring.png` | Escudo da nave e dos inimigos, anel dos power-ups, ondas de choque | 80×80 | anel fino |

## 3. Inimigos — `Art/Final/Enemies/`

Frente para baixo. Elites usam a mesma imagem com tom dourado automático e 20 % maiores.

| Arquivo | Serve para | Comportamento (para inspirar o desenho) | Canvas sugerido | Tamanho na tela |
|---|---|---|---|---|
| `drone.png` | Drone (também drone de suprimento, em verde) | desce reto, tiro esparso | 48×48 | 0,72 u |
| `interceptor.png` | Interceptor e Raider pirata (em vermelho) | zigue-zague rápido, rajadas | 48×48 | 0,68 u |
| `bomber.png` | Bomber | lento, largo, solta bombas de área | 64×64 | 1,2 u |
| `kamikaze.png` | Kamikaze | persegue e explode no contato | 48×48 | 0,64 u |
| `shielddrone.png` | Shield Drone | escudo próprio (anel por cima) | 48×48 | 0,8 u |
| `turret.png` | Turret | fixa a posição e dispara anéis | 64×64 | 1,06 u |
| `asteroid.png` | Asteroide destrutível | rocha irregular, rola | 64×64 | 1,06 u |

## 4. Mini-chefes — `Art/Final/Bosses/`

Frente para baixo. Variantes (Mk.II, Prime) reutilizam a imagem com outra cor automática.

| Arquivo | Serve para | Mecânica | Canvas sugerido | Tamanho na tela |
|---|---|---|---|---|
| `sentinelx.png` | Sentinel-X e Sentinel-X Mk.II | robô octogonal, anel giratório de tiros | 160×160 | 3,6 u |
| `widow.png` | Widow e Widow Prime | aranha mecânica, atira teias | 160×160 | 3,8 u |
| `reaperwing.png` | Reaper Wing e Reaper Prime | caça de elite, investidas | 144×144 | 3,1 u |

## 5. Chefes — `Art/Final/Bosses/`

Frente para baixo. Alguns têm partes destrutíveis desenhadas separadamente (seção 6).

| Arquivo | Serve para | Mecânica (fase da campanha) | Canvas sugerido | Tamanho na tela |
|---|---|---|---|---|
| `ironwarden.png` | Iron Warden | mineradora com duas brocas laterais, solta rochas (fase 1) | 200×200 | 5,4 u |
| `destroyer.png` | The Destroyer e Destroyer Mk.II | cruzador militar: canhões laterais, mísseis, laser frontal (fases 2 e 10) | 240×240 | 6,9 u |
| `bastion.png` | Bastion | muralha larga com três baterias (fase 3) | 240×240 | 8,4 u |
| `leviathan_head.png` | Leviathan, cabeça | serpente biomecânica (fase 4) | 128×128 | 2,9 u |
| `leviathan_segment.png` | Leviathan, cada um dos 8 segmentos do corpo | seguem a cabeça, diminuem até a cauda | 64×64 | 1,2 u |
| `corsairqueen.png` | Corsair Queen | nau pirata: investidas, minas, chama capitães (fase 5) | 200×200 | 5,1 u |
| `assembler.png` | The Assembler | fábrica com duas fabricadoras que produzem turrets e drones (fase 6) | 240×240 | 7,7 u |
| `aetherguardian.png` | Aether Guardian | guardião antigo, teleporta; só vulnerável sem os cristais (fase 7) | 200×200 | 5,4 u |
| `riftwalker.png` | Rift Walker | criatura dimensional, teleporta e abre fendas (fase 8) | 200×200 | 5,1 u |
| `hivequeen.png` | Hive Queen | rainha da colmeia, invoca enxames (fase 9) | 240×240 | 7,3 u |
| `omegacore.png` | Omega Core, forma 1 | núcleo octogonal da IA (fase 10) | 240×240 | 7,7 u |
| `omegacore2.png` | Omega Core, forma 2 e forma final | estrela de oito pontas, mais agressiva | 240×240 | 8,8 u |
| `omegacore3.png` | Omega Core, forma 3 | estrela de seis pontas com anel, dispara laser | 240×240 | 9,2 u |

## 6. Partes destrutíveis de chefe — `Art/Final/Bosses/`

Desenhadas isoladas, com fundo transparente. Ficam sobrepostas ao chefe.

| Arquivo | Serve para | Canvas sugerido |
|---|---|---|
| `turretpart.png` | Brocas do Iron Warden e baterias do Bastion | 40×40 |
| `fabricator.png` | Fabricadoras do Assembler | 40×40 |
| `crystal.png` | Cristais que blindam o Aether Guardian | 40×40 |

## 7. Itens e interface — `Art/Final/Items/` e `Art/Final/UI/`

| Arquivo | Pasta | Serve para | Canvas sugerido | Observação |
|---|---|---|---|---|
| `circle.png` | Items | corpo do power-up (colorido por tipo: azul arma, verde escudo, vermelho dano, amarelo velocidade, roxo energia, branco invencível) | 80×80 | entregar em **branco/cinza claro**, é a única exceção: recebe a cor do tipo |
| `panel.png` | UI | fundo de botões, barras e painéis (9-slice) | 64×64 | borda de 16 px, cantos arredondados, centro liso |
| `logo.png` | UI | cartão do estúdio no splash | 256×256 | símbolo do estúdio |
| `Icon/icon.png` | Icon | ícone do aplicativo nas lojas e no celular | **1024×1024, sem transparência, sem cantos arredondados** | vira o ícone de Android e iOS |
| `Fonts/<nome>.ttf` | Fonts | fonte de toda a interface | — | TTF ou OTF com licença comercial; o projeto gera o atlas |

## 8. Cenários — `Art/Final/Environment/`

Fundos são compostos por código: gradiente de cor por fase, estrelas, uma silhueta grande que rola devagar ("backdrop") e destroços. As silhuetas podem ser maiores que o canvas sugerido.

| Arquivo | Serve para | Fases | Canvas sugerido | Observação |
|---|---|---|---|---|
| `planet.png` | Planeta ao fundo | 2 Silent Colony, menu | 512×512 | disco com borda suave, cor final |
| `colony.png` | Domos da colônia | 2 | 512×512 | silhueta translúcida |
| `fortresswall.png` | Muralha/fortaleza mecânica | 3, 6, 10 | 512×512 | pode ser um tile alto |
| `ruins.png` | Pilares das ruínas de Aether | 7 | 512×512 | |
| `rift.png` | Fenda dimensional (anéis) | 8 | 512×512 | |
| `starcore.png` | Núcleo estelar (brilho) | 9 | 512×512 | |
| `hivewall.png` | Favos da colmeia | 9 | 512×512 | |
| `satellite.png` | Destroço de satélite que cai | 1, 2, 3, 6 | 64×64 | |

O céu de cada fase (cores do gradiente, densidade de estrelas e névoa) é configurado em dados e não precisa de imagem.

---

## 9. Efeitos sonoros — `Audio/Final/SFX/`

WAV mono. Nome = identificador. Variações opcionais: `Laser_1.wav`, `Laser_2.wav`, `Laser_3.wav` (o jogo sorteia).

| Arquivo | Serve para | Duração sugerida |
|---|---|---|
| `Laser.wav` | Tiro do laser (frequente, mantenha curto e leve) | 0,1 s |
| `Spread.wav` | Tiro do Spread Shot | 0,1 s |
| `Plasma.wav` | Tiro de plasma e esporos | 0,2 s |
| `Railgun.wav` | Disparo do railgun | 0,4 s |
| `Missile.wav` | Lançamento de míssil (jogador e chefes) | 0,4 s |
| `EnergyCannon.wav` | Disparo do canhão carregado | 0,5 s |
| `Charge.wav` | Início da carga do canhão | 0,6 s |
| `EnemyShot.wav` | Tiro inimigo (muito frequente, discreto) | 0,1 s |
| `Impact.wav` | Tiro acertando casco | 0,1 s |
| `ShieldHit.wav` | Tiro absorvido por escudo | 0,15 s |
| `ExplosionSmall.wav` | Inimigo comum destruído | 0,3 s |
| `ExplosionLarge.wav` | Chefe, mini-chefe ou jogador destruído | 1,0 s |
| `PowerUp.wav` | Coleta de power-up | 0,3 s |
| `PlayerHit.wav` | Jogador recebe dano no casco | 0,2 s |
| `Ultimate.wav` | Ativação da Ultimate e do laser de chefe | 1,0 s |
| `LaserCharge.wav` | Aviso do laser frontal do chefe (telegraph) | 1,0 s |
| `Alarm.wav` | Casco crítico e aviso de solar flare | 0,8 s |
| `BossWarning.wav` | "WARNING" na entrada do chefe e troca de fase | 1,0 s |
| `WebShot.wav` | Widow lança teia | 0,3 s |
| `Summon.wav` | Chefe invoca minions / abre fendas | 0,4 s |
| `Graze.wav` | Projétil passou raspando (frequente, sutil) | 0,08 s |
| `RiskUp.wav` | Zona de risco subiu de nível | 0,2 s |
| `RiskDown.wav` | Zona de risco caiu | 0,2 s |
| `OverdriveStart.wav` | Overdrive ativado | 0,8 s |
| `OverdriveEnd.wav` | Overdrive terminou | 0,5 s |
| `ComboUp.wav` | Multiplicador subiu | 0,15 s |
| `RankReveal.wav` | Letra do rank aparece na tela de resultado | 0,6 s |
| `Achievement.wav` | Conquista desbloqueada | 0,8 s |
| `Purchase.wav` | Compra no Hangar/Melhorias e módulo instalado | 0,4 s |
| `UiSelect.wav` | Botão focado / toque em item | 0,05 s |
| `UiConfirm.wav` | Confirmação de menu | 0,15 s |
| `UiError.wav` | Ação inválida (sem créditos, Ultimate vazia) | 0,2 s |

## 10. Músicas — `Audio/Final/Music/`

OGG estéreo em loop perfeito, 60 a 120 s. Estilos do GDD §21.

| Arquivo | Serve para | Estilo pedido |
|---|---|---|
| `Menu.ogg` | Menu, Hangar, ranking | Ambient sci-fi calmo |
| `Stage1.ogg` | Fases 1 Iron Belt e 2 Silent Colony | Orquestral leve |
| `Stage2.ogg` | Fases 3 Orbital Wall e 4 Living Nebula | Eletrônica espacial |
| `Stage3.ogg` | Fases 5 Pirate Corridor e 6 Autonomous Factory | Synthwave |
| `Stage4.ogg` | Fases 7 Aether Ruins e 8 Dimensional Rift | Industrial |
| `Stage5.ogg` | Fases 9 Stellar Core e 10 Omega Fortress | Épica cinematográfica |
| `Boss.ogg` | Todos os chefes e mini-chefes | Intensa, percussiva |
| `FinalBoss.ogg` | Omega Core | Épica, clímax |
| `Survival.ogg` | Sobrevivência e Desafio Diário | Eletrônica crescente |
| `OverdriveLayer.ogg` | Camada extra tocada **por cima** da música atual enquanto o Overdrive está ativo | Só percussão/textura, sem melodia, no mesmo BPM base das trilhas (sugestão: 120 BPM em todas) |

## 11. Ambientes — `Audio/Final/Ambient/`

OGG em loop perfeito, 20 a 60 s, volume baixo (tocam sob a música).

| Arquivo | Serve para |
|---|---|
| `Space.ogg` | Espaço aberto: menu, fases 1, 2, 3 |
| `Asteroids.ogg` | Campo de rochas: fases 1 e 5 |
| `Nebula.ogg` | Gás ionizado: fases 4 e 7 |
| `Fortress.ogg` | Máquinas e alarmes: fases 3, 6, 10 |
| `Hive.ogg` | Orgânico e pulsante: fases 8 e 9 |

---

## Resumo de quantidades

| Grupo | Arquivos |
|---|---|
| Naves e motor | 9 |
| Projéteis e efeitos | 11 |
| Inimigos | 7 |
| Mini-chefes | 3 |
| Chefes | 13 |
| Partes de chefe | 3 |
| Itens, UI, ícone, fonte | 5 |
| Cenários | 8 |
| **Total de imagens** | **59** |
| Efeitos sonoros | 32 |
| Músicas | 10 |
| Ambientes | 5 |
| **Total de áudio** | **47** |

## Ordem de prioridade para entregar aos poucos

1. `ship.png`, `flame.png`, `projectile.png`, `bullet.png`, `drone.png`, `interceptor.png`, `dot.png`, `ring.png`, `panel.png`, `icon.png`, `Laser.wav`, `ExplosionSmall.wav`, `Stage1.ogg` — já deixam a fase 1 apresentável.
2. Os demais inimigos, `ironwarden.png`, `sentinelx.png`, `Boss.ogg`, `Menu.ogg`.
3. Chefes e mini-chefes restantes, cenários, o resto do áudio.

## Como verificar a integração

1. Coloque os arquivos nas pastas e abra o Unity.
2. Menu `Starfall → Run Full Bootstrap`.
3. O Console lista `Final art: <nome> <- <arquivo>` para cada imagem reconhecida e `Final audio: N clip(s)`.
4. Um nome fora do padrão aparece ausente da lista: confira a grafia.
