# Conformidade com o GDD (`document.md`)

Validação do projeto contra o Game Design Document v1.0, seção por seção. Desde a decisão D-016 o GDD é a fonte de verdade: tudo que ele descreve foi implementado, exceto a "Visão de Futuro" (versão 2.0), que exige serviços online.

Legenda: ✅ atende · 🟡 atende com placeholder ou adaptação registrada · ⏭️ item declarado da versão 2.0 no próprio GDD.

Validado por inspeção de código/dados + testes automatizados. Itens visuais e sonoros dependem do playtest de `docs/QA_CHECKLIST.md`.

## §1–3 Visão, história e loop

| Item | Situação | Evidência |
|---|---|---|
| Shmup 2D, piloto da frota terrestre vs. The Swarm | ✅ | briefings das cinco fases, créditos |
| Plataforma "PC (Windows) inicialmente" | 🟡 | mobile por pedido do usuário (D-001); build Windows continua disponível para playtest |
| Unity 6 (2D) | ✅ | 6000.3.23f1 |
| Loop: fase → abates → upgrades → mini-chefe → chefe → recompensas → melhorias permanentes → próxima fase | ✅ | `StageDirector`, `ProgressionRules`, Hangar e Upgrades |

## §4 Mecânicas

| Item | Situação | Evidência |
|---|---|---|
| 8 direções, limite de tela, movimento suave | ✅ | `PlayerMovement` |
| Laser simples | ✅ | `Weapons/Laser.asset`, 5 níveis |
| Laser Duplo | ✅ | `Weapons/DoubleLaser.asset` |
| Plasma (maior dano) | ✅ | `Weapons/Plasma.asset`, 11 de dano por tiro |
| Spread Shot (leque) | ✅ | `Weapons/SpreadShot.asset`, 3→8 projéteis |
| Railgun (perfura) | ✅ | `Weapons/Railgun.asset`, `Pierce = 3` |
| Mísseis (buscam alvo) | ✅ | `Weapons/Missiles.asset`, teleguiado + dano em área |
| Canhão de Energia (carregado) | ✅ | `Weapons/EnergyCannon.asset`, 1,2 s de carga, x4 de dano |

## §5 Sistema de vida

| Item | Situação |
|---|---|
| 3 vidas iniciais | ✅ |
| Escudo e integridade por vida | ✅ |
| Escudo absorve antes do casco | ✅ testes `Damage_HitsShieldBeforeHull` e `Damage_OverflowsFromShieldIntoHull` |

## §6 Power-ups

Os seis (azul, verde, vermelho, amarelo, roxo, branco) ✅. O roxo concede energia da Ultimate (D-007).

## §7 Sistema de especial

| Item | Situação |
|---|---|
| Barra carrega com abates | ✅ escalada por nave e upgrades |
| Só ativa cheia | ✅ teste `Ultimate_OnlyWhenFull` |
| Explosão massiva na tela | ✅ flash, onda de choque, shake e limpeza de projéteis |
| Elimina inimigos menores | ✅ elites e chefes recebem dano em vez de morrer |

## §8 Pontuação

Comum 100 · **Elite 500** · Mini-chefe 2 500 · Chefe 10 000 ✅. Multiplicador x1–x10, reset ao sofrer dano ✅.

## §9 Inimigos

| Classe | Situação | Observação |
|---|---|---|
| Drone, Interceptor, Bomber, Kamikaze, Shield | ✅ | Bomber agora tem dano em área real (raio 0,9) |
| Elite (versões melhoradas, maior recompensa) | ✅ | cinco variantes: x2,4 de vida, 500 pontos, 1 componente, sobrevivem à Ultimate |

## §10 Mini-chefes

| Item | Situação |
|---|---|
| "Cada fase possui um" | ✅ as cinco fases têm mini-chefe |
| Sentinel-X (robô gigante, ataque giratório) | ✅ |
| Widow (aranha, teias energéticas) | ✅ a teia deixa a nave lenta |
| Reaper Wing (caça de elite agressivo) | ✅ investidas rápidas |

Fases 4 e 5 usam variantes reforçadas (Sentinel-X Mk.II, Widow Prime).

## §11 Chefes

| Item | Situação |
|---|---|
| "Cada fase termina em um" | ✅ |
| The Destroyer (canhões laterais, mísseis, laser frontal) | ✅ 3 fases |
| Leviathan (forma serpentina, ataques contínuos) | ✅ corpo de 8 segmentos que seguem a cabeça |
| Hive Queen (invoca enxames) | ✅ invoca drones, kamikazes e interceptores com limite |
| Omega Core (IA central, transformações múltiplas) | ✅ 4 fases com 3 trocas de forma |

## §12 Fases

