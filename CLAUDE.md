# CLAUDE.md — Starfall Defense

Guia para agentes (Claude Code) trabalhando neste repositório. Leia também `blueprint.md` (especificação original) e `docs/`.

## O que é

Shoot'em up 2D vertical para **Android/iOS** em **Unity 6000.3.23f1** (Unity 6.3 LTS), orientação portrait.

Direção: `docs/product/IMPLEMENTATION_MASTER_PLAN.md` (Plano Mestre STAR RISK, D-023) — núcleo = Zona de Risco, Overdrive, graze, facções; fases A–G; nada de monetização/serviços fora da fase F. Escopo atual (fases B, C e E do plano): 7 naves em 3 facções, 8 armas, inimigos comuns + elites + turret + raider + asteroide, 7 mini-chefes/variantes, 10 chefes principais (Destroyer, Leviathan, Hive Queen, Omega Core, Iron Warden, Bastion, Corsair Queen, Assembler, Aether Guardian, Rift Walker), 10 fases com hazards, encontros aleatórios por semente e builds temporárias, vidas/escudo/casco com crítico, multiplicador x1–x10, 6 power-ups, Ultimate, progressão permanente (créditos/XP/componentes, árvore de 10 upgrades), Hangar, Ranking local, conquistas, modos Sobrevivência/Boss Rush/Desafio Diário, áudio placeholder e save local v2.

**A fonte de verdade é `document.md` (GDD)** desde D-016; o `blueprint.md` foi só o recorte inicial de MVP. Plataforma mobile foi pedida pelo usuário e sobrepõe o "Windows" do GDD (D-001/D-002). Fora do escopo: a "Visão de Futuro" do GDD (coop, ranking global, clãs, Battle Pass, editor de fases, Steam).

## Estrutura essencial

```
Assets/_Project/Scripts/
  Logic/    regras puras (sem UnityEngine) — testadas por dotnet e EditMode; inclui RiskModel, OverdriveModel, GrazeRules, StageResultRules, StageVariation, BuildMods, FactionRules
  Core/     GameFlowController, GameplayContext, GameSignals, PlayArea, GameConfig, GameSession
  Input/    IGameInput, GameInputReader (teclado+gamepad+toque), TouchPad
  Player/   PlayerShip, PlayerMovement, WeaponController, PlayerStatusEffects, UltimateController, RiskSensor (risco+overdrive+graze)
  Combat/   IDamageable, Health, Projectile, ProjectileSpec/Launcher, WeaponDefinition, GameLayers
  Enemies/  Enemy (genérico, orientado a dados), EnemyDefinition, estratégias de movimento/ataque, registry, spawner
  Bosses/   BossController (fases, partes destrutíveis, minas, fendas), BossDefinition, BossPart, LaserBeam
  Waves/    WaveDefinition, StageDefinition (eventos: Wave, Hazard, BuildChoice, RandomEncounter...), StageDirector, HazardController, SpawnPositionResolver
  Scoring/ PowerUps/ UI/ Audio/ Save/ Pooling/ VFX/
  Editor/   ProjectBootstrap (settings+prefabs), ContentFactory (todos os dados), SceneBuilder, UiBuilder, PlaceholderArt, BuildScript
Assets/_Project/{ScriptableObjects,Prefabs,Scenes,Art/Placeholders}  gerados pelo bootstrap
Assets/_Project/Tests/{EditMode,EditMode/UnityOnly (usa AssetDatabase, fora do dotnet),PlayMode}
Tools/LogicTests/   projeto `dotnet test` que compila Scripts/Logic + Tests/EditMode
docs/               ASSET_SPEC (arte/áudio finais: nomes, pastas, formatos), IMAGE_PROMPTS e AUDIO_PROMPTS (prompts p/ gerar cada imagem/áudio), RELEASE_STATUS (o que existe e o que falta p/ as lojas), ARCHITECTURE,
                    DECISIONS, BALANCING, QA_CHECKLIST, BACKLOG, GDD_COMPLIANCE
```

