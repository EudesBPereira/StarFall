# Arquitetura

## Visão geral

```
Boot ──► MainMenu ──► Gameplay (fase N) ──► Victory ──► Gameplay (fase N+1) …
                          │                    └──► MainMenu
                          └──► GameOver ──► Gameplay (mesma fase) | MainMenu
```

Três cenas. A cena `Gameplay` é única e carrega a fase indicada por `GameSession.CurrentStageIndex` (dado volátil, não persistido). Toda a configuração de fase é um `StageDefinition`.

## Módulos (Assets/_Project/Scripts)

| Pasta | Responsabilidade | Principais tipos |
|---|---|---|
| `Logic/` | Regras puras, sem UnityEngine, testáveis com `dotnet test` | `HealthModel`, `ScoreModel`, `LivesModel`, `EnergyModel`, `StatusEffectTracker`, `SaveRepository`, `StageProgression`, `DamageInfo/DamageResult` |
| `Core/` | Fluxo, contexto de cena, área de jogo, eventos | `GameFlowController`, `GameplayContext`, `GameSignals`, `PlayArea`, `GameConfig`, `GameSession`, `SceneLoader`, `BootLoader` |
| `Input/` | Abstração de entrada (teclado, controle, toque, botões UI) | `IGameInput`, `GameInputReader`, `TouchPad` |
| `Player/` | Nave, movimento, arma, efeitos, Ultimate | `PlayerShip`, `PlayerMovement`, `WeaponController`, `PlayerStatusEffects`, `UltimateController`, `ShipDefinition` |
| `Combat/` | Contrato de dano, projéteis, camadas | `IDamageable`, `Health`, `Projectile`, `ProjectileSpec`, `ProjectileLauncher`, `WeaponDefinition`, `GameLayers`, `SortingOrders` |
| `Enemies/` | Inimigos genéricos orientados a dados + estratégias | `Enemy`, `EnemyDefinition`, `IMovementStrategy` (5 impl.), `IAttackStrategy` (`PatternAttack`), `EnemyRegistry`, `EnemySpawner` |
| `Bosses/` | Fases e padrões de chefe | `BossController`, `BossDefinition`, `LaserBeam` |
| `Waves/` | Dados de ondas/fases e execução | `WaveDefinition`, `StageDefinition`, `StageDirector`, `SpawnPositionResolver` |
| `Scoring/` | Pontuação por eventos | `ScoreService` |
| `PowerUps/` | Itens, tabela de drop, pickup | `PowerUpDefinition`, `DropTable`, `PowerUpPickup` |
| `UI/` | Apresentação (sem regras) | `HudView`, `HudPresenter`, `UiPanel` e painéis, `SettingsPanel`, `MainMenuController`, `SafeAreaFitter`, `UiButtonSfx` |
| `Audio/` | Música/SFX com placeholders sintetizados | `AudioManager`, `AudioLibrary`, `PlaceholderAudioSynth`, `SfxId`, `MusicId` |
| `Save/` | Persistência local em JSON | `SaveService`, `FileSaveStorage`, `JsonUtilitySaveSerializer` |
| `Pooling/` | Pool por prefab | `PoolService`, `PooledObject`, `IPoolable` |
| `VFX/` | Feedback visual por código | `VfxSpawner`, `ExplosionEffect`, `FloatingText`, `CameraShake`, `ThrusterFlicker`, `StageBackground` |
| `Editor/` | Geração do projeto e builds | `ProjectBootstrap`, `SceneBuilder`, `UiBuilder`, `PlaceholderArt`, `BuildScript` |

Assemblies: `Starfall.Runtime`, `Starfall.Editor`, `Starfall.Tests.EditMode`, `Starfall.Tests.PlayMode`.

## Fluxo de eventos (`GameSignals`)

```
Projectile/Enemy contato ─► Health.ApplyDamage ─► HealthModel
    ├─ (player) PlayerShip.OnDamaged ─► GameSignals.PlayerDamaged ─► ScoreService (reset x1), AudioManager
    └─ (enemy)  Enemy.OnDied ─► GameSignals.EnemyDestroyed ─► ScoreService (+pontos), UltimateController (+energia),
                                                                AudioManager (sfx), HudPresenter (via ScoreModel)
StageDirector fim dos eventos ─► GameSignals.StageCompleted ─► GameFlowController (Victory)
PlayerShip morte ─► GameSignals.PlayerDied ─► GameFlowController (respawn | GameOver)
BossController ─► BossSpawned / BossDefeated ─► HudPresenter (barra), AudioManager (música)
PowerUpPickup ─► PlayerShip.CollectPowerUp ─► GameSignals.PowerUpCollected ─► HUD
```

