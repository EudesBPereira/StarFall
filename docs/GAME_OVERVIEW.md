# Starfall Defense — Visão completa do jogo (estado atual)

Documento de avaliação: descreve o jogo **como ele está construído hoje**, extraído diretamente do código e dos dados gerados (`ContentFactory`, `Scripts/Logic`, `docs/BALANCING.md`). Onde o texto do jogo é citado, está na versão em português exibida ao jogador. Data: 10/09/2026. Versão do build: 0.2.0.

---

## 1. Ficha técnica

| Item | Estado |
|---|---|
| Gênero | Shoot'em up 2D vertical (portrait), arcade com progressão permanente |
| Plataformas | Android (build e instalação validados) e iOS (projeto preparado, sem build) |
| Engine | Unity 6000.3.23f1 |
| Idiomas | Inglês e português do Brasil (detecção automática + troca em Configurações) |
| Controles | Arrastar na tela (toque), teclado e controle; disparo automático opcional |
| Conteúdo | 10 fases, 10 chefes, 3 mini-chefes + 3 variantes, 7 naves em 3 facções, 8 armas, 9 inimigos base + 6 elites, 6 power-ups, 12 módulos temporários, 10 upgrades permanentes, 4 modos, 5 conquistas |
| Arte | 56 sprites finais gerados por IA + ícone, integrados; fonte da UI ainda padrão |
| Áudio | 47 arquivos finais (32 efeitos, 5 ambientes, 10 trilhas) gerados pela ElevenLabs; mixagem ajustada em 10/09 |
| Testes | 127 testes automatizados (regras puras e Editor), todos passando |
| Save | Local, JSON versão 2, com migração e redefinição |
| Fora do escopo por enquanto | Monetização, analytics, serviços online, ranking global, coop (fases F e G do plano) |

---

## 2. Enredo

### Premissa

Ano **2237**. Depois de décadas explorando galáxias distantes, a humanidade encontra uma antiga civilização mecânica conhecida como **The Swarm** (o Enxame). Ela inicia uma invasão em larga escala rumo ao Sistema Solar. A última esperança da Terra é a nave experimental **SF-01 Vanguard**, da Frota de Defesa da Terra, pilotada pelo jogador. A missão: atravessar dez setores do espaço destruindo as frotas inimigas antes que alcancem a Terra.

### Arco da campanha (dez setores)

O enredo é contado pelos briefings de missão, pelos objetivos e pelas mensagens em tela. Não há cutscenes nem diálogos.

| Setor | Nome | Situação | Antagonistas | O que o jogador descobre |
|---|---|---|---|---|
| 1 | **Cinturão de Ferro** | O Enxame atacou primeiro o cinturão de mineração. Rochas por toda parte, drones entre elas. Serve de tutorial: arrastar para mover, arma automática, Risco e Raspão. | Sentinel-X, Iron Warden | Primeiro contato; o Enxame usa máquinas de mineração como armas |
| 2 | **Colônia Silenciosa** | Colônia morta na órbita da Terra. Os domos não respondem. Um cargueiro batedor do Enxame está parado sobre eles. Tempestades solares. | Widow, The Destroyer | O Enxame já chegou à órbita da Terra e faz reconhecimento |
| 3 | **Muralha Orbital** | O Enxame cercou o planeta com uma corrente de plataformas de canhão. Torretas mantêm a linha e atiram em todas as direções. | Reaper Wing, Bastion | Bloqueio organizado; é preciso rompê-lo |
| 4 | **Nebulosa Viva** | Sensores falham na nebulosa e o próprio gás pulsa. O Enxame se reproduz ali. Todos os tipos de inimigo aparecem, com escoltas de elite. | Widow Prime, Leviathan | O Enxame não só constrói: ele cria |
| 5 | **Corredor Pirata** | Piratas humanos venderam o corredor ao Enxame. Saqueadores vêm pelos lados, minas flutuam e a rainha deles luta sujo. | Sentinel-X Mk.II, Corsair Queen | Há humanos colaborando com a invasão |
| 6 | **Fábrica Autônoma** | Uma fábrica do tamanho de uma lua constrói o exército do Enxame em tempo real. Torretas saem da linha e o Assembler continua produzindo; destruir as fabricadoras interrompe o fluxo. | Reaper Prime, The Assembler | A origem do suprimento infinito de inimigos |
| 7 | **Ruínas de Aether** | Ruínas de uma civilização mais antiga que o Enxame. O Enxame escava em busca de algo. O guardião ainda está de pé, com o núcleo protegido por cristais. | Widow Prime, Aether Guardian | Existe algo anterior ao Enxame que ele procura |
| 8 | **Fenda Dimensional** | O Enxame abriu uma fenda para trazer reforços de outro lugar. Fendas se abrem em qualquer ponto da tela após um aviso; o Rift Walker se teleporta e rasga novas. | Reaper Prime, Rift Walker | O Enxame vem de fora deste espaço |
| 9 | **Núcleo Estelar** | A colmeia do Enxame envolve uma estrela moribunda. Calor, enxames e a rainha. Tempestades solares constantes; tudo vem em números. | Sentinel-X Mk.II, Hive Queen | A colmeia-mãe; o coração biológico do Enxame |
| 10 | **Fortaleza Ômega** | Fim da linha: o Omega Core, a inteligência por trás da invasão. A fortaleza joga tudo antes de o núcleo acordar; ele se transforma conforme sofre dano. | Destroyer Mk.II, Omega Core | A mente-máquina central; derrotá-la encerra a invasão ("O ENXAME FOI DERROTADO") |

