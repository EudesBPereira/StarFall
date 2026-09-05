# Decisões técnicas e de design

Registro das decisões tomadas para requisitos ambíguos do blueprint, com alternativas consideradas e impactos.

## D-001 — Plataforma alvo: Android e iOS (mobile), não Windows

- **Contexto:** o blueprint descreve "Plataforma inicial: Windows PC", mas o pedido do produto foi explícito: "game usando Unity para mobile Android e iPhone".
- **Decisão:** o alvo primário é mobile (Android via IL2CPP/ARM64, iOS via projeto Xcode). Windows continua funcionando no Editor e como build de desenvolvimento (menu `Starfall/Build/Windows`), útil para playtest com teclado e controle.
- **Alternativas:** manter Windows como alvo e adaptar depois. Rejeitada porque o toque muda a camada de entrada, o HUD e o layout desde o início.
- **Impacto:** entrada por arrasto (touch), HUD com área segura (notch), botões na tela, orientação portrait, resolução de referência 1080x1920.

## D-002 — Orientação retrato (portrait 9:16), não paisagem 16:9

- **Contexto:** o blueprint sugere paisagem 16:9 como padrão "caso o projeto ainda não determine". Para um shoot'em up vertical em celular, retrato oferece muito mais espaço vertical e é o padrão do gênero (referência Sky Force).
- **Decisão:** portrait. A área de jogo é definida pela **largura** visível (10 unidades), e a altura deriva da proporção real do dispositivo (`PlayArea`). Assim, 9:16, 9:19.5 e tablets 3:4 funcionam sem mudanças de código.
- **Impacto:** HUD ancorado por cantos; spawn no topo; jogador na parte inferior. Tudo continua funcionando em paisagem no Editor, apenas com menos altura.

## D-003 — Esquema de controle mobile: arrastar para mover + auto-fire

- **Decisão:** o dedo em qualquer ponto da tela (fora dos botões) move a nave por delta 1:1 (com sensibilidade configurável). O disparo é automático por padrão (`autoFire`, persistido no save). A Ultimate é acionada tocando na barra de energia (ou tecla E / botão oeste do controle). Pausa por botão no topo (ou Esc / Start).
- **Alternativas:** joystick virtual (menos preciso para shmup), toque-para-ir (menos responsivo).
- **Impacto:** a classe `GameInputReader` centraliza teclado, gamepad, arrasto e botões de UI; remapeamento futuro altera apenas `BuildActions()`.

## D-004 — Pipeline de render: Built-in Render Pipeline

- **Decisão:** usar o Built-in RP com sprites no MVP. URP 2D com Bloom fica no backlog (visual neon).
- **Motivo:** menos assets/configuração gerados por script, menor risco em batch mode; performance suficiente para o MVP.

## D-005 — Assets placeholders gerados por código

- **Decisão:** sprites brancos procedurais (formas SDF) gerados pelo Editor em `Assets/_Project/Art/Placeholders`, tintados em runtime. Áudio placeholder sintetizado em memória (`PlaceholderAudioSynth`) quando um slot do `AudioLibrary` está vazio.
- **Motivo:** o blueprint proíbe assets de terceiros e pede que não se fabriquem arquivos de áudio inválidos; áudio sintetizado em runtime não cria arquivos.
- **Impacto:** trocar arte = substituir `Sprite` nos ScriptableObjects; trocar áudio = preencher slots do `AudioLibrary`.

## D-006 — Geração do projeto por script do Editor

- **Decisão:** cenas, prefabs e ScriptableObjects são criados por `Starfall/Run Full Bootstrap` (`ProjectBootstrap`), executável também em batch mode. Nenhum YAML de cena foi escrito à mão.
- **Motivo:** garante GUIDs consistentes e referências corretas sem editar arquivos serializados manualmente. Re-executar o gerador atualiza os assets existentes (idempotente), mas **sobrescreve** valores de balanceamento editados à mão nos assets gerados.
- **Impacto:** após ajustar balanceamento no Inspector, não rode o gerador de novo sem antes migrar os valores para `ProjectBootstrap.CreateData` ou desativar o passo correspondente.

## D-007 — Regras de power-ups repetidos