Os modelos (`ScoreModel`, `HealthModel`, `EnergyModel`, `LivesModel`) expõem eventos C# próprios; o `HudPresenter` assina os modelos e alguns sinais. Nada na UI altera regras.

## Estado do gameplay (`GameFlowController`)

`Briefing (timeScale 0) → Playing ⇄ Paused → PlayerDown → Playing | GameOver`, `Playing/PlayerDown → Victory`.
Pausa usa `Time.timeScale = 0`; toda animação de gameplay usa tempo escalado, e a UI usa tempo real. Ao destruir a cena, o timeScale volta a 1.

## Dados

- `GameConfig` (global) → `StageDefinition[]` → `StageEvent[]` (Wave / Delay / Message / MiniBoss / Boss / AsteroidField) → `WaveDefinition` → `SpawnEntry[]` (`EnemyDefinition`, quantidade, intervalo, padrão de spawn).
- `EnemyDefinition` escolhe `MovementKind` + `AttackKind` e seus parâmetros; um único prefab genérico serve para os cinco inimigos e o asteroide. `BossDefinition` herda e adiciona entrada e fases (`BossPhase` → `BossAttack[]`).
- `PowerUpDefinition` + `DropTable`; `ShipDefinition` + `WeaponDefinition`; `AudioLibrary`.

## Dependências e serviços

- `GameplayContext.Current`: localizador com escopo de cena com referências atribuídas no Inspector (câmera, pools, registro de inimigos, spawner, jogador, entrada, pontuação, diretor, VFX). Objetos pooled o consultam **uma vez** ao serem inicializados, nunca em `Update`.
- `AudioManager.Instance`: único `DontDestroyOnLoad`. Criado sob demanda por `AudioManager.Ensure` no Boot, no menu e no gameplay (permite abrir qualquer cena diretamente no Editor).
- `SaveService`: fachada estática; a lógica real (`SaveRepository`) recebe `ISaveStorage` e `ISaveSerializer` injetados.
- Sem `FindObjectOfType` em gameplay; sem alocação em `Update` além de buffers reutilizados.

## Física e camadas

| Camada | Índice | Colide com |
|---|---|---|
| Player | 6 | EnemyProjectile, Enemy, Obstacle, PowerUp |
| PlayerProjectile | 7 | Enemy, Obstacle |
| Enemy | 8 | Player, PlayerProjectile |
| EnemyProjectile | 9 | Player |
| PowerUp | 10 | Player |
| Obstacle | 11 | Player, PlayerProjectile |

Ordem de sorting (`SortingOrders`): fundo −100 · estrelas −90 · destroços −80 · inimigos 0 · escudo inimigo 2 · névoa 5 · jogador 10 · power-ups 15 · projéteis 20 · laser 25 · VFX 30 · texto 40. A névoa da fase 3 fica **abaixo** dos projéteis para preservar a leitura.

## Motivos das principais escolhas

- **Estratégias por enum + parâmetros** em vez de subclasses por inimigo: novos inimigos são assets; código compartilhado só para o que é realmente comum (`Enemy`).
- **Modelos puros**: as regras de aceite (escudo antes do casco, morte única, multiplicador x10, Ultimate só cheia, expiração de efeitos, save seguro) são testadas sem o Editor.
- **Geração por script**: elimina YAML manual e mantém o repositório reproduzível.
- **Bus estático**: simplicidade e desacoplamento; a alternativa (ScriptableObject event channels) foi considerada excessiva para o MVP.

---

## Versão 1.0 completa — módulos acrescentados

### Progressão (lógica pura, `Scripts/Logic`)

- `ProgressionRules` — recompensas por partida, custos e regras de desbloqueio de naves e armas, nível de piloto, conclusão de campanha.
- `UpgradeTree` (`UpgradeCatalog` + `LoadoutModifiers`) — dez nós, custo por nível e os multiplicadores resultantes.
- `AchievementRules` — avalia as cinco conquistas contra o save e a partida atual, sem repetir concessões.
- `ProceduralWaves` — gerador determinístico de ondas por semente e índice, usado por Sobrevivência e Desafio Diário.
- `SaveData` v2 — economia, upgrades, máscaras de naves/armas/conquistas/chefes, placar local e preferências, com migração automática do v1.