### Facções jogáveis

As três facções são origens das naves, não personagens da história. O texto de cada nave dá o único contexto:

- **Federação (Frota de Defesa da Terra):** interceptadores e quadros de assalto experimentais (Vanguard, Falcon, Titan, Nova-X).
- **Biomech:** organismos biomecânicos enxertados em um casco; os esporos caçam sozinhos e cada abate alimenta a nave (Symbiont).
- **Cyber Corp:** quadros corporativos furtivos e de comando, com drone de combate e sistemas de precisão (Phantom, Nexus).

---

## 3. Loop de jogo

1. Menu principal → **Jogar** → seleção de missão (campanha ou modo extra).
2. **Briefing** da fase: título, subtítulo, texto, objetivos e loadout atual → **Lançar**.
3. Gameplay: ondas de inimigos, eventos ambientais, encontros aleatórios, mini-chefe no meio, escolha de módulo temporário, chefe no fim.
4. **Resultado**: pontuação detalhada, rank D a SSS, créditos, XP e componentes.
5. **Hangar** (comprar/equipar naves e armas) e **Melhorias** (upgrades permanentes).
6. Próxima fase.

Controles: movimento em oito direções com aceleração, desaceleração e inclinação lateral; a nave fica presa à área visível. No toque, a nave segue o arrasto com sensibilidade ajustável. Disparo automático ligado por padrão.

---

## 4. Sistemas centrais (o "STAR RISK")

### 4.1 Zona de Risco

Um sensor ao redor da nave (raio 3,6 unidades) soma a ameaça de inimigos e projéteis próximos, com peso maior quanto mais perto (quadrático). O valor suavizado sobe rápido (3,5/s) e cai devagar (1,2/s), com trava mínima de 0,35 s por estado, para não piscar.

| Estado | Limiar | Multiplicador de pontos | HUD |
|---|---:|---:|---|
| SEGURO | < 0,25 | x1 | RISCO SEGURO x1 |
| ALERTA | ≥ 0,25 | x2 | RISCO ALERTA! x2 |
| PERIGO | ≥ 0,60 | x3 | RISCO PERIGO!! x3 |
| EXTREMO | ≥ 1,10 | x5 | RISCO EXTREMO!!! x5 |

Pesos de ameaça: inimigo comum 1,0; projétil 0,55; bomber e torreta 1,2; kamikaze 1,6; elite 1,4; chefe 2,0; drone de suprimento 0,2; asteroide 0,7.

### 4.2 Overdrive

Medidor de 0 a 100 preenchido por jogo arriscado. Ao encher, ativa sozinho por **8 segundos**.

| Fonte | Ganho |
|---|---:|
| Em PERIGO | +7/s |
| Em EXTREMO | +14/s |
| Em SEGURO | −5/s (drena) |
| Raspão | +4 |
| Abate a menos de 2,2 u | +5 |
| Abate em combo | +1 |
| Parte de chefe destruída | +10 |
| Levar dano (carregando) | −30 |
| Levar dano (ativo) | perde metade do tempo restante |
| Morrer | zera |

Enquanto ativo: multiplicador de risco ×1,6 (teto x8), cadência de tiro maior (por facção), carga da Ultimate ×1,5, camada musical extra de percussão. Reações por facção: Federação +10 % a +30 % de cadência (Nova-X +30 %, mas ganha Overdrive 30 % mais devagar); Cyber +15 % a +20 % de chance de crítico; Biomech +8 de escudo por segundo e +5 % de cadência.

### 4.3 Raspão (Graze)

Projétil inimigo que passa a até 0,42 u do hitbox sem acertar. Vale **50 pontos × multiplicador de risco** (não multiplica pelo combo) e +4 de Overdrive. Não conta com invulnerabilidade nem com projéteis lentos (< 2,5 u/s). Aviso visual e sonoro desligável em Configurações.

### 4.4 Pontuação e combo

- Pontos por abate = valor base × combo × risco.
- Combo de x1 a **x10**: sobe um nível a cada 4 abates sem levar dano; zera ao levar dano.
- Valores base: inimigo comum 100, drone de suprimento 50, elite 500, mini-chefe 2 500, chefe 10 000, partes de chefe 700 a 900.

### 4.5 Ultimate

Barra de energia de 100 que enche com abates (cada inimigo dá de 2 a 12 de energia, mini-chefe 40, chefe 60 a 80) e com o power-up roxo (+40). Só dispara cheia. Cada facção tem a sua:

| Facção | Ultimate | Efeito |
|---|---|---|
| Federação | **Orbital Strike** | Elimina todos os inimigos comuns na tela, causa 150 de dano (× potência) a elites e chefes, limpa os projéteis inimigos |
| Biomech | **Spore Swarm** | Solta 10 esporos teleguiados de 45 de dano (área 0,7 u) que caçam por 6 s; sem limpeza instantânea |
| Cyber | **EMP Burst** | Atordoa tudo por 3 s (chefes 1,5 s), 90 de dano, marca todos os alvos por 6 s (+25 % de dano recebido), limpa projéteis |

