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