| Tipo | Efeito | Repetição |
|---|---|---|
| Azul (Laser) | +1 nível de arma (máx. 5) até o fim da fase | Acumula até o máximo; não reseta ao perder vida |
| Verde (Escudo) | Restaura 50 de escudo (limitado ao máximo) | Instantâneo |
| Vermelho (Dano) | Dano x2 por 8 s | Renova a duração; **não** acumula magnitude |
| Amarelo (Velocidade) | Velocidade x1,4 por 8 s | Renova a duração |
| Roxo (Energia) | +40 de energia da Ultimate | Instantâneo |
| Branco (Invencível) | Invulnerável por 5 s | Renova a duração |

- Renovar = `max(restante, nova duração)`. Efeitos temporários são removidos ao morrer. O nível de laser persiste durante a fase e volta a 1 no início da próxima fase.

## D-008 — Pontuação e origem do dano

- Só abates causados pelo jogador (`DamageSource.Player`) ou pela Ultimate (`DamageSource.Ultimate`) pontuam. Kamikaze que explode ao bater na nave (`Environment`) não pontua nem dropa item.
- Só abates por armas do jogador carregam a barra de energia; abates pela Ultimate não recarregam (evita loop).
- Multiplicador: +1 a cada 4 abates sem sofrer dano, até x10; volta a x1 ao sofrer dano aplicado (dano bloqueado por invulnerabilidade não reseta).
- Asteroides valem 25 pontos, não bloqueiam a conclusão de ondas e são imunes à Ultimate.

## D-009 — Vidas, respawn e pontuação entre fases

- 3 vidas por campanha; vidas e pontuação são carregadas entre fases pela `GameSession` (estático, não persistido).
- "Reiniciar fase" recomeça com as vidas/pontuação com que a fase começou.
- Respawn após 1,5 s com 2,5 s de invulnerabilidade; projéteis inimigos são limpos no respawn.
- "Continuar" no menu inicia uma nova campanha a partir da fase mais alta desbloqueada.

## D-010 — Bus de eventos estático e serviços persistentes

- `GameSignals` é um bus estático tipado. Justificativa: desacopla inimigos, jogador, pontuação, HUD e áudio sem dependências diretas; simples de testar. Regra: todo assinante cancela a inscrição em `OnDisable/OnDestroy`.
- Únicos objetos globais: `AudioManager` (DontDestroyOnLoad, música contínua entre cenas) e `SaveService` (fachada estática sobre `SaveRepository`, testável). `GameplayContext.Current` é um localizador com escopo de cena, limpo ao descarregar.

## D-011 — Física 2D

- Todas as entidades usam `Rigidbody2D` cinemático + colisores trigger; movimento por transform. A matriz de colisão é configurada pelo bootstrap (apenas pares relevantes). O laser frontal do chefe tem corpo próprio para não disparar o dano de contato do chefe.

## D-012 — Testes

- Lógica de regras vive em `Scripts/Logic` sem dependência de UnityEngine. Os mesmos testes NUnit rodam no Unity Test Framework (EditMode) e em `dotnet test` (`Tools/LogicTests`), permitindo validação em CI sem o Editor.

## D-013 — Input System referenciado por tarball local

- **Contexto:** o download do pacote `com.unity.inputsystem@1.14.2` pelo Package Manager falhou repetidamente neste ambiente (conexão abortada), embora o registro liste a versão.
- **Decisão:** o tarball oficial foi baixado e versionado em `Packages/com.unity.inputsystem-1.14.2.tgz`, referenciado no manifest como `file:com.unity.inputsystem-1.14.2.tgz`. Conteúdo idêntico ao do registro.
- **Como reverter:** trocar a linha do manifest para `"com.unity.inputsystem": "1.14.2"` e apagar o tarball.
- **Impacto:** +16 MB no repositório; nenhum impacto em runtime.

## D-014 — Instalação do Unity sem elevação (ambiente de desenvolvimento)

- O instalador NSIS do Unity pede UAC (`highestAvailable`). Em ambientes sem interação, foi executado com `__COMPAT_LAYER=RunAsInvoker` e `/S /D=<pasta do usuário>`, extraindo o editor em `C:\Users\<user>\Unity\Editor\6000.3.23f1`. O Hub pode não listar essa instalação; abra o projeto por `Unity.exe -projectPath` ou adicione a pasta em *Locate* no Hub.

## D-015 — TLS interceptado por antivírus no ambiente de build Android

