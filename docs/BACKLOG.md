# Backlog

## MVP 1.0 — estado

| Item | Estado |
|---|---|
| Nave SF-01 Vanguard, 8 direções, limites da tela | Implementado (validar no Editor) |
| Laser com 5 níveis | Implementado |
| 5 inimigos (Drone, Interceptor, Bomber, Kamikaze, Shield) + asteroide | Implementado |
| Sentinel-X (2 fases) | Implementado |
| The Destroyer (3 fases, canhões, mísseis, laser com aviso) | Implementado |
| 3 fases orientadas a dados | Implementado |
| Vidas, escudo/casco, invulnerabilidade, respawn, Game Over | Implementado |
| Pontuação e multiplicador x1–x10 | Implementado + testes |
| 6 power-ups com regras de repetição | Implementado + testes |
| Energia e Ultimate | Implementado + testes |
| Menu, Briefing, Pausa, HUD, Vitória, Game Over, Configurações, Créditos | Implementado |
| Áudio (placeholders sintetizados) + volumes persistidos | Implementado |
| Save local com versão, validação e reset | Implementado + testes |
| Teclado, controle e toque | Implementado |
| Build Android (IL2CPP/ARM64) e projeto iOS | Script pronto; requer módulos instalados |
| Vertical slice (critérios 1–15) | Validado por testes PlayMode em batch mode (`VerticalSliceTests`, 3/3); validação visual manual pendente (docs/QA_CHECKLIST.md) |

## Melhorias pós-MVP

- Bloom/neon com URP 2D (D-004).
- Arte e áudio finais substituindo placeholders (slots já existem).
- Joystick virtual opcional e botão de disparo dedicado para quem desliga o auto-fire.
- Seleção de fase no menu (hoje "Continuar" vai para a mais alta desbloqueada).
- Vibração (haptics) ao sofrer dano.
- Tela de resultados com bônus por "sem dano".
- Remapeamento de controles na UI (a camada de entrada já centraliza as bindings).
- Testes PlayMode: ciclo Boot→Menu→Gameplay→Vitória automatizado.
- Localização (pt-BR/en).

## Visão 2.0 (fora do MVP)

Coop online, ranking global, clãs, Battle Pass, eventos semanais, editor de fases, integração Steam, Cloud Save, compras, serviços online, modos Sobrevivência / Boss Rush / Desafio Diário, cinco naves jogáveis, árvore de progressão permanente, inimigos elite (500 pts), dano crítico/perfuração/tipos de dano (campos já reservados em `DamageInfo`/`ProjectileSpec.Pierce`).

## Débitos técnicos conhecidos

- O gerador (`ProjectBootstrap`) sobrescreve valores de balanceamento ao ser re-executado (D-006).
- `HudPresenter.Update` monta uma string por frame para o indicador especial (alocação pequena; trocar por cache por segundo).
- `StageBackground` instancia estrelas com `new GameObject` na carga da fase (não em gameplay); poderia usar `ParticleSystem`.
- Sem `AudioMixer`: volumes aplicados por multiplicação direta nas fontes.
- A névoa da fase 3 é um sprite plano; sem efeito de parallax.
- `PlaceholderAudioSynth` gera clipes de música por cálculo no primeiro uso (~100 ms).
- Tests PlayMode ainda não cobrem transição de cena.

## Lacunas apontadas pela validação do GDD (`docs/GDD_COMPLIANCE.md`)

- Animação/estado de dano crítico da nave (casco baixo: fumaça, alerta no HUD).
- Propulsores visuais nos inimigos.
- Partículas de fumaça e faíscas (hoje só sprites animados por código).
- Loops de ambiente: motores da nave e nebulosa; trilhas finais por estilo (orquestral, eletrônica, synthwave).
- Dano em área real para os projéteis do Bomber (`DamageType.Explosive` já existe).
- Splash com logo do estúdio quando existir o asset.

---

## Estado após a entrega completa do GDD (D-016)

