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