### 4.6 Vidas, escudo, casco e dano

- **3 vidas** iniciais, máximo 9 (não há mecânica de vida extra implementada hoje; o campo de "revive" existe para a fase F).
- Escudo absorve antes do casco; regenera por nave após 4 s sem dano. Casco abaixo de 30 % entra em estado crítico (fumaça, alarme, aviso no HUD).
- Ao levar dano: 1 s de invulnerabilidade. Ao morrer: respawn em 1,5 s com 2,5 s de invulnerabilidade; o combo, o Overdrive e os efeitos temporários são perdidos.
- Crítico: chance e multiplicador por nave (ex.: Phantom 25 % × 3,0). Tipos de dano: laser, plasma, cinético, explosivo, energia, contato.
- Passivas: **Precisão** (Federação) +1 % de dano por acerto consecutivo, teto +25 %, zera ao errar. **Marcação** (Cyber) cada acerto marca o alvo por 4 s com +25 % de dano recebido. **Lifesteal** (Biomech) cada abate cura 2 % do casco máximo, teto 5 %.

---

## 5. Naves

| Nave | Facção | Casco | Escudo | Regen/s | Veloc. | Crítico | Dano | Ult. carga / potência | Overdrive | Extra | Custo |
|---|---|---:|---:|---:|---:|---|---:|---|---|---|---:|
| **SF-01 Vanguard** | Federação | 100 | 50 | 0 | 9,0 | 5 % ×2,0 | ×1,00 | ×1,0 / ×1,0 | +15 % cadência | Precisão | inicial |
| **SF-02 Falcon** | Federação | 75 | 40 | 0 | 12,0 | 8 % ×2,0 | ×1,00 | ×1,15 / ×0,9 | +20 % cadência | Precisão | 2 500 |
| **SF-03 Titan** | Federação | 160 | 90 | 1,5 | 6,8 | 3 % ×1,8 | ×1,10 | ×0,9 / ×1,3 | +10 % cadência | Precisão | 4 000 |
| **SF-04 Phantom** | Cyber | 85 | 45 | 0,5 | 10,0 | 25 % ×3,0 | ×0,95 | ×1,0 / ×1,0 | +20 % crítico | Marcação, EMP | 6 000 |
| **Nova-X** | Federação | 90 | 40 | 0 | 11,0 | 12 % ×2,2 | ×1,15 | ×1,0 / ×1,4 | +30 % cadência, ganho ×0,7 | Precisão | completar a campanha |
| **BX-01 Symbiont** | Biomech | 110 | 30 | 1,0 | 8,5 | 4 % ×1,8 | ×1,00 | ×1,1 / ×1,0 | +8 escudo/s, +5 % cadência | Lifesteal 2 %, arma Spore Launcher, Spore Swarm | 3 500 |
| **CX-7 Nexus** | Cyber | 85 | 60 | 0,8 | 10,0 | 10 % ×2,0 | ×0,90 | ×1,0 / ×1,0 | +15 % crítico | Drone companheiro (4 de dano a cada 0,5 s), Marcação, EMP | 5 000 |

Descrições no jogo: Vanguard "interceptador experimental equilibrado da Frota de Defesa da Terra"; Falcon "muito rápida, casco fino, reflexos afiados"; Titan "quadro de assalto pesado, lento, mas aguenta castigo"; Phantom "interceptador furtivo corporativo, crítico alto e Overdrive que empurra os críticos ainda mais"; Nova-X "protótipo híbrido experimental, potencial de pontuação enorme e Ultimate devastadora, mas blindagem fina, sem regeneração e um Overdrive que só recompensa agressividade constante"; Symbiont "organismo biomecânico enxertado a um casco"; Nexus "quadro de comando corporativo com drone de combate".

---

## 6. Armas

Todas têm 5 níveis (power-up azul). O nível aumenta o dano e o número ou ângulo dos tiros. Valores no nível 1; DPS em alvo único com todos os tiros acertando.

| Arma | Dano/tiro | Intervalo | Tiros nv.1 → nv.5 | Veloc. | DPS nv.1 | Especial | Custo |
|---|---:|---:|---|---:|---:|---|---:|
| **Laser** | 4 | 0,14 s | 1 → 5 | 18 | 29 | Feixe confiável; níveis adicionam tiros paralelos e angulados | inicial |
| **Laser Duplo** | 3 | 0,13 s | 2 → 6 | 19 | 46 | Dois feixes; mais tiros por rajada a cada nível | 1 500 |
| **Plasma** | 11 | 0,32 s | 1 → 3 | 12 | 34 | Bolas lentas e pesadas | 3 000 |
| **Spread Shot** | 3 | 0,20 s | 3 → 7 em leque | 16 | 45 | Cobertura ampla, dano baixo por bala | 2 500 |
| **Railgun** | 16 | 0,55 s | 1 → 3 | 32 | 29 | Perfura 3 inimigos | 4 500 |
| **Mísseis** | 12 | 0,55 s | 2 → 4 | 10 | 44 | Teleguiados (240°/s), área 0,9 u | 5 000 |
| **Canhão de Energia** | 10 | 0,60 s | 1 | 14 | 17 (sem carga) | Segurar 1,2 s carrega: ×4 de dano, ×2,6 de tamanho, perfura 1 | 7 000 |
| **Spore Launcher** | 3 | 0,18 s | 2 → 7 | 7,5 | 33 | Esporos teleguiados (200°/s); vem com a Symbiont | 3 500 |

