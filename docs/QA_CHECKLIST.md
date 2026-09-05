# Checklist de QA manual

Executar no Editor (Game view em 1080x1920 e 1920x1080) e em pelo menos um Android real. Marcar ✅/❌ e anotar o build.

## Movimento
- [ ] Teclado WASD e setas movem em 8 direções; diagonais normalizadas.
- [ ] Analógico esquerdo e d-pad movem; a nave inclina ao mover lateralmente.
- [ ] Arrastar o dedo/mouse move a nave 1:1 (sensibilidade das configurações aplicada).
- [ ] A nave nunca sai da área visível, em qualquer resolução.
- [ ] Aceleração/desaceleração perceptíveis, sem "escorregar".

## Disparo
- [ ] Auto-fire ligado: a nave atira continuamente; desligado: só com Espaço / botão sul / arrasto.
- [ ] Azul sobe o nível (1→5) e o HUD "LASER LV.n" acompanha; padrão de tiros muda.
- [ ] Vermelho dobra o dano por 8 s; indicador inferior direito mostra tempo restante.

## Colisões
- [ ] Projéteis do jogador atingem inimigos e asteroides, não o jogador.
- [ ] Projéteis inimigos atingem apenas o jogador.
- [ ] Contato com inimigo causa dano ao jogador; Kamikaze explode ao tocar.
- [ ] Shield: o anel some quando o escudo quebra e só então o casco recebe dano.
- [ ] Power-ups só são coletados pelo jogador.
- [ ] Laser do Destroyer: linha fina de aviso ~1,2 s antes do feixe; dano em ticks.

## Vidas, dano e respawn
- [ ] Escudo absorve antes do casco; barras do HUD atualizam.
- [ ] Após dano: piscar + ~1 s invulnerável; multiplicador volta a x1.
- [ ] Morte: explosão, vida −1, respawn após 1,5 s com invulnerabilidade e projéteis inimigos limpos.
- [ ] Efeitos temporários somem ao morrer; nível de laser permanece.
- [ ] Sem vidas: Game Over com pontuação final e novo recorde quando aplicável.

## Pontuação e Ultimate
- [ ] +100 por inimigo comum × multiplicador; x10 é o máximo.
- [ ] Barra de energia enche com abates; "ULTIMATE READY" + brilho.
- [ ] Ultimate (toque na barra / E / botão oeste) destrói comuns, danifica chefes, limpa projéteis, flash e shake.
- [ ] Ultimate com barra incompleta: som de erro, nada acontece.

## Pausa
- [ ] Esc / Start / botão "II" pausa; tudo congela (inimigos, projéteis, timers).
- [ ] Continuar, Configurações (volumes aplicam ao vivo), Reiniciar fase, Menu.
- [ ] Ao retomar, o arrasto não "pula" a nave.

## Fases, vitória e chefes
- [ ] Fase 1: mensagens de tutorial; drones/interceptors; Supply Drones dropam itens.
- [ ] Fase 2: asteroides caem, Bomber/Kamikaze/Shield; aviso e entrada do Sentinel-X; 2 fases (anel → anel + leque).
- [ ] Fase 3: névoa visível sem esconder projéteis; mistura dos 5; Destroyer com 3 padrões e 3 fases.
- [ ] Barra de vida do chefe aparece e some ao derrotar; música de chefe entra e sai.
- [ ] Vitória: pontuação, maior multiplicador, inimigos destruídos, dano recebido, vidas; "Próxima fase" (não na 3ª).
- [ ] Progresso salvo: "Continuar" no menu aparece com a fase desbloqueada.

## Transições, save e menu
- [ ] Boot → Menu → Jogar → Briefing → Gameplay → Vitória/Game Over → Menu sem erros no Console.
- [ ] Reiniciar fase mantém vidas/pontuação iniciais da fase.
- [ ] Voltar ao menu com timeScale restaurado (música e animações do menu normais).
- [ ] Save inexistente: inicia com padrões; save corrompido (editar o JSON): aviso no Console e padrões.
- [ ] "Reset progress" zera fase/recorde e mantém volumes.

## Áudio
- [ ] Música de menu, por fase e de chefe; efeitos de laser, impacto, explosão, coleta, dano, Ultimate, UI.
- [ ] Sliders Master/Música/Efeitos persistem após reiniciar o app.
- [ ] Nenhum erro quando um slot do AudioLibrary está vazio.

## Controle e navegação por foco
- [ ] Todos os painéis navegáveis por d-pad/setas + confirmar; foco inicial visível.
- [ ] Clique do mouse em área vazia não perde o foco definitivamente.

## Resoluções
- [ ] 1080x1920, 1080x2340 (notch: HUD respeita área segura), 1536x2048 (tablet), 1920x1080 (Editor).
- [ ] Spawns no topo fora da tela em todas; asteroides/inimigos despawnam fora da área.

## Robustez
- [ ] Console sem NullReferenceException em um ciclo completo das 3 fases.
- [ ] Abrir a cena Gameplay diretamente no Editor funciona (contexto cria Save/Audio).
- [ ] Pools não instanciam durante o jogo (Hierarchy estável após o prewarm).
