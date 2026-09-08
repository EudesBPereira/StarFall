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

## D-023 — Plano Mestre STAR RISK vira a direção do produto

- **Contexto:** o usuário entregou `docs/product/IMPLEMENTATION_MASTER_PLAN.md`, que reposiciona o jogo em torno de Zona de Risco, Overdrive, graze, facções, dez fases e monetização ética.
- **Decisão:** o plano é a fonte de direção; o GDD (`document.md`) continua a especificação detalhada. A ordem segue as fases A–G do plano. Esta entrega cobre a **Fase B (núcleo)** e os P0 de dados (Nova-X, facções, economia separada do score).
- **Nome:** confirmado como "Starfall Defense" pelo responsável (D-029).

## D-024 — Zona de Risco: sensor por distância com histerese

- **Como:** `RiskSensor` na nave amostra a cada 0,1 s um círculo de 3,6 u com `OverlapCircleNonAlloc` (inimigos, obstáculos, projéteis inimigos). Cada objeto contribui `peso × (1 - d/R)²`; projéteis pesam 0,55 e inimigos usam `EnemyDefinition.RiskWeight` (kamikaze 1,6; elites 1,4; chefes 2,0 ×1,25).
- **Estabilidade:** `RiskModel` suaviza o valor bruto (sobe a 3,5/s, desce a 1,2/s) e segura cada estado por 0,35 s antes de cair, evitando oscilação. Limiares 0,25 / 0,6 / 1,1.
- **Efeito:** multiplicador x1/x2/x3/x5 aplicado a cada abate (`base × combo × risco`); Overdrive escala por 1,6 com teto x8.
- **Feedback sem HUD:** `RiskVignette` tinge as bordas da tela com a paleta do plano; o rótulo do HUD usa "!", "!!", "!!!" além da cor (acessibilidade).
- **Alternativa rejeitada:** risco por contagem de inimigos na tela (não recompensa proximidade, que é a promessa do produto).

## D-025 — Overdrive automático ao encher

- **Como:** `OverdriveModel` carrega em Perigo (7/s) e Extremo (14/s), com graze (+4), abate próximo (+5), abate em combo (+1) e parte de chefe (+10); drena em Seguro (5/s); dano tira 30 pontos carregando ou metade do tempo restante se ativo; morte zera. Ativa sozinho ao encher e dura 8 s.
- **Por que automático:** no toque não há botão sobrando e o plano define o estado como "conquistado por habilidade", não como escolha.
- **Facções (plano §6):** Federação ganha cadência (x1,15; Falcon x1,2; Titan x1,1; Nova-X x1,3), Cyber ganha +20 % de crítico, Biomecânica regenera escudo. A Nova-X carrega 30 % mais devagar (`OverdriveGainMultiplier 0,7`).
- **Feedback:** camada musical rítmica (`MusicId.OverdriveLayer`) com fade, propulsor pulsando magenta, vinheta ciano/magenta, atração de itens, Ultimate carrega x1,5. Tudo reduzível pela opção "Reduced effects".

## D-026 — Graze com confirmação de saída

- **Como:** o projétil que entra no anel (hitbox + 0,42 u) fica pendente; só pontua quando sai do anel ou expira **sem ter atingido a nave**. Um graze por projétil (`Projectile.Grazed`), nenhum com a nave invulnerável, nenhum com projétil abaixo de 2,5 u/s.
- **Pontos:** 50 × multiplicador de risco, somados fora do combo (fórmula do plano §9.2). Também +4 de Overdrive e +2 XP.
- **Acessibilidade:** som e texto "GRAZE" desligáveis em "Graze feedback".

## D-027 — Créditos separados do score (SR-ECO-001)

- **Antes:** créditos = pontuação ÷ 10.
- **Agora:** `recompensa-base da fase + bônus de rank + bônus de risco (teto 150) + primeira conclusão (= base)`; derrota paga 40 % da base; revive reduz o bônus de rank à metade; modos infinitos pagam por onda com teto. Score fica só para ranking.
- **Simulação (teste `Progression_SimulatedCampaign_...`):** uma campanha completa em rank A rende o suficiente para a primeira nave, não para todas.
- **Migração:** saldos existentes são mantidos; `SaveData.BalanceVersion = 3` é carimbado em cada linha do placar junto com seed e flag de revive (plano §9.4).

## D-028 — Ranks D–SSS por fase e composição do score

