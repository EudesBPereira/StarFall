# STARFALL DEFENSE — 1.0

Shoot'em up 2D vertical para **Android e iOS**, feito em **Unity 6**. No ano de 2237 a humanidade enfrenta The Swarm; você pilota uma das cinco naves da frota terrestre por cinco setores até o Omega Core, a inteligência por trás da invasão.

Campanha de 5 fases com 4 chefes e 5 mini-chefes, 7 armas, 5 naves, árvore de upgrades permanente, ranking local, conquistas e os modos Sobrevivência, Boss Rush e Desafio Diário.

Documentação: [Estado do projeto e caminho até as lojas](docs/RELEASE_STATUS.md) · [Arquitetura](docs/ARCHITECTURE.md) · [Decisões](docs/DECISIONS.md) · [Balanceamento](docs/BALANCING.md) · [Checklist de QA](docs/QA_CHECKLIST.md) · [Backlog](docs/BACKLOG.md) · [Blueprint original](blueprint.md)

## Requisitos

- **Unity 6000.3.23f1** (6.3 LTS) com módulo *Android Build Support* (SDK/NDK/OpenJDK) para APK; módulo *iOS Build Support* + macOS/Xcode para iPhone.
- Pacotes (resolvidos por `Packages/manifest.json`): Input System 1.14, uGUI 2.0 (TextMeshPro), Test Framework.
- Opcional: .NET SDK 10 para rodar os testes de lógica fora do Unity (`Tools/LogicTests`).

## Como abrir e gerar o projeto

O repositório versiona **código, dados de geração e documentação**. Cenas, prefabs, sprites placeholder e ScriptableObjects são criados por script:

1. Abra a pasta no Unity Hub (ou `Unity.exe -projectPath <pasta>`).
2. Aguarde a importação dos pacotes.
3. Menu **Starfall → Run Full Bootstrap** (ou passo 1 e depois passo 2). Isso:
   - configura camadas, matriz de colisão 2D, Input System e Player Settings (portrait, IL2CPP/ARM64, bundle id);
   - importa os recursos essenciais do TextMeshPro;
   - gera sprites placeholder em `Assets/_Project/Art/Placeholders`;
   - cria prefabs, ScriptableObjects (nave, arma, inimigos, chefes, power-ups, ondas, fases) e as cenas `Boot`, `MainMenu`, `Gameplay`;
   - registra as cenas no Build Settings.
4. Abra `Assets/_Project/Scenes/Boot.unity` e aperte Play.

Em linha de comando:

```bat
Unity.exe -batchmode -nographics -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.PrepareProject -quit -logFile prepare.log
Unity.exe -batchmode -nographics -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.GenerateAll -quit -logFile generate.log
```

> Re-executar o gerador sobrescreve os valores de balanceamento nos assets gerados (veja D-006 em `docs/DECISIONS.md`).

## Como executar

- **Editor:** cena `Boot` (fluxo completo) ou `Gameplay` diretamente (usa a fase `GameSession.CurrentStageIndex`, padrão 0). Use a Game view em 1080x1920 para simular um celular.
- **Testes:** Window → General → Test Runner → EditMode (regras) e PlayMode. Fora do Unity: `cd Tools/LogicTests && dotnet test`.
- **Android:** Starfall → Build → Android APK (ou `-executeMethod Starfall.EditorTools.BuildScript.BuildAndroid`). Saída em `Builds/Android/StarfallDefense.apk`.
- **iOS:** Starfall → Build → iOS Xcode Project (apenas no macOS), depois compile no Xcode.
- **Windows (playtest):** Starfall → Build → Windows.

## Controles

| Ação | Toque | Teclado | Controle |
|---|---|---|---|
| Mover | Arrastar em qualquer lugar da tela | WASD / setas | Analógico esquerdo / d-pad |
| Disparar | Automático (configurável) | Espaço (ou automático) | Botão sul / RT |
| Ultimate | Tocar na barra de energia | E | Botão oeste / LT |
| Pausar | Botão "II" no topo | Esc | Start |
| Confirmar / iniciar fase | Toque | Espaço / Enter | Botão sul / Start |

Sensibilidade do toque, auto-fire e volumes ficam em Configurações e são persistidos.

## Estrutura geral