Concluídos e removidos do backlog: cinco naves, sete armas, classe Elite, mini-chefes Widow e Reaper Wing, chefes Leviathan/Hive Queen/Omega Core, fases 4 e 5, progressão permanente, árvore de upgrades, telas Hangar/Melhorias/Ranking, seleção de missão, conquistas, modos Sobrevivência/Boss Rush/Desafio Diário, partículas, shaders, ambiente sonoro, dano crítico da nave e propulsores dos inimigos.

### Continua pendente

**Assets finais**
- Substituir os sprites placeholder por arte definitiva (o pipeline já aceita: cada `Definition` tem campo `Sprite`).
- Substituir o áudio sintetizado preenchendo `AudioLibrary.asset`, incluindo as trilhas por estilo do GDD §21.
- Logo do estúdio no splash.

**Polimento**
- URP + Bloom para o neon (hoje é blending aditivo no Built-in RP, D-021).
- Balanceamento após playtest: itens marcados com ⚠️ em `docs/BALANCING.md`, em especial Laser Duplo, Mísseis e a Phantom.
- Tutorial contextual mais explícito na fase 1.
- Vibração (haptics) no dano e na Ultimate.

**Visão de Futuro (2.0, do próprio GDD)**
- Coop online, ranking global, clãs, eventos semanais, Battle Pass, editor de fases, Steam Achievements, Steam Cloud Save, 20 fases.
- O ranking e as conquistas existem em versão local (D-017, D-020); migrar para servidor exige apenas trocar a fonte de dados.

**Débitos técnicos**
- `PlaceholderAudioSynth` gera clipes na primeira reprodução; pré-gerar no boot evitaria um pico de CPU.
- Os segmentos do Leviathan usam um rastro em lista; com muitos chefes segmentados valeria um buffer circular.
- Não há teste PlayMode para os modos Sobrevivência/Boss Rush/Diário (só para a campanha).

---

## Plano Mestre STAR RISK — quadro de acompanhamento (docs/product/IMPLEMENTATION_MASTER_PLAN.md)

| ID | Item | Prioridade | Status | Observação |
|---|---|---|---|---|
| SR-PROD-001 | Confirmar nome comercial | P0 | Blocked | Exige pesquisa nas lojas/marca pelo responsável do produto; projeto segue "Starfall Defense" |
| SR-GAME-001 | Zona de Risco | P0 | QA | Implementada com testes; falta validação em aparelho |
| SR-GAME-002 | Overdrive | P0 | QA | Implementado com testes, camada musical e reações por facção |
| SR-GAME-003 | Graze | P1 | QA | Implementado com testes e opção de acessibilidade |
| SR-DESIGN-001 | Reestruturar facções | P0 | In Progress | Federação e Cyber definidas em dados (Phantom → Cyber); Biomecânica sem nave até a fase C |
| SR-BAL-001 | Reequilibrar Nova-X | P0 | QA | Horizontal; falta teste com jogadores |
| SR-ECO-001 | Separar créditos de score | P0 | QA | Nova fórmula testada e simulada |
| SR-QA-001 | Teste em aparelhos reais | P0 | Backlog | Nunca executado |
| — | Ranks D–SSS | P1 | QA | Limiares por fase; partes de chefe e dificuldade reservados |
| — | Tutorial do núcleo | P0 | In Progress | Dicas contextuais na fase 1 (risco, overdrive, graze); faltam desviar/upgrade/rank |
| — | Dez fases / dez chefes | P1 | Backlog | Fase E |
| — | Builds temporárias | P1 | Backlog | Fase E |
| SR-MON-001/002/003 | Ads, Pacote Comandante, loja cosmética | P1 | Backlog | Fase F; SDK atrás de interface |
| — | Analytics, política de privacidade com coleta | P1 | Backlog | Fase F |
| — | Ranking online | P2 | Backlog | Modelo de dados já carimba seed, versão de balanceamento e revive |
| — | Fatia de arte final | P0 | Backlog | Fase D |