- **Contexto:** o Gradle (OpenJDK embutido no Unity) não conseguia resolver o Android Gradle Plugin porque o Norton Web Shield substitui os certificados TLS por uma raiz própria, desconhecida do `cacerts` do JDK. O mesmo mecanismo causava falhas intermitentes de download (Hub, UPM, curl).
- **Decisão (ambiente local, não versionada):** a raiz "Norton Web/Mail Shield Root" foi importada em `Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/lib/security/cacerts` (alias `norton-webshield`, backup em `cacerts.bak`). Nenhuma alteração no repositório.
- **Alternativa:** desativar a inspeção HTTPS do antivírus para o Unity/Gradle, ou usar um `gradleTemplate.properties` com `javax.net.ssl.trustStore` (rejeitada por ser específica da máquina).

## D-016 — GDD passa a ser a fonte de verdade (escopo completo)

- **Contexto:** o usuário determinou que `document.md` (GDD v1.0) substitui o recorte de MVP do `blueprint.md` e pediu a implementação de tudo que faltava.
- **Decisão:** implementados naves (5), armas (7), classe Elite, mini-chefes Widow e Reaper Wing, chefes Leviathan/Hive Queen/Omega Core, fases 4 e 5, progressão com créditos/XP/componentes, árvore de upgrades, telas Hangar/Melhorias/Ranking, conquistas locais e os modos Sobrevivência, Boss Rush e Desafio Diário.
- **Fora do escopo (continuam no backlog):** itens da "Visão de Futuro" do próprio GDD que exigem serviços online — coop, ranking global, clãs, eventos semanais, Battle Pass, editor de fases, Steam Achievements e Cloud Save.

## D-017 — Ranking local em vez de global

- **Contexto:** o GDD §16 pede uma tela de Ranking, mas a Visão de Futuro coloca "ranking global" na versão 2.0.
- **Decisão:** placar local no save (top 10 por modo), com abas Campanha / Sobrevivência / Boss Rush / Diário. Nenhum acesso à rede.
- **Impacto:** a tela existe e é útil offline; migrar para servidor depois exige só uma fonte de dados nova.

## D-018 — Nova-X desbloqueia pela campanha, não por créditos

- **Contexto:** o GDD chama a Nova-X de "nave lendária desbloqueável" sem definir o critério.
- **Decisão:** concluir a fase 5 desbloqueia a Nova-X; as outras três naves são compradas com créditos.
- **Alternativa:** preço muito alto em créditos (rejeitada por transformar a nave lendária em grind).

## D-019 — "Munição especial" do HUD mostra o efeito ativo

- **Contexto:** o GDD §17 pede "munição especial" no canto inferior direito, mas nenhuma arma consome munição.
- **Decisão:** o campo mostra o efeito temporário com maior duração restante (DANO, VELOCIDADE, INVENCÍVEL, LENTO) e o tempo em segundos. A carga do Canhão de Energia aparece como barra no canto inferior esquerdo.

## D-020 — Conquistas locais sem Steam

- **Contexto:** o GDD §22 lista cinco conquistas; a integração Steam é da versão 2.0.
- **Decisão:** avaliadas em `AchievementRules` (lógica pura, testada) e persistidas como bitmask no save, com aviso na tela e listagem no Ranking.

## D-021 — Shaders próprios em vez de pós-processamento

- **Contexto:** o GDD §19 pede shaders de escudo, raios e hologramas e um visual neon.
- **Decisão:** quatro shaders próprios sem dependências (`Starfall/HologramSprite`, `ShieldSprite`, `AdditiveSprite`, `EnergyBeam`). O brilho neon vem de blending aditivo, não de Bloom.
- **Motivo:** manter o Built-in Render Pipeline (D-004) e o custo baixo em celular. URP + Bloom continua no backlog.

## D-022 — Um MonoBehaviour por arquivo

- **Contexto:** `GameplayPanels.cs` e `MenuPanels.cs` agrupavam vários MonoBehaviours. O Unity só cria um MonoScript por arquivo (o que combina com o nome do arquivo), então painéis como Pausa, Vitória e Game Over ficavam com referência de script quebrada na cena gerada (`m_Script` sem GUID) e chegavam como `null` em runtime.
- **Descoberta:** teste PlayMode `Boot_LoadsMainMenu` falhou com `missionPanel` nulo; a inspeção do YAML da cena confirmou o `m_Script` sem GUID.
- **Decisão:** cada MonoBehaviour vive no próprio arquivo, com o mesmo nome da classe. Arquivos criados: `BriefingPanel`, `PausePanel`, `VictoryPanel`, `GameOverPanel`, `MenuPanel`, `ListRow`, `MissionPanel`, `HangarPanel`, `UpgradesPanel`, `RankingPanel`.
- **Impacto:** regra permanente do projeto; structs, enums e classes puras podem continuar compartilhando arquivo.