Multiplicadores de dano por nível: Laser 1,0/1,15/1,3/1,45/1,6; Laser Duplo até 1,4; Plasma até 1,5; Spread até 1,4; Railgun até 1,5; Mísseis até 1,3; Canhão até 1,8; Esporos até 1,4.

---

## 7. Power-ups

Caem de inimigos (12 % dos comuns, 50 % dos elites, 100 % de mini-chefes, chefes e drones de suprimento). Efeitos temporários renovam a duração, nunca somam.

| Cor | Nome | Efeito | Duração | Peso de sorteio |
|---|---|---|---:|---:|
| Azul | Weapon Power | +1 nível de arma (até 5) | permanente na fase | 1,2 |
| Verde | Shield Restore | +50 de escudo | instantâneo | 1,4 |
| Vermelho | Damage Boost | Dano ×2 | 8 s | 1,0 |
| Amarelo | Speed Boost | Velocidade ×1,4 | 8 s | 1,0 |
| Roxo | Ultimate Energy | +40 de energia | instantâneo | 0,9 |
| Branco | Invincibility | Invencível | 5 s | 0,5 |

---

## 8. Módulos temporários (builds)

Nos pontos de "escolha de módulo" da fase (2 a 3 por fase, 1 a cada 3 chefes no Boss Rush) o jogo oferece 3 opções sorteadas pela semente da partida. Duram só a fase atual. Cada módulo empilha até 3 vezes (Magnet só 1).

| Módulo | Efeito por cópia |
|---|---|
| Overclock | +12 % cadência |
| Heavy Rounds | +15 % dano |
| Piercing Tips | tiros perfuram +1 inimigo |
| Wide Spread | +1 tiro angulado por rajada |
| Reactive Plating | +25 de escudo máximo, restaurado na hora |
| Nano Repair | abates curam 1 % do casco |
| Afterburner | +8 % velocidade |
| Risk Tuner | +20 % ganho de Overdrive |
| Magnet | itens voam até a nave |
| Lucky Core | +8 % chance de crítico |
| Focus Lens | +15 % velocidade e alcance dos projéteis |
| Energy Cells | +20 % carga da Ultimate |

---

## 9. Upgrades permanentes (Melhorias)

Dez nós, 5 níveis cada. Custo em créditos = base × (nível+1)^1,6; a partir do 3º nível também custa componentes (1, 2, 3).

| Categoria | Nó | Bônus por nível | Custo base |
|---|---|---|---:|
| ARMAS | Weapon Damage | +8 % dano | 400 |
| ARMAS | Fire Rate | +7 % cadência | 400 |
| ARMAS | Weapon Range | +10 % alcance | 300 |
| ESCUDO | Shield Capacity | +12 % escudo | 350 |
| ESCUDO | Shield Regeneration | +1 escudo/s | 450 |
| CASCO | Hull Integrity | +10 % casco | 350 |
| MOBILIDADE | Engine Speed | +5 % velocidade | 300 |
| MOBILIDADE | Thrusters | +8 % aceleração | 250 |
| ESPECIAL | Ultimate Recharge | +10 % ganho de energia | 500 |
| ESPECIAL | Ultimate Power | +15 % dano da Ultimate | 600 |

Maximizar tudo custa cerca de 63 000 créditos e 30 componentes.

---

## 10. Inimigos

Todos os valores são do nível base; cada fase aplica um multiplicador de casco e dano (1,00 na fase 1 até 1,50 na fase 10).

| Inimigo | Casco / Escudo | Contato | Pontos | Energia | Movimento | Ataque |
|---|---:|---:|---:|---:|---|---|
| **Drone** | 12 / 0 | 20 | 100 | 6 | Desce reto (2,6 u/s) | Tiro frontal a cada 2,8 s, 8 de dano |
| **Interceptor** | 18 / 0 | 22 | 100 | 8 | Zigue-zague rápido (4,0 u/s) | Rajada dupla mirada a cada 1,5 s, 8 de dano |
| **Bomber** | 60 / 0 | 30 | 100 | 12 | Desce devagar (1,4 u/s) | 3 bombas em leque a cada 2,6 s, 18 de dano, área 0,9 u |
| **Kamikaze** | 10 / 0 | 35 | 100 | 6 | Persegue a nave (5,5 u/s) | Explode no contato |
| **Shield Drone** | 20 / 30 | 22 | 100 | 10 | Para no alto e desliza de lado por 7 s | 2 tiros a cada 2 s, 10 de dano |
| **Turret** | 55 / 0 | 25 | 100 | 10 | Fixa posição | Anel de 8 tiros a cada 2,4 s, 9 de dano |
| **Raider (pirata)** | 22 / 0 | 22 | 100 | 8 | Entra pelas laterais em varredura (4,5 u/s) | Rajada dupla mirada a cada 1,6 s, 9 de dano |
| **Supply Drone** | 8 / 0 | 0 | 50 | 2 | Zigue-zague lento | Nenhum; drop garantido |
| **Asteroide** | 40 / 0 | 30 | 0 | 0 | Desce reto | Obstáculo; imune à Ultimate |

