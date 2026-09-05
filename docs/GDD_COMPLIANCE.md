# Conformidade com o GDD (`document.md`)

Validação do projeto implementado contra o Game Design Document v1.0, seção por seção. O GDD descreve a visão completa do jogo; o `blueprint.md` (prompt de execução) recorta o **MVP 1.0** e manda deixar o resto no backlog. Por isso a coluna "Situação" usa:

- ✅ **Atende** — implementado como descrito.
- 🟡 **Parcial** — implementado com simplificação/placeholder, ou de forma diferente do GDD por decisão registrada.
- ⏭️ **Fora do MVP** — item da visão completa, excluído explicitamente pelo blueprint (§3 "Não implementar nesta etapa") e listado em `docs/BACKLOG.md`.
- ❌ **Não atende** — deveria estar no MVP e não está (ou diverge sem justificativa).

Validado por inspeção estática do código/dados + testes automatizados (EditMode 35/35, PlayMode 3/3, build Android OK). Itens visuais/sonoros ainda dependem de validação manual no Editor (`docs/QA_CHECKLIST.md`).

## §1–3 Visão, história e loop

| Item do GDD | Situação | Evidência / observação |
|---|---|---|
| Conceito: shmup 2D, piloto da frota terrestre vs. invasão | ✅ | Briefings das fases e créditos citam The Swarm / SF-01 Vanguard |
| Plataforma "PC (Windows) inicialmente" | 🟡 | Usuário definiu **Android/iOS** (D-001). Windows continua como build de playtest |
| Engine Unity 6 (2D) | ✅ | 6000.3.23f1, projeto 2D (sprites, Physics2D) |
| Loop: iniciar fase → destruir → coletar → mini-chefe → chefe → recompensas → próxima fase | ✅ | `GameFlowController` + `StageDirector`; recompensas = bônus de conclusão + recorde salvo |
| Loop: "Compra melhorias permanentes" | ⏭️ | Árvore de progressão permanente excluída do MVP (blueprint §3) |

## §4 Mecânicas

| Item | Situação | Evidência |
|---|---|---|
| Movimento em 8 direções, limite de tela, movimento suave | ✅ | `PlayerMovement` (aceleração/desaceleração, clamp em `PlayArea`), teste `Gameplay_VerticalSlice` |
| Arma básica: laser simples | ✅ | `Weapons/Laser.asset`, `WeaponController` |
| Laser Duplo | 🟡 | Não é arma separada: o nível 2 do laser dispara dois projéteis paralelos |
| Spread Shot | 🟡 | Níveis 3–5 do laser adicionam tiros angulados (leque). Arma dedicada fica no backlog |
| Plasma, Railgun, Mísseis, Canhão de Energia | ⏭️ | "1 arma principal" no MVP. Bases prontas: `ProjectileSpec.Pierce` (railgun), `ProjectileSpec.Homing` (mísseis, já usado pelo Destroyer) |

## §5 Sistema de vida

| Item | Situação | Evidência |
|---|---|---|
| 3 vidas iniciais | ✅ | `GameConfig.StartingLives = 3` |
| Cada vida possui escudo e integridade | ✅ | `PlayerShip.SpawnAt` restaura ambos a cada respawn |
| Escudo absorve primeiro; sem escudo, dano reduz vida | ✅ | `HealthModel.TakeDamage`; testes `Damage_HitsShieldBeforeHull`, `Damage_OverflowsFromShieldIntoHull` |

## §6 Power-ups

| Cor (GDD) | Situação | Implementação |
|---|---|---|
| Azul – poder da arma | ✅ | `LaserLevel`: +1 nível (máx. 5) |
| Verde – recuperação de escudo | ✅ | `ShieldRestore` +50 |
| Vermelho – dano extra temporário | ✅ | `DamageBoost` x2 por 8 s |
| Amarelo – velocidade aumentada | ✅ | `SpeedBoost` x1,4 por 8 s |
| Roxo – "arma especial" | 🟡 | Implementado como **+40 de energia da Ultimate** (blueprint §10 permite "energia especial ou ativação especial simplificada"; D-007). Arma especial dedicada depende das armas da v2 |
| Branco – invencibilidade temporária | ✅ | `Invincibility` 5 s |

## §7 Sistema de especial (Ultimate)

| Item | Situação | Evidência |
|---|---|---|
| Barra carregada ao destruir inimigos | ✅ | `UltimateController.OnEnemyDestroyed` (energia por tipo de inimigo) |
| Ativa só quando cheia | ✅ | `EnergyModel.TryConsumeAll`; testes `Ultimate_OnlyWhenFull` e PlayMode |
| "Explosão massiva na tela" | 🟡 | Flash de tela + shake + limpeza de projéteis; sem partículas/onda de choque (placeholder) |
| Elimina inimigos menores instantaneamente | ✅ | `Health.Kill(Ultimate)` em comuns; elites/chefes recebem 150 de dano |

## §8 Pontuação