| Fase | Situação |
|---|---|
| 1 Setor Orbital (Terra, satélites) | ✅ planeta ao fundo e destroços |
| 2 Campo de Asteroides | ✅ asteroides móveis destrutíveis |
| 3 Nebulosa Violeta (baixa visibilidade) | ✅ névoa roxa abaixo dos projéteis |
| 4 Fortaleza Mecânica (muitos canhões) | ✅ turrets fixos e muralha ao fundo |
| 5 Núcleo da Colmeia (chefe final) | ✅ colmeia ao fundo, Omega Core |

O GDD coloca o Destroyer na fase 1; ele encerra a fase 1 e volta como Mk.II na fase 4.

## §13 Progressão

Créditos, experiência e componentes após cada fase ✅. Nível de piloto no menu. Componentes só de elites e chefes.

## §14 Árvore de upgrades

Dez nós em cinco níveis, cobrindo armas (dano, cadência, alcance), escudo (capacidade, regeneração), casco (vida máxima), mobilidade (velocidade, aceleração) e especial (recarga, potência) ✅.

## §15 Naves jogáveis

Vanguard (balanceada), Falcon (rápida, menos vida), Titan (resistente e lenta), Phantom (crítico) e Nova-X (lendária) ✅. A Nova-X desbloqueia ao concluir a campanha (D-018).

## §16 Telas

| Tela | Situação |
|---|---|
| Splash (logo do estúdio + Unity) | ✅ cartão do estúdio e título; a Unity mostra o próprio logo |
| Menu: Jogar, Hangar, Melhorias, Ranking, Configurações, Créditos, Sair | ✅ |
| Seleção de nave "3D ou 2D animada" | ✅ prévia em holograma no Hangar (shader próprio) |
| Briefing (resumo + objetivos) | ✅ mostra também a nave e a arma equipadas |
| Gameplay com HUD completa | ✅ |
| Vitória (resultados) | ✅ inclui recompensas e posição no ranking |
| Derrota | ✅ |
| Créditos (equipe e ferramentas) | ✅ |

## §17 HUD

As cinco posições ✅. A "munição especial" mostra o efeito temporário ativo e o tempo restante (D-019); a carga do Canhão de Energia tem barra própria.

## §18 Animações

| Item | Situação |
|---|---|
| Nave: impulso dos motores, inclinação lateral | ✅ |
| Nave: dano crítico | ✅ fumaça, aviso piscando no HUD e alarme abaixo de 30 % de casco |
| Inimigos: propulsores, explosões, efeitos de escudo | ✅ todos os inimigos têm propulsor com tremulação |
| Chefes: entrada cinematográfica, transformações, destruição final | ✅ o Omega Core troca de forma três vezes |
| Interface: botões animados, barras preenchendo, som ao passar o mouse | ✅ |

## §19 Efeitos visuais

| Item | Situação |
|---|---|
| Partículas: tiros, faíscas, explosões, energia | ✅ `ParticleBurst` com pooling |
| Partículas: fumaça | ✅ estado crítico da nave |
| Shaders: escudos, raios, hologramas | ✅ `ShieldSprite`, `EnergyBeam`, `HologramSprite`, mais `AdditiveSprite` para o neon (D-021) |

## §20 Efeitos sonoros

Armas (laser, plasma, railgun, míssil, canhão), ambiente (motores, alarmes, nebulosa) e interface (seleção, confirmação, erro) ✅, todos como placeholders sintetizados em tempo real. Substituir é preencher `AudioLibrary.asset`.

## §21 Trilha sonora

Uma trilha por fase com o estilo pedido (pad orquestral, arpejo eletrônico, synthwave, industrial, épica) e trilhas exclusivas de chefe e chefe final 🟡 — sintetizadas, aguardando assets finais.

## §22 Conquistas

Primeiro Abate, Sobrevivente, Colecionador, Caçador de Chefes e Lenda Galáctica ✅, locais (D-020), com aviso na tela e lista no Ranking.

## §23 Modos de jogo

Campanha ✅ · Sobrevivência (ondas infinitas) ✅ · Boss Rush (só chefes) ✅ · Desafio Diário (semente do dia) ✅.

## §24 Estilo artístico

Paleta neon (azul elétrico, roxo, vermelho, branco) e blending aditivo ✅. Os sprites continuam placeholders procedurais.

## §25 Escopo de MVP

Os 12 itens do checklist ✅ (áudio como placeholder sintetizado).

## Visão de Futuro (versão 2.0) — fora do escopo

Coop online, ranking global, 20 fases, clãs, eventos semanais, Battle Pass, editor de fases, Steam Achievements e Steam Cloud Save. Todos no `docs/BACKLOG.md`. O ranking existe em versão local (D-017) e as conquistas em versão local (D-020).

## Resumo

Todas as seções §1–§25 do GDD estão atendidas. As adaptações registradas são: plataforma mobile (D-001), power-up roxo como energia (D-007), ranking e conquistas locais (D-017, D-020), critério de desbloqueio da Nova-X (D-018), campo de "munição especial" do HUD (D-019) e shaders próprios sem pós-processamento (D-021). O que permanece pendente é substituir os placeholders de arte e áudio por assets finais.