- **Como:** `StageResultRules` compõe `abates + graze + objetivo + tempo + sem dano` e classifica pelos limiares derivados de `RankTargetScore` da fase (C 25 %, B 45 %, A 70 %, S 100 %, SS 140 %, SSS 200 %). Bônus de tempo: 40 pontos por segundo abaixo do par, teto 6 000. Sem dano: 5 000. Derrota = D.
- **Reservado:** partes de chefe e dificuldade ficam a zero até existirem chefes com componentes destrutíveis e níveis de dificuldade.

## D-029 — Nome comercial confirmado: Starfall Defense (SR-PROD-001)

- **Contexto:** o Plano Mestre recomendava "STAR RISK" e pedia pesquisa de disponibilidade antes de decidir.
- **Decisão do responsável do produto (2026-09-05):** manter **Starfall Defense** como nome comercial. Identificador `com.starfallteam.starfalldefense`, título, menus e documentação permanecem.
- **Impacto:** "STAR RISK" continua apenas como nome do plano de direção e do laço de risco; nenhuma renomeação no projeto.

## D-030 — Teste em aparelho real adiado para o produto completo (SR-QA-001)

- **Decisão do responsável do produto:** a validação em celular será feita quando o jogo estiver completo (fases C–F concluídas), não a cada entrega.
- **Consequência:** itens de gameplay ficam no status "QA" (validados por testes automatizados e build) até a bateria final em aparelho; o plano §20 exige "Device Validation" antes de "Done", então nenhum item de gameplay será marcado como Done antes disso.

## D-031 — Facções em dados e comportamento (fase C, SR-DESIGN-001)

- **Federação** (Vanguard, Falcon, Titan, Nova-X): passiva de precisão — acertos consecutivos sem errar somam +1 % de dano por acerto até +25 % (`PrecisionModel`; o projétil reporta ao despawn se acertou); Overdrive dá cadência; Ultimate **Orbital Strike** (o antigo).
- **Biomecânica** (BX-01 Symbiont, 3 500 créditos): arma própria Spore Launcher (esporos teleguiados, desbloqueada junto com a nave); passiva de absorção — cada abate restaura 2 % do casco (teto 5 %); Overdrive regenera escudo a 8/s; Ultimate **Spore Swarm** — dez esporos buscadores de 45 de dano com área, sem limpar a tela.
- **Corporação Cyber** (CX-7 Nexus 5 000 créditos; Phantom convertida): marcação — todo acerto deixa o alvo 4 s recebendo +25 % de dano (`Health.IncomingDamageMultiplier`); Nexus tem drone companheiro que orbita e dispara sozinho (também marca); Overdrive dá crítico; Ultimate **EMP Burst** — atordoa 3 s (chefes 1,5 s), limpa projéteis, marca tudo por 6 s e causa 90 de dano.
- **Por que a Phantom virou Cyber:** a identidade de precisão/crítico casa com a corporação; a Federação ficou com quatro cascos (o plano permite mais naves humanas depois das outras facções, não antes — nenhuma nova foi criada).
- **Regra do plano respeitada:** nenhuma facção é melhor em tudo — Symbiont tem casco alto e escudo baixo; Nexus tem escudo alto, dano x0,9 e drone; Federação é a linha de base.

## D-032 — Campanha de dez fases (fase E, plano §7)

- **Estrutura adotada (plano §7.4):** 1 Iron Belt (Cinturão de Ferro) · 2 Silent Colony · 3 Orbital Wall (Muralha Orbital) · 4 Living Nebula · 5 Pirate Corridor · 6 Autonomous Factory · 7 Aether Ruins · 8 Dimensional Rift · 9 Stellar Core · 10 Omega Fortress.
- **Reaproveitamento (plano §7.3):** Setor Orbital virou a base da fase 2 (Destroyer); Campo de Asteroides virou a fase 1 (Iron Warden); Nebulosa → fase 4 (Leviathan); Fortaleza Mecânica → fase 6 (Assembler); Núcleo da Colmeia → fase 9 (Hive Queen, colmeia em torno de uma estrela). Omega Core encerra a fase 10.
- **Cada fase tem:** ≥3 ondas com formações próprias (novas: LeftEdge, RightEdge, Pincer, TopColumn, TopArc), um evento ambiental (solar flare, chuva de meteoros ou pulso de nebulosa), um encontro aleatório (probabilidade por semente), pelo menos uma escolha de build, mini-chefe, preparação (delay) e chefe no fim. Os assets antigos das fases 1–5 são substituídos pelos novos nomes `Stage01_…`.
- **Curva:** `EnemyStatMultiplier` 1,00 → 1,50; alvo de rank 18 000 → 80 000; par 190 s → 320 s; créditos-base 300 → 900.
- **Seed reproduzível:** variantes de onda, encontros aleatórios e posição dos hazards derivam de `GameSession.Seed` + índice da fase + índice do evento (`StageVariation`); o seed vai para o placar.