```
Assets/_Project/
  Art/Placeholders/      sprites brancos gerados (tintados em runtime)
  Prefabs/               Player, Enemies, Bosses, Projectiles, PowerUps, VFX
  Scenes/                Boot, MainMenu, Gameplay
  ScriptableObjects/     Config, Ships, Weapons, Enemies, Bosses, PowerUps, Waves, Stages, Audio
  Scripts/               Core, Logic, Input, Player, Combat, Enemies, Bosses, Waves, Scoring,
                         PowerUps, UI, Audio, Save, Pooling, VFX, Editor
  Tests/EditMode, Tests/PlayMode
Tools/LogicTests/        projeto dotnet que compila Scripts/Logic + Tests/EditMode
docs/                    documentação obrigatória
```

## Como criar conteúdo

**Novo inimigo:** `Create → Starfall → Enemies → Enemy Definition`. Escolha sprite/cor, vida, escudo, dano de contato, `Movement` (StraightDown, Weave, Chase, HoverStrafe, LateralPatrol, Serpentine, Dash, Hold) e `Attack` (None, Forward, Aimed, Ring, Stream, Web) com seus parâmetros, pontos, energia, `ComponentReward`, `IsElite` e `DropTable`. Não é preciso criar prefab: o spawner usa o prefab genérico `Prefabs/Enemies/Enemy` (ou um override no campo `Prefab`).

**Nova onda:** `Create → Starfall → Waves → Wave Definition`. Adicione entradas (`Enemy`, `Count`, `Interval`, `DelayBefore`, `Pattern`: TopRandom, TopCenter, TopLeft/Right, TopLine, TopVee, TopFixed, TopAlternate). `WaitForClear` segura o próximo evento até a tela limpar; `MaxDuration` é o timeout.

**Nova fase:** `Create → Starfall → Stages → Stage Definition`. Preencha nome, briefing, cores de fundo/névoa, música e a lista de `Events` (Wave, Delay, Message, MiniBoss, Boss, AsteroidField). Adicione a fase ao array `Stages` em `ScriptableObjects/Config/GameConfig.asset`; o desbloqueio e o botão "Próxima fase" seguem a ordem do array.

**Novo chefe:** `Create → Starfall → Bosses → Boss Definition` com prefab que contenha `BossController` (copie `Prefabs/Bosses/Destroyer`). Defina `Phases` (limiar de vida, movimento, lista de `BossAttack`: Ring, Aimed, Forward, SideCannons, Missiles, FrontLaser, Stream, Summon, Web). Uma fase com `Sprite`/`Tint`/`Scale` preenchidos vira uma transformação, com invulnerabilidade por `TransformSeconds`. `SegmentCount` acima de zero dá ao chefe um corpo em segmentos (Leviathan). Chefes principais precisam de um `BossId` próprio; mini-chefes usam `MiniBoss`.

**Áudio final:** preencha os slots de `ScriptableObjects/Audio/AudioLibrary.asset`; slots vazios usam placeholders sintetizados (desative em `UseSynthesizedPlaceholders`).

## Build Android passo a passo

1. Unity Hub → editor 6000.3.23f1 → *Add modules* → Android Build Support + OpenJDK + SDK & NDK Tools.
2. Edit → Project Settings → Player: já configurado pelo bootstrap (portrait, IL2CPP, ARM64, `com.starfallteam.starfalldefense`, minSdk 24).
3. Starfall → Build → Android APK. Para Play Store, ative *Build App Bundle* em Build Settings.
4. Instale com `adb install Builds/Android/StarfallDefense.apk`.

## Licenças e assets

Todos os sprites são formas procedurais originais; todo o áudio é sintetizado em runtime. Nenhum asset de terceiros é incluído.

## Modos de jogo

| Modo | Como funciona |
|---|---|
| Campanha | 5 fases em ordem; cada uma desbloqueia a seguinte e guarda a melhor pontuação |
| Sobrevivência | Ondas procedurais infinitas, dificuldade crescente, mini-chefe a cada 8 ondas |
| Boss Rush | Os quatro chefes principais em sequência; desbloqueia ao derrotar qualquer chefe |
| Desafio Diário | Semente derivada da data: mesma sequência para todos no dia, inimigos mais rápidos e menos drops |

## Progressão permanente

Cada partida rende **créditos** (pontuação/10), **XP** e **componentes** (elites e chefes). No menu:

- **Hangar** — comprar e equipar as cinco naves e as sete armas, com prévia em holograma e comparação de atributos.
- **Upgrades** — dez nós em cinco níveis (dano, cadência, alcance, escudo, regeneração, casco, velocidade, aceleração, recarga e potência da Ultimate). Os últimos níveis também custam componentes.
- **Ranking** — top 10 local por modo e a lista de conquistas.

Nada disso usa rede: tudo fica em `Application.persistentDataPath/starfall_save.json` (versão 2, migração automática de saves da versão 1).