**Elites** (Drone, Interceptor, Bomber, Kamikaze, Shield Drone e o **Pirate Captain**): 2,4× casco, 2× escudo, ataques 25 % mais frequentes e 30 % mais fortes, 15 % mais rápidos, 20 % maiores, tom dourado. Valem **500 pontos**, dão **1 componente**, 50 % de drop e sobrevivem à Orbital Strike (levam 150 de dano).

Padrões de entrada: linha, V, arco, coluna, alternado, aleatório, centro, esquerda/direita, pinça (dois lados) e bordas laterais.

---

## 11. Mini-chefes

Aparecem no meio da fase e a cada 8 ondas na Sobrevivência. Valem 2 500 pontos e 40 de energia.

| Mini-chefe | Casco | Fase 1 (100 %) | Fase 2 (50 %) | Comp. |
|---|---:|---|---|---:|
| **Sentinel-X** | 900 | Patrulha lateral; anel de 10 tiros a cada 2,2 s + tiro mirado | "OVERDRIVE": anel de 14 a cada 1,6 s + leque mirado de 3 | 1 |
| **Widow** | 1 100 | Patrulha lateral; **teia** (14 de dano, deixa a nave lenta por 3 s) + 2 tiros mirados | "FRENZY": teia mais rápida, anel de 8, leque de 3 | 1 |
| **Reaper Wing** | 1 000 | **Investidas** (11 u/s) com rajadas triplas miradas | "BERSERK": investidas a 14 u/s, rajadas duplas + jato contínuo de tiros | 1 |
| **Sentinel-X Mk.II** | 1 800 | Igual ao Sentinel-X com ataques 30 % mais rápidos e projéteis 10 % mais velozes | | 2 |
| **Widow Prime** | 2 200 | Igual à Widow, 35 % mais rápida | | 2 |
| **Reaper Prime** | 2 400 | Igual ao Reaper Wing, 35 % mais rápido | | 2 |

---

## 12. Chefes

Todos valem 10 000 pontos, têm entrada cinematográfica, barra de vida com rastro de dano, números de dano acumulados e sequência de morte com múltiplas explosões. Fases são disparadas por percentual de vida; partes destrutíveis desligam ataques e valem pontos extras.

| # | Chefe | Casco / Escudo | Fases | Partes | Padrões de ataque | Comp. |
|---|---|---:|---:|---|---|---:|
| 1 | **Iron Warden** | 2 000 / 0 | 2 (100 %, 50 %) | 2 brocas (220 de vida, 800 pts): a esquerda desliga o anel, a direita desliga o lançamento de asteroides | Anel de 8 → 12 tiros; lança asteroides (até 6 → 8); tiro mirado 1 → 3 | 3 |
| 2 | **The Destroyer** | 2 400 / 0 | 3 (100, 66, 33 %) | — | Canhões laterais + anel de 12; "MISSILE BARRAGE": canhões duplos + mísseis teleguiados de área; "FINAL FURY": **laser frontal** (aviso 1,2 s, feixe 1,6 s, 45 dano/s) + mísseis + anel de 16 | 3 |
| 3 | **Bastion** | 2 600 / 0 | 2 (100, 50 %) | 3 baterias (240–260 de vida, 700 pts) que desligam canhões laterais, leque frontal e anel | Canhões laterais, leque frontal de 3 → 5, anel de 10 → 14; "SIEGE MODE" acrescenta laser frontal | 3 |
| 4 | **Leviathan** | 2 800 / 0 | 3 (100, 60, 30 %) | Corpo de 8 segmentos que seguem a cabeça | Movimento serpentino; jato contínuo em leque de 40° → 50°; anel de 10; leque mirado de 3 na fúria | 3 |
| 5 | **Corsair Queen** | 3 000 / 0 | 3 (100, 60, 30 %) | — | Investidas; **minas** (24 de dano, área 1,3 u) 3 → 5; canhões laterais; "BOARDING PARTY" invoca Pirate Captains (até 4); "NO QUARTER" jato contínuo | 3 |
| 6 | **The Assembler** | 3 400 / 150 | 2 (100, 50 %) | 2 fabricadoras (300 de vida, 900 pts): uma produz torretas, a outra drones | Invoca torretas (até 3 → 4) e drones (até 8) → kamikazes na "OVERPRODUCTION"; jato em leque; anel de 12 | 4 |
| 7 | **Aether Guardian** | 3 200 / 0 | 3 (100, 60, 30 %) | 3 cristais (160 de vida, 700 pts) que **blindam o núcleo**: só depois de quebrá-los o chefe sofre dano ("NÚCLEO EXPOSTO") | Teleporte (blink); anel de 10 → 14; leque mirado de 5; jato; "JUDGEMENT" acrescenta laser frontal | 4 |
| 8 | **Rift Walker** | 3 600 / 200 | 3 (100, 60, 30 %) | — | Teleporte; abre **fendas** que soltam kamikazes (até 6) → interceptors (8) → kamikazes elite (6); rajadas miradas; anel de 12 → 16; jato | 4 |
| 9 | **Hive Queen** | 3 400 / 200 | 3 (100, 60, 30 %) | — | Invoca drones (até 8) → kamikazes → interceptors; anel de 10 → 12 → 16; leques mirados e rajadas | 3 |
| 10 | **Omega Core** | 6 000 / 400 | 4 (100, 75, 50, 25 %) | — | 3 **transformações de forma** (1,4 a 1,8 s cada). Forma 1: anel de 12 + canhões laterais. "TRANSFORMATION": mísseis + jato. "ANNIHILATOR": laser frontal 50 dano/s + anel de 16 + kamikazes. "OMEGA OVERLOAD": canhões duplos + mísseis + laser 55 dano/s + anel de 18 | 5 |