## D-033 — Seis chefes novos com mecânica própria (plano §7.6–§7.7)

| Chefe | Fase | Mecânica exclusiva |
|---|---|---|
| Iron Warden | 1 | duas brocas destrutíveis (desligam anel e chuva de rochas); invoca asteroides |
| Bastion | 3 | muralha com três baterias destrutíveis, cada uma desliga um padrão; laser só na fase 2 |
| Corsair Queen | 5 | investidas, minas que param e explodem, invoca capitães piratas |
| The Assembler | 6 | duas fabricadoras destrutíveis que param a produção de turrets e drones |
| Aether Guardian | 7 | três cristais que blindam o núcleo (ponto fraco); teleporte; laser na fase final |
| Rift Walker | 8 | teleporte e fendas telegrafadas que cospem inimigos em pontos aleatórios da tela |

Recolorações (Destroyer Mk.II, Sentinel-X Mk.II, Widow Prime, Reaper Prime) continuam mini-chefes e não contam como chefes (plano §7.6). `BossId` cobre os dez chefes principais e o Boss Rush enfileira todos.

## D-034 — Partes destrutíveis de chefe

- `BossPart` = filho com vida própria, colisor e sprite; `BossDefinition.Parts` define offset, vida, pontos, ataque desligado e se blinda o núcleo. Pontos de parte entram na fórmula do score (`ScoreModel.RegisterPart`, escalados pelo risco) e carregam Overdrive.
- Enquanto uma parte `ShieldsCore` viver, o núcleo é invulnerável (`BossController.RefreshInvulnerability` combina entrada, transformação e blindagem). Mensagem "CORE EXPOSED" ao cair a última.

## D-035 — Builds temporárias por sorteio com semente

- Doze módulos (`BuildModId`), sorteio de três por `BuildMods.Draft(seed, fase, índice)`, sem repetição no mesmo sorteio, com limite de três pilhas (Magnet uma). Duram só a fase; `BuildState` zera no início.
- O evento `BuildChoice` congela o jogo (`GameState.BuildChoice`, `timeScale 0`) e mostra `BuildDraftPanel`; modos infinitos oferecem a cada cinco ondas e o Boss Rush a cada três chefes.
- Efeitos aplicados por multiplicadores externos (`WeaponController.SetBuild`, `UltimateController.BuildChargeMultiplier`, `RiskSensor.BuildGainMultiplier`, `HealthModel.AddMaxShield`, ímã nos itens). Nada persiste no save (plano §12.7: sem venda de poder).

## D-036 — Eventos ambientais sempre telegrafados

- `HazardController`: solar flare mostra a faixa piscando 1,4 s antes de causar 22 de dano/s por 1,8 s (nunca na linha de spawn); chuva de meteoros avisa 1 s antes; pulso de nebulosa só reduz visibilidade. Isso cumpre "sem situação impossível" do plano §7.5 e a diretriz de dano evitável do GDD.
- `ContentValidationTests` (só no Editor) verifica dez fases/dez chefes, estrutura por fase, velocidades de projétil ≤ 14 u/s, intervalos ≥ 0,1 s, anéis ≤ 20, laser telegrafado ≥ 0,8 s, invocações com limite e spawns laterais só com inimigos `SideSweep`.

## D-037 — Integração de arte e áudio finais por convenção de nome

- `Assets/_Project/Art/Final/**/<nome>.png` substitui o placeholder de mesmo nome no bootstrap (`FinalAssets.TryLoadSprite`); o importador fixa PPU = 100 × largura ÷ canvas do placeholder, para o sprite ocupar o mesmo tamanho no mundo; filtro Point (pixel art) e sem compressão. Sprites finais recebem tinta branca (já vêm coloridos); elites continuam com a tinta dourada automática.
- `Assets/_Project/Audio/Final/{SFX,Music,Ambient}/<Id>[_n].{wav,ogg}` preenche `AudioLibrary` (`FinalAssets.FillAudioLibrary`); slots vazios seguem sintetizados.
- `Art/Final/Fonts/*.ttf` vira a fonte da UI (atlas TMP gerado ao lado); `Art/Final/Icon/icon.png` vira o ícone do app.
- Especificação completa para o artista e o sound designer em `docs/ASSET_SPEC.md`. Motivo: permitir entregas parciais sem tocar em código nem em YAML.