| Item | Situação | Evidência |
|---|---|---|
| Comum 100 / Elite 500 / Mini-chefe 2 500 / Chefe 10 000 | ✅ | Assets: Drone…Shield = 100, Sentinel-X = 2 500, Destroyer = 10 000. Elite (500) não existe no MVP (⏭️) |
| Multiplicador x1–x10, mantido sem sofrer dano | ✅ | `ScoreModel` (+1 a cada 4 abates, reset ao sofrer dano); testes `Multiplier_IsCappedAtMax`, `Damage_ResetsMultiplierToOne` |

## §9 Inimigos

| Classe | Situação | Implementação |
|---|---|---|
| Drone – vida baixa, movimento simples | ✅ | 12 HP, descida reta, tiro frontal esparso |
| Interceptor – muito rápido, ataques frequentes | ✅ | 4 u/s em zigue-zague, rajada dupla mirada a cada 1,5 s |
| Bomber – lento, projéteis explosivos | 🟡 | 1,4 u/s, leque de 3 projéteis grandes (1,7x) de dano 18. Sem área de explosão real (dano em área fica no backlog; `DamageType.Explosive` já existe) |
| Kamikaze – persegue, explode ao contato | ✅ | `ChaseMovement` + `SelfDestructOnContact` |
| Shield – escudos, múltiplos disparos | ✅ | 30 de escudo + 20 de casco, anel visual que some ao quebrar |
| Elite – versões melhoradas, maior recompensa | ⏭️ | Excluído pelo blueprint; flag `IsElite` e regra de Ultimate já preparadas |

## §10 Mini-chefes

| Item | Situação | Observação |
|---|---|---|
| "Cada fase possui um" | 🟡 | Blueprint §3/§8: **1 mini-chefe** no MVP, na Fase 2 (posição configurável por `StageEvent`) |
| Sentinel-X – robô gigante, ataque giratório | ✅ | `BossDefinition` SentinelX: anel de projéteis com fase rotativa; 2 fases comportamentais |
| Widow, Reaper Wing | ⏭️ | Backlog. Criar = novo `BossDefinition` + prefab com `BossController` |

## §11 Chefes

| Item | Situação | Observação |
|---|---|---|
| "Cada fase termina em um" | 🟡 | MVP: 1 chefe. Fases 1 e 2 terminam por onda final / mini-chefe (blueprint §9) |
| The Destroyer – canhões laterais, mísseis, laser frontal | ✅ | 3 padrões (`SideCannons`, `Missiles` teleguiados, `FrontLaser` com telegraph) e 3 fases por vida |
| Destroyer como "Boss 1" (Fase 1 no GDD) | 🟡 | Blueprint §8 manda encerrar a **Fase 3** com ele; seguido o blueprint |
| Leviathan, Hive Queen, Omega Core | ⏭️ | Backlog (v2: 20 fases) |

## §12 Fases

| Fase | Situação | Evidência |
|---|---|---|
| 1 Setor Orbital – Terra próxima, satélites destruídos | ✅ | `Stage1_OrbitalSector`: fundo azul-escuro, estrelas densas, destroços de satélite (`DebrisSprites`) |
| 2 Campo de Asteroides – asteroides móveis | ✅ | `AsteroidField` event + `Asteroid` (obstáculo destrutível, dano de contato) |
| 3 Nebulosa Violeta – dificuldade maior, baixa visibilidade | ✅ | Ondas densas com os 5 tipos; névoa roxa (alpha 0,22) desenhada abaixo dos projéteis |
| 4 Fortaleza Mecânica, 5 Núcleo da Colmeia | ⏭️ | Backlog. `GameConfig.Stages` aceita novas fases sem código |

## §13–15 Progressão, upgrades, naves

| Item | Situação |
|---|---|
| Créditos / experiência / componentes após a fase | ⏭️ (recompensa do MVP = bônus de pontos + desbloqueio da fase) |
| Árvore de upgrades (armas, escudo, casco, mobilidade, especial) | ⏭️ |
| Naves Falcon, Titan, Phantom, Nova-X | ⏭️ — `ShipDefinition` já modela vida/escudo/velocidade/arma por nave |
| Vanguard balanceada | ✅ |

## §16 Telas

| Tela | Situação | Observação |
|---|---|---|
| Splash (logo do estúdio + Unity) | 🟡 | Cena `Boot` com título; logo Unity aparece automaticamente na licença Personal; sem logo do estúdio (asset inexistente) |
| Menu: Jogar, Configurações, Créditos, Sair | ✅ | + "Continuar" (progresso salvo) |
| Menu: Hangar, Melhorias, Ranking | ⏭️ | Dependem de naves/upgrades/ranking (v2) |
| Seleção de nave (3D/2D animada) | ⏭️ | MVP inicia direto com a Vanguard (blueprint §12) |
| Briefing de missão (resumo + objetivos) | ✅ | `BriefingPanel` por fase |
| Gameplay com HUD completa | ✅ | ver §17 |
| Vitória (resultados) | ✅ | pontuação, maior multiplicador, inimigos, dano recebido, vidas, recorde |
| Derrota (Game Over) | ✅ | pontuação final, reiniciar, menu |
| Créditos (equipe, ferramentas) | ✅ | painel de créditos |