Nenhuma dessas classes referencia `UnityEngine`; todas têm teste em `Tests/EditMode` e rodam também no `dotnet test`.

### Ponte entre save e gameplay

`PlayerLoadout` combina `ShipDefinition` + `WeaponDefinition` + `LoadoutModifiers` em uma struct só, montada uma vez quando a nave entra em cena. Movimento, arma, vida e Ultimate leem essa struct, então nenhum sistema de gameplay lê o save diretamente.

### Fluxo por modo

`GameSession` guarda o modo, a semente e as estatísticas da partida (`RunStats`). `StageDirector` escolhe a rotina conforme o modo: eventos da fase (campanha), ondas procedurais infinitas (Sobrevivência/Diário) ou a fila de chefes (Boss Rush). `RunTracker` alimenta as estatísticas a partir dos sinais, e `GameFlowController` fecha a partida: aplica recompensas, grava progresso e placar, e chama `AchievementService`.

### Chefes

`BossController` ganhou fases com transformação (troca de sprite e invulnerabilidade temporária), corpo em segmentos que seguem um rastro (Leviathan), invocação com limite (Hive Queen) e novos padrões (Stream, Web). Cada padrão é um `AttackRunner` com timer próprio, então um chefe roda vários ao mesmo tempo.

### Apresentação

- Quatro shaders próprios: holograma (Hangar), escudo, aditivo (neon) e feixe de energia.
- `ParticleBurst` (pooled) para faíscas; fumaça contínua na nave em estado crítico.
- `EngineAudio` liga o som do motor à velocidade; `AudioManager` ganhou uma trilha de ambiente por fase.
- Telas novas: `MissionPanel`, `HangarPanel`, `UpgradesPanel`, `RankingPanel` e `AchievementToast`, todas construídas por `SceneBuilder` a partir do helper `UiBuilder.CreateListRow`.
- `ContentFactory` foi separado de `ProjectBootstrap`: é o único lugar que define os números iniciais de todo o conteúdo.

---

## Fase B do Plano Mestre — núcleo STAR RISK

### Lógica pura (`Scripts/Logic`)

- `RiskModel` — suaviza a leitura bruta de ameaça em um estado estável (Seguro, Alerta, Perigo, Extremo) com histerese e retenção mínima; expõe o multiplicador (com escala de Overdrive e teto x8) e a fórmula de contribuição por distância.
- `OverdriveModel` — medidor que enche por risco, graze e abates, drena em segurança, sofre penalidade por dano, ativa sozinho ao encher e drena durante os 8 s ativos.
- `GrazeRules` — raio do anel, regras de elegibilidade (uma vez por projétil, nunca invulnerável, velocidade mínima) e pontos.
- `StageResultRules` — composição do score (abates × combo × risco + graze + objetivo + tempo + sem dano) e ranks D–SSS por limiares derivados do alvo da fase.
- `ProgressionRules` — recompensa por base de fase, rank, risco limitado e primeira conclusão; `FactionId`.
- `ScoreModel` — `RegisterKill(base, risco)` e `RegisterGraze(risco)`.

### Runtime

- `RiskSensor` (na nave) é o único ponto que conhece a física: a cada 0,1 s faz um `OverlapCircleNonAlloc`, soma contribuições ponderadas (`EnemyDefinition.RiskWeight`), alimenta `RiskModel` e `OverdriveModel`, detecta grazes com lista de pendentes (confirmados só quando o projétil sai do anel sem acertar) e emite `GameSignals.RiskStateChanged`, `OverdriveChanged`, `OverdriveMeter` e `Graze`.
- `PlayerShip.ApplyOverdriveEffects` aplica a reação da facção: cadência (Federação), crítico (Cyber) ou regeneração (Biomecânica), além de carga do Ultimate e propulsor pulsante.
- `ScoreService` lê o multiplicador do sensor a cada abate e registra grazes; `RunTracker` acumula segundos em perigo, grazes, ativações e maior risco.
- `GameFlowController.ComposeResult` fecha a fase com `StageResultRules`, aplica os bônus ao score, calcula o rank e chama `ProgressionRules.ComputeRewards` com a base de créditos da fase.
- `Projectile` guarda `Grazed` e `HitPlayer` para a regra de um graze por projétil.

