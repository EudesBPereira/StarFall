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

---

## Versão 1.0 completa — itens adicionais

### Naves e armas (Hangar)
- [ ] As cinco naves aparecem; as bloqueadas mostram o preço e a Nova-X mostra "FINISH THE CAMPAIGN".
- [ ] O holograma gira/flutua e troca ao selecionar outra nave ou arma.
- [ ] Comprar desconta os créditos e equipa na hora; sem saldo o botão fica inativo e toca o som de erro.
- [ ] Os atributos exibidos incluem os upgrades comprados.
- [ ] A nave e a arma equipadas são as usadas na partida seguinte (confira no briefing e no HUD).

### Armas em jogo
- [ ] Laser Duplo dispara dois canos; Spread Shot cobre o leque; Plasma é lento e forte.
- [ ] Railgun atravessa vários inimigos enfileirados.
- [ ] Mísseis curvam até o alvo e causam dano em área.
- [ ] Canhão de Energia carrega (barra no canto inferior esquerdo) e dispara sozinho ao encher.
- [ ] Acertos críticos mostram "CRIT" e um projétil maior.

### Upgrades
- [ ] Cada nó sobe até 5 e mostra os quadradinhos preenchidos.
- [ ] A partir do nível 4 o custo inclui componentes e o botão bloqueia sem eles.
- [ ] Comprar um upgrade de casco/escudo aumenta as barras na partida seguinte.
- [ ] Regeneração de escudo recompõe o escudo após alguns segundos sem levar dano.

### Fases 4 e 5
- [ ] Fase 4: as turrets param na linha e disparam anéis; o fundo mostra a muralha da fortaleza.
- [ ] Fase 5: fundo de colmeia, ondas densas, todos os tipos de inimigo.
- [ ] Os cinco briefings mostram objetivos e a nave/arma equipadas.

### Chefes novos
- [ ] Leviathan: os segmentos seguem a cabeça sem "engessar" e somem na morte.
- [ ] Hive Queen: invoca minions, respeita o limite e para de invocar com a tela cheia.
- [ ] Omega Core: muda de forma três vezes, fica invulnerável durante a troca e volta a atacar.
- [ ] Widow: a teia deixa a nave lenta e o efeito expira (indicador no canto inferior direito).
- [ ] Reaper Wing: as investidas são legíveis e não prendem o jogador contra a borda.

### Modos extras
- [ ] Sobrevivência: as ondas continuam indefinidamente e ficam mais difíceis; mini-chefe na onda 8.
- [ ] Boss Rush: só aparece depois de derrotar um chefe e enfileira os quatro.
- [ ] Desafio Diário: reiniciar a fase mantém a mesma sequência de ondas do dia.
- [ ] Game over em qualquer modo grava a linha no Ranking do modo certo.

### Progressão, ranking e conquistas
- [ ] Vitória e derrota mostram créditos, XP e componentes ganhos.
- [ ] O menu mostra nível de piloto, créditos e componentes atualizados.
- [ ] Ranking: as quatro abas listam apenas o modo escolhido, ordenado por pontuação.
- [ ] Conquistas disparam o aviso na tela e ficam marcadas na lista.
- [ ] "Sobrevivente" só é concedida quando a fase termina sem perder vida.

### Feedback novo
- [ ] Com o casco abaixo de 30 %, a nave solta fumaça, o HUD pisca "HULL CRITICAL" e toca o alarme.
- [ ] Explosões e impactos emitem faíscas de partículas.
- [ ] O som do motor acompanha a velocidade da nave.
- [ ] Cada fase tem música e ambiente próprios; o chefe troca a música e ela volta ao normal depois.
- [ ] Splash: cartão do estúdio, depois o título, sem travar.

---

## Fase B do Plano Mestre — Zona de Risco, Overdrive, Graze

### Zona de Risco
- [ ] Aproximar-se de um drone muda o rótulo para "RISK ALERT! x2" e a borda da tela fica amarela.
- [ ] Com vários inimigos e projéteis perto, o estado sobe para DANGER (laranja) e EXTREME (magenta).
- [ ] Ao se afastar, o estado demora um instante para cair (sem piscar entre estados).
- [ ] Um abate em EXTREME mostra "+pontos" em magenta e a pontuação sobe pelo multiplicador.
- [ ] O som de subida/descida de risco toca uma vez por transição.
- [ ] Na fase 1, as dicas "RISK ALERT", "DANGER", "EXTREME" aparecem só na primeira vez.
- [ ] Sem perda de quadros visível com a tela cheia (sensor a 10 Hz).

### Overdrive
- [ ] A barra OVERDRIVE enche em DANGER/EXTREME e drena em SAFE.
- [ ] Ao encher, ativa sozinha: som, camada musical rítmica, propulsor pulsando, vinheta ciano/magenta.
- [ ] Durante o Overdrive a cadência sobe (Federação) e os itens são atraídos para a nave.
- [ ] Levar dano corta o tempo restante pela metade; morrer zera tudo.
- [ ] A camada musical some suavemente ao terminar.
- [ ] Com "Reduced effects" ligado, a vinheta fica bem mais discreta.
- [ ] Phantom em Overdrive mostra mais acertos "CRIT".

### Graze
- [ ] Um projétil passando rente sem acertar mostra "GRAZE", toca o som e conta no HUD.
- [ ] Um projétil que acerta a nave não conta graze.
- [ ] Durante a invulnerabilidade de respawn nenhum graze é contado.
- [ ] O mesmo projétil nunca conta duas vezes.
- [ ] Desligar "Graze feedback" remove o som e o texto, mas os pontos continuam.

### Resultado e ranks
- [ ] A tela de vitória mostra RANK, a decomposição (abates, graze, objetivo, tempo, sem dano) e melhor risco.
- [ ] Terminar sem levar dano soma 5 000 e terminar abaixo do par soma bônus de tempo.
- [ ] Créditos exibidos como BASE + RANK + RISK (+ FIRST CLEAR na primeira vez) e não mudam com a pontuação.
- [ ] Refazer a fase não paga FIRST CLEAR de novo.
- [ ] A seleção de missão mostra o melhor rank quando implementado na lista (hoje só a pontuação).

### Configurações
- [ ] "Screen shake" em 0 elimina o tremor; em 1 volta ao normal.
- [ ] "Show hitbox" desenha o anel branco no centro da nave.