## §17 HUD

| Posição | GDD | Situação |
|---|---|---|
| Superior esquerdo | Vida, escudo | ✅ vidas + barras de casco e escudo |
| Superior direito | Pontuação, multiplicador | ✅ |
| Centro inferior | Energia Ultimate | ✅ barra tocável que ativa a Ultimate |
| Inferior esquerdo | Arma atual | ✅ "LASER LV.n" |
| Inferior direito | Munição especial | 🟡 mostra o efeito temporário ativo e tempo restante (não há munição no MVP) |

## §18 Animações

| Item | Situação | Observação |
|---|---|---|
| Nave: impulso dos motores | ✅ | `ThrusterFlicker` (sprite com tremulação) |
| Nave: inclinação lateral | ✅ | `PlayerMovement.bankTarget` |
| Nave: dano crítico | ❌ | Só flash vermelho ao sofrer dano; falta estado de casco crítico (fumaça/alerta). Adicionado ao backlog |
| Inimigos: propulsores | ❌ | Sem efeito de propulsão nos inimigos. Backlog |
| Inimigos: explosões, efeitos de escudo | ✅ | `ExplosionEffect`, anel de escudo com pulso e quebra |
| Chefes: entrada cinematográfica | ✅ | Aviso + descida suave + shake, invulnerável na entrada |
| Chefes: transformações | 🟡 | Troca de fase = flash + mensagem + novo padrão; sem mudança de forma |
| Chefes: destruição final | ✅ | Cadeia de explosões + flash |
| UI: botões animados, barras preenchendo, som ao passar o mouse | ✅ | `ColorBlock` de transição, `Image.fillAmount`, `UiButtonSfx` |

## §19 Efeitos visuais

| Item | Situação |
|---|---|
| Partículas: tiros, faíscas, explosões, energia | 🟡 sprites animados por código (placeholder); `ExplosionEffect` aceita um `ParticleSystem` opcional |
| Partículas: fumaça | ❌ backlog |
| Shaders: escudos, raios, hologramas | ⏭️ Built-in RP sem shaders customizados (D-004); URP + Bloom no backlog |

## §20–21 Áudio

| Item | Situação |
|---|---|
| Laser, míssil, alarmes (aviso de chefe), interface (seleção/confirmação/erro) | 🟡 slots existem e tocam placeholders sintetizados |
| Plasma, railgun | ⏭️ armas da v2 |
| Motores, ambiente de nebulosa | ❌ sem loop ambiente; backlog |
| Trilha por fase (orquestral, eletrônica, synthwave) + trilhas de chefe | 🟡 `MusicId` por fase e de chefe com placeholders sintetizados; estilos finais dependem de assets |

## §22–23 Conquistas e modos

| Item | Situação |
|---|---|
| Conquistas (Primeiro Abate, Sobrevivente, …) | ⏭️ Steam Achievements excluídos; `ScoreModel` já conta abates e dano recebido |
| Campanha | ✅ |
| Sobrevivência, Boss Rush, Desafio Diário | ⏭️ blueprint §3 |

## §24 Estilo artístico

| Item | Situação |
|---|---|
| Sci-fi, neon, cyberpunk; paleta azul elétrico / roxo neon / vermelho / branco holográfico | 🟡 tints seguem a paleta (laser ciano, Sentinel roxo, Destroyer vermelho, UI ciano/rosa), mas os sprites são placeholders brancos e não há bloom |

## §25 Escopo de MVP (checklist do GDD)

| Item | Situação |
|---|---|
| 1 nave jogável | ✅ |
| 1 arma principal | ✅ |
| 5 tipos de inimigos | ✅ |
| 1 mini-chefe | ✅ |
| 1 chefe principal | ✅ |
| 3 fases | ✅ |
| Sistema de pontuação | ✅ |
| Sistema de vidas | ✅ |
| Power-ups | ✅ |
| Menu principal | ✅ |
| Game Over | ✅ |
| Música e efeitos sonoros | 🟡 placeholders sintetizados; integração pronta em `AudioLibrary` |

Visão de Futuro (coop, ranking, 20 fases, clãs, eventos, Battle Pass, editor, Steam) — ⏭️ conforme o próprio GDD.

## Resumo

- **MVP do GDD (§25): 11 de 12 itens atendidos; 1 parcial** (áudio final).
- **Divergências deliberadas** (registradas em `docs/DECISIONS.md`): plataforma mobile (D-001/D-002), Roxo = energia (D-007), Destroyer na Fase 3 e um único mini-chefe (blueprint §8/§9), Built-in RP sem shaders (D-004).
- **Lacunas do MVP a corrigir** (não exigidas pelo blueprint, mas descritas no GDD e baratas): animação de dano crítico da nave, propulsores dos inimigos, fumaça, loops de ambiente/motores. Adicionadas ao `docs/BACKLOG.md`.
- **Fora do MVP por decisão do blueprint**: armas extras, Elite, mini-chefes/chefes adicionais, fases 4–5, progressão/upgrades/naves, Hangar/Melhorias/Ranking, seleção de nave, conquistas, modos extras.