Variante de campanha: **Destroyer Mk.II** (4 200 de casco, 15 % mais rápido) é o mini-chefe da fase 10.

---

## 13. Fases

Cada fase é uma sequência fixa de eventos com variação por semente: algumas ondas têm versões alternativas, encontros aleatórios têm 50 % a 60 % de chance, e a posição dos eventos ambientais varia. A mesma semente reproduz a mesma partida.

| # | Fase | Ambiente (música / som / cenário) | Mult. inimigos | Par | Alvo rank S | Créditos base |
|---|---|---|---:|---:|---:|---:|
| 1 | Cinturão de Ferro | Stage1 / asteroides / campo de rochas | 1,00 | 190 s | 18 000 | 300 |
| 2 | Colônia Silenciosa | Stage1 / espaço / colônia e satélites | 1,05 | 210 s | 24 000 | 360 |
| 3 | Muralha Orbital | Stage2 / espaço / muralha de fortaleza | 1,10 | 220 s | 30 000 | 420 |
| 4 | Nebulosa Viva | Stage2 / nebulosa / névoa roxa | 1,15 | 230 s | 36 000 | 480 |
| 5 | Corredor Pirata | Stage3 / asteroides / rochas e satélites | 1,20 | 240 s | 42 000 | 540 |
| 6 | Fábrica Autônoma | Stage3 / fortaleza / muralha industrial, névoa alaranjada | 1,25 | 250 s | 48 000 | 600 |
| 7 | Ruínas de Aether | Stage4 / nebulosa / ruínas e cristais | 1,30 | 260 s | 54 000 | 660 |
| 8 | Fenda Dimensional | Stage4 / colmeia / fenda roxa | 1,35 | 270 s | 60 000 | 720 |
| 9 | Núcleo Estelar | Stage5 / colmeia / núcleo da estrela | 1,40 | 280 s | 68 000 | 800 |
| 10 | Fortaleza Ômega | Stage5 / fortaleza / parede da colmeia | 1,50 | 320 s | 80 000 | 900 |

### Roteiro de cada fase

**1. Cinturão de Ferro** — Objetivos: limpar as patrulhas de drones; sobreviver à chuva de meteoros; destruir o Iron Warden.
Ondas de drones (linha, V, arco) → campo de asteroides ligado → "INTERCEPTADORES CHEGANDO" → **Sentinel-X** → módulo → interceptors + drones → **chuva de meteoros** → encontro "BANDO DE DRONES" (50 %) → onda com drone elite → **Iron Warden**.

**2. Colônia Silenciosa** — Objetivos: varrer a colônia; derrotar a Widow; destruir The Destroyer.
Drones + interceptors → bombers ou kamikazes (variante) → **tempestade solar** → interceptors + shield drones → **Widow** → módulo → encontro "SAQUEADORES!" (raiders em pinça) → bombers + kamikazes + bomber elite → tempestade solar → shield drones + interceptor elite → módulo → **The Destroyer**.

**3. Muralha Orbital** — Objetivos: silenciar a grade de torretas; abater o Reaper Wing; romper o Bastion.
Torretas + drones → raiders pelas duas bordas → torretas + interceptors ou kamikazes → **Reaper Wing** → módulo → encontro "ESQUADRÃO DE PATRULHA" → torretas + shield drones + bomber elite → tempestade solar → torretas + bombers + shield elite → módulo → **Bastion**.

**4. Nebulosa Viva** — Objetivos: atravessar a nebulosa; derrotar a Widow Prime; destruir o Leviathan.
Interceptors + drones → kamikazes + shield drones + interceptor elite → **pulso da nebulosa** → bombers/interceptors/kamikazes ou raiders em pinça → **Widow Prime** → módulo → encontro "NUVEM DE ESPOROS" (10 kamikazes + elite) → shield drones + bombers + kamikazes + shield elite → pulso da nebulosa → interceptors + drones + bomber e drone elites → módulo → **Leviathan**.

**5. Corredor Pirata** — Objetivos: percorrer o corredor; derrotar o Sentinel-X Mk.II; afundar a Corsair Queen.
Campo de asteroides → raiders pelas bordas → raiders em pinça + bombers (ou interceptors) → encontro "EMBOSCADA PIRATA!" (60 %, capitães) → **Sentinel-X Mk.II** → módulo → chuva de meteoros → kamikazes + shield drones + capitão → bombers + raiders + bomber elite → módulo → **Corsair Queen**.

**6. Fábrica Autônoma** — Objetivos: desligar as linhas de montagem; derrotar o Reaper Prime; destruir The Assembler.
Torretas + drones → torretas + interceptors + kamikazes → tempestade solar → torretas + shield drones + bomber elite (ou coluna de torretas + raiders) → **Reaper Prime** → módulo → encontro "SURTO DE PRODUÇÃO" (5 torretas + 8 kamikazes) → torretas + bombers + interceptor elites → torretas + kamikazes + shield e kamikaze elites → módulo → **The Assembler**.