Cenas, prefabs, sprites placeholder e ScriptableObjects são **gerados por script** (`Starfall → Run Full Bootstrap`). Arte e áudio finais entram por `Assets/_Project/Art/Final` e `Audio/Final` com os nomes de `docs/ASSET_SPEC.md` (`FinalAssets` substitui o placeholder de mesmo nome no bootstrap). Não escreva YAML de cena/prefab à mão. Re-executar `GenerateAll` sobrescreve valores de balanceamento dos assets gerados (D-006): mude os valores em `ContentFactory.CreateData` ou não re-execute.

## Comandos

Editor local (esta máquina): `C:\Users\eudes\Unity\Editor\6000.3.23f1\Editor\Unity.exe`. Apenas uma instância pode abrir o projeto por vez.

```bat
:: preparar (settings, TMP) e gerar (arte, dados, prefabs, cenas)
Unity.exe -batchmode -nographics -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.PrepareProject -quit -logFile prepare.log
Unity.exe -batchmode -nographics -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.GenerateAll -quit -logFile generate.log
:: testes
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults editmode-results.xml -logFile editmode.log
Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform PlayMode -testResults playmode-results.xml -logFile playmode.log
:: build Android
Unity.exe -batchmode -nographics -projectPath . -buildTarget Android -executeMethod Starfall.EditorTools.BuildScript.BuildAndroid -quit -logFile build-android.log
```

Lógica pura sem o Editor: `cd Tools/LogicTests && dotnet test` (.NET 10 em `C:\Program Files\dotnet`).

Erros de compilação aparecem no log como `error CS`; `grep -n "error CS" <log>`.

## Regras de trabalho (do blueprint)

- Trabalhe em etapas pequenas e verificáveis; nada da "Visão de Futuro" (2.0) do GDD.
- Nunca afirme que rodou o Editor/testes/builds sem ter rodado; separe "implementado", "validado por inspeção" e "pendente no Editor".
- Sem assets de terceiros: só placeholders procedurais (sprites SDF em `PlaceholderArt`, áudio sintetizado em `PlaceholderAudioSynth`).
- Sem `FindObjectOfType`/`GameObject.Find` em gameplay; sem alocação em `Update`; pooling para projéteis, inimigos, explosões, itens.
- Nada de lógica concentrada num GameManager; composição, eventos (`GameSignals`), interfaces (`IDamageable`, `IGameInput`, estratégias de inimigo).
- Valores de balanceamento em ScriptableObjects, documentados em `docs/BALANCING.md`.
- Requisito ambíguo: solução mais simples + registro em `docs/DECISIONS.md`.
- Mantenha `README.md` e `docs/*.md` sincronizados com o código a cada ciclo; registre débitos em `docs/BACKLOG.md`.

## Convenções de código

- Um MonoBehaviour por arquivo, com o nome do arquivo igual ao da classe (D-022); structs e enums podem compartilhar arquivo.
- C# com `namespace Starfall.<Módulo>`; campos serializados `[SerializeField] internal` (o assembly Editor e os testes têm `InternalsVisibleTo`).
- Assinantes de `GameSignals` e dos modelos cancelam a inscrição em `OnDisable`/`OnDestroy`.
- Pausa usa `Time.timeScale = 0`; gameplay em tempo escalado, UI em tempo real.
- Camadas físicas fixas em `GameLayers`; ordem de sprites em `SortingOrders`.
- Regras novas vão para `Scripts/Logic` (puras) com teste em `Tests/EditMode`.

## Ambiente desta máquina (não versionado)

Antivírus (Norton) intercepta TLS: downloads grandes falham; Input System é referenciado por tarball local (`Packages/*.tgz`, D-013); a raiz do Norton foi importada no `cacerts` do OpenJDK embutido (D-015). Módulo Android instalado manualmente em `Editor/Data/PlaybackEngines/AndroidPlayer/{SDK,NDK,OpenJDK}`.

## Git

Remoto: `https://github.com/EudesBPereira/StarFall.git` (branch `main`). Não versionar `Library/`, `Builds/`, logs, resultados de teste.