### Apresentação

- `RiskVignette` — anel aditivo nas bordas da tela tingido pela paleta do plano; pulsa em ciano/magenta no Overdrive; respeita "Reduced effects".
- `HudView.SetRisk/SetOverdrive/SetGrazes` — rótulo com "!" por estado (informação não depende só de cor), barras de risco e Overdrive, contador de grazes.
- `HudPresenter` — dicas contextuais únicas na fase 1 (Alerta, Perigo, Extremo, Overdrive, Graze).
- `AudioManager` — fonte extra `OverdriveLayer` com fade; sons `RiskUp/RiskDown/Graze/OverdriveStart/OverdriveEnd/RankReveal`.
- `SettingsPanel` — screen shake, efeitos reduzidos, feedback de graze e hitbox visível (`CameraShake` e `RiskSensor` leem o save).

---

## Fase C — Facções

- `Logic/FactionRules` — `UltimateKind`, `PrecisionModel` (sequência de acertos), constantes de marcação, EMP, lifesteal e enxame; mapeamento nave → facção.
- `ShipDefinition` — `Faction`, `Ultimate`, `MarksTargets`, `HasCompanionDrone`, `LifestealPerKill`, `PrecisionBonusPerHit`, bônus de Overdrive por facção.
- `WeaponController` — aplica a precisão da Federação (o `Projectile` chama `ReportShot` ao despawnar) e carimba `MarkSeconds` nos projéteis de naves Cyber.
- `Health.IncomingDamageMultiplier` + `Enemy.ApplyMark/Stun` — estado de marcação e atordoamento com feedback de cor; `BossController` respeita o stun (metade do tempo) e esconde o laser.
- `CompanionDrone` — filho do prefab do jogador; orbita e dispara sozinho no inimigo mais próximo com projéteis marcadores.
- `UltimateController` — três execuções: Orbital Strike, Spore Swarm (esporos buscadores) e EMP Burst (stun + marca + limpeza).
- `PlayerShip` — lifesteal por sinal de abate; liga o drone conforme a nave.
- Conteúdo: `ContentFactory` cria Symbiont, Nexus e Spore Launcher; `PlaceholderArt` gera as silhuetas orgânica e hexagonal, o drone e o esporo.

---

## Fase E — Conteúdo (dez fases, dez chefes, builds temporárias)

- `Logic/StageVariation` — hash estável (seed, fase, evento) para escolhas reproduzíveis: variante de onda, encontro aleatório, posição de hazard.
- `Logic/BuildMods` + `Player/BuildState` — catálogo de doze módulos temporários, sorteio com semente sem repetição e limite de pilhas, e o estado por fase com os multiplicadores agregados.
- `Waves/StageDefinition` — eventos `Hazard`, `BuildChoice` e `RandomEncounter`; `Variants` por evento; `EnemyStatMultiplier`, nomes de chefe/mini-chefe.
- `Waves/HazardController` — solar flare (faixa telegrafada), chuva de meteoros e pulso de nebulosa; o diretor aguarda o hazard terminar.
- `Waves/StageDirector` — escolhe variantes por semente, rola encontros, executa hazards, pede o sorteio de build ao fluxo (`BuildDraftRequest`) e aplica o multiplicador de atributos da fase. Modos infinitos sorteiam a cada cinco ondas; Boss Rush a cada três chefes.
- `Bosses/BossPart` + `BossDefinition.Parts` — partes destrutíveis com vida própria que desligam ataques ou blindam o núcleo; `BossController` recompõe a invulnerabilidade (entrada, transformação, blindagem) e ignora runners desligados. Novos padrões `Mines` (projétil que para e detona) e `RiftSpawn` (fendas telegrafadas), movimentos `SideSweep` e `Blink`, formações `LeftEdge/RightEdge/Pincer/TopColumn/TopArc`.
- `UI/BuildDraftPanel` + `GameState.BuildChoice` — congela o jogo e oferece três módulos; `PlayerShip.ApplyBuild` empurra os multiplicadores para arma, Ultimate, sensor de risco, escudo e ímã de itens.
- `Tests/EditMode/UnityOnly/ContentValidationTests` — validação estática do conteúdo gerado (dez fases, dez chefes, limites de velocidade/intervalo/telegraph). Fica fora do `dotnet test` porque usa `AssetDatabase`.