**7. Ruínas de Aether** — Objetivos: cruzar as ruínas; derrotar a Widow Prime; quebrar os cristais e destruir o Aether Guardian.
Interceptors + shield drones → kamikazes + bombers + drone elites → pulso da nebulosa → raiders + shield elite + interceptors (ou torretas + kamikazes) → **Widow Prime** → módulo → encontro "CONSTRUTOS GUARDIÕES" (shield drones e elites) → bombers + interceptor elites + kamikazes → tempestade solar → shield drones + bomber elites + capitães + 10 drones → módulo → **Aether Guardian**.

**8. Fenda Dimensional** — Objetivos: segurar a linha na fenda; derrotar o Reaper Prime; destruir o Rift Walker.
Kamikazes + interceptors → bombers + raiders + kamikaze elites → tempestade solar → shield drones + interceptor elites + drones (ou torretas + capitães + kamikazes) → **Reaper Prime** → módulo → encontro "SURTO DA FENDA" (60 %, 12 kamikazes + elites) → bomber elites + interceptors + shield elites + kamikazes → pulso da nebulosa → raiders + torretas + drone elites → módulo → **Rift Walker**.

**9. Núcleo Estelar** — Objetivos: alcançar o núcleo; derrotar o Sentinel-X Mk.II; matar a Hive Queen.
12 drones + kamikazes → tempestade solar → interceptors + shield drones + drone elites (ou raiders + bombers) → bombers + torretas + 10 kamikazes + interceptor elites → **Sentinel-X Mk.II** → módulo → tempestade solar → encontro "ENXAME DA NINHADA" (60 %, 14 kamikazes + 3 elites) → shield e bomber elites + 12 drones + kamikaze elites → interceptors + torretas + kamikazes + capitães → módulo → **Hive Queen**.

**10. Fortaleza Ômega** — Objetivos: invadir a fortaleza; destruir o Destroyer Mk.II; destruir o Omega Core.
Torretas + 10 drones + kamikazes → raiders em pinça + interceptor elites + shield drones → tempestade solar → bombers + torretas + kamikazes + bomber elites (ou capitães + shield elites + 12 drones) → **Destroyer Mk.II** → módulo → encontro "GUARDA ÔMEGA" (60 %) → pulso da nebulosa → interceptors + torretas + 12 kamikazes + drone e shield elites → módulo → "O NÚCLEO ÔMEGA DESPERTA" → **Omega Core**.

### Eventos ambientais

| Evento | Aviso | Efeito |
|---|---|---|
| **Tempestade solar** | Faixa piscando por 1,4 s ("TEMPESTADE SOLAR - SAIA DA FAIXA") | Faixa horizontal de 2,2 u causa 22 de dano/s por 1,8 s; nunca nos 30 % inferiores da tela |
| **Chuva de meteoros** | 1 s ("CHUVA DE METEOROS") | 8 a 14 asteroides a 2,2× a velocidade normal |
| **Pulso da nebulosa** | Imediato ("PULSO DA NEBULOSA - BAIXA VISIBILIDADE") | Névoa roxa que sobe até 40 % de opacidade e some em 6 s |

Cada onda espera ser limpa (ou 45 s no máximo) antes da próxima.

---

## 14. Modos de jogo

| Modo | Regras | Desbloqueio |
|---|---|---|
| **Campanha** | 10 fases em ordem; cada conclusão libera a próxima. Rank, melhor pontuação e chefes derrotados ficam salvos por fase | inicial |
| **Sobrevivência** | Ondas procedurais infinitas geradas pela semente: orçamento cresce a cada onda, arquétipos mais duros entram a partir das ondas 2, 5 e 10, casco e dano dos inimigos +6 % por onda. Mini-chefe a cada 8 ondas (rotação dos 6). Drone de suprimento a cada 5 ondas. Créditos 12 por onda, teto 600 | inicial |
| **Boss Rush** | Os 10 chefes em sequência, com escolha de módulo a cada 3. Créditos 120 por chefe; +400 na primeira conclusão | após derrotar um chefe |
| **Desafio Diário** | Sobrevivência com semente = data (todos jogam a mesma sequência no dia), inimigos 15 % mais rápidos e 40 % menos drops. Uma tentativa vale por dia; guarda o melhor do dia | inicial |

---

## 15. Resultado, rank e recompensas

**Pontuação final** = pontos de abate (base × combo × risco) + raspões + partes de chefe + bônus de conclusão da fase (1 000 na fase 1 até 5 000 na fase 10) + bônus de tempo (40 pontos por segundo abaixo do par, teto 6 000) + bônus sem dano (5 000).

**Rank** por fase, derivado do alvo S de cada fase: C = 25 %, B = 45 %, A = 70 %, S = 100 %, SS = 140 %, SSS = 200 % do alvo. Fase não concluída = D.

**Créditos** (separados da pontuação, para o balanceamento do score não inflar a economia):
- Campanha: base da fase (40 % se falhou) + bônus de rank (50 a 700, D a SSS) + risco (2 por segundo em PERIGO ou EXTREMO, teto 150) + primeira conclusão (= base da fase).
- **XP**: 5 por abate, 20 por elite, 200 por chefe, 150 por conclusão, 2 por raspão. Nível de piloto n exige 100 × n² de XP.
- **Componentes**: 1 por elite, 1 a 2 por mini-chefe, 3 a 5 por chefe; usados nos últimos níveis dos upgrades.

Estimativa da curva atual: campanha completa em rank A com primeira conclusão rende cerca de 12 800 créditos, o suficiente para Falcon, Symbiont e Titan; Phantom e Nexus exigem repetir fases ou ranks altos.

---

## 16. Conquistas, ranking e save

**Conquistas** (aviso animado na tela, salvas localmente):

| Conquista | Condição |
|---|---|
| First Kill | destruir o primeiro inimigo |
| Survivor | terminar uma fase sem perder vida |
| Collector | maximizar todos os upgrades |
| Boss Hunter | derrotar todos os chefes |
| Galactic Legend | 100 %: fases, naves, armas, upgrades e as conquistas anteriores |

**Ranking local**: 10 melhores por modo, com nave, fase ou onda, data, rank e semente. Sem rede.

**Save**: arquivo JSON versão 2 no diretório persistente; migração da versão 1; tolera arquivo ausente ou corrompido; botão "Redefinir progresso" em Configurações. Guarda progresso, moedas, compras, upgrades, recordes, conquistas, opções e idioma.

---

## 17. Telas, HUD e opções

**Telas**: Boot (splash) → Menu principal (Jogar, Hangar, Melhorias, Ranking, Configurações, Créditos, Sair) → Seleção de missão → Briefing → Gameplay (com Pausa e escolha de módulo) → Vitória ou Fim de jogo → volta ao menu.

**HUD em jogo**: naves restantes, barras de casco e escudo (canto superior esquerdo); pontos, combo e fase/onda (superior direito); barra do chefe com nome, percentual e rastro de dano (topo); risco com multiplicador, barra e cor por estado, medidor de Overdrive (centro inferior); arma e nível, energia da Ultimate com botão de disparo (rodapé). Mensagens de fase no centro; números de dano, "CRÍTICO", "ELITE", "RASPÃO" flutuantes; dicas de tutorial na primeira fase; vinheta vermelha proporcional ao risco.

**Configurações**: volume geral, música e efeitos; sensibilidade do toque; tremor de tela; disparo automático; efeitos reduzidos; aviso de raspão; mostrar hitbox; idioma; redefinir progresso.

**Hangar**: lista de naves e armas com estatísticas (casco, escudo, velocidade, dano, crítico, regen, Ultimate, kit da facção), descrição, preço, comprar e equipar. **Melhorias**: os dez nós com nível atual, custo e bônus.

---

## 18. Apresentação

- **Arte**: 56 sprites finais gerados por IA (naves, projéteis, inimigos, chefes e partes, UI, cenários) mais o ícone do app, convertidos para PNG transparente em tamanhos móveis. Placeholders procedurais permanecem como fallback para qualquer nome ausente. Efeitos: partículas de tiro, faíscas, explosões em camadas, fumaça no casco crítico, rastro de motor, tremor de câmera, flash de dano, vinheta de risco.
- **Áudio**: 32 efeitos, 5 ambientes em loop e 10 trilhas instrumentais (menu, 5 estilos de fase, chefe, chefe final, sobrevivência e camada de Overdrive; todas a 120 BPM em ré menor para a camada encaixar). Mixagem por clipe e limitação de vozes para os tiros não cobrirem a música.
- **Pendente na apresentação**: fonte própria da UI (usa a padrão do TextMeshPro), conferência da emenda de cada trilha em loop, splash da Unity (licença Personal).

---

## 19. Pontos para avaliação

O que está completo e validado no aparelho:
- Toda a campanha, os quatro modos, as sete naves, as oito armas, os sistemas de risco/Overdrive/raspão, a economia e a progressão, os dois idiomas, a arte e o áudio finais.

O que ainda não foi testado com pessoas (a validação foi automatizada e por inspeção):
- **Balanceamento**: nenhum valor foi jogado por um jogador externo. Suspeitas registradas: Laser Duplo e Mísseis fortes demais pelo preço; Phantom talvez melhor que a Nova-X lendária; EMP + marcação pode ser forte demais em chefes; curva de créditos pode ser lenta para Phantom e Nexus.
- **Dificuldade da campanha**: multiplicador 1,00 → 1,50 e par de 190 s → 320 s foram estimados, não medidos.
- **Tutorial**: só a fase 1 explica os controles, no briefing e em dicas do HUD.

Lacunas de conteúdo e narrativa que valem discussão:
- A história é contada apenas por briefings e mensagens; não há introdução, cutscenes, diálogos, retratos de personagens nem epílogo além da mensagem final.
- Piratas humanos (fase 5), as ruínas de Aether (fase 7) e a origem do Enxame (fase 8) são ganchos abertos sem resolução.
- As facções jogáveis (Biomech, Cyber Corp) não têm papel na história.
- Só cinco conquistas; nenhuma ligada a risco, raspão, Overdrive ou facções.
- Não há vida extra, continue nem reviver; a fase F prevê reviver por anúncio.
- Chefes de campanha não têm variação por semente; só as ondas, encontros e eventos variam.

Próximas fases do plano: **F** (anúncios recompensados atrás de interface, Pacote Comandante sem anúncios, analytics opcional, política de privacidade; decisões SR-BIZ-001 a 003 pendentes) e **G** (loja: keystore, app bundle, listagem, teste fechado).
