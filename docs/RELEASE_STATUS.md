# Starfall Defense — estado do projeto e caminho até as lojas

> **Direção do produto:** desde 2026-09-05 o [Plano Mestre STAR RISK](product/IMPLEMENTATION_MASTER_PLAN.md) define o que é produto completo (Zona de Risco, Overdrive, graze, facções, dez fases, monetização ética). Este documento descreve o estado técnico; a priorização oficial (P0–P3) está no plano e o quadro de acompanhamento em `BACKLOG.md`.

Documento único com **tudo que está implementado** e **tudo que falta para publicar** na Google Play e na App Store.

- Versão atual: `0.2.0` (Android `versionCode 2`, iOS `buildNumber 2`)
- Engine: Unity 6000.3.23f1 (6.3 LTS), Built-in Render Pipeline, 2D
- Identificador: `com.starfallteam.starfalldefense` (Android e iOS)
- Fonte de verdade do design: `document.md` (GDD v1.0) — ver `docs/GDD_COMPLIANCE.md`
- Última validação: 65 testes de lógica pura, 65 EditMode, 3 PlayMode e build Android IL2CPP/ARM64 gerando APK de 25,7 MB, todos sem erro

---

# Parte 1 — O que está implementado

## Números do projeto

| Item | Quantidade |
|---|---|
| Scripts C# (sem testes) | 101 arquivos, ~11 800 linhas |
| Regras puras testáveis (`Scripts/Logic`) | 13 arquivos |
| Arquivos de teste | 8 |
| ScriptableObjects gerados | 87 |
| Sprites placeholder gerados | 41 |
| Prefabs | 15 |
| Cenas | 3 (Boot, MainMenu, Gameplay) |
| Shaders próprios | 4 |

## Jogabilidade

**Nave e controle.** Movimento em oito direções com aceleração e desaceleração, limite da área visível e inclinação lateral. Três esquemas de entrada na mesma camada: arrastar na tela, teclado e controle. Disparo automático configurável.

**Dano.** Escudo absorve antes do casco, invulnerabilidade temporária após levar dano e no respawn, acerto crítico com chance e multiplicador por nave, e tipos de dano (laser, plasma, cinético, explosivo, energia, contato). Contrato genérico `IDamageable` com camadas de colisão dedicadas.

**Vidas e continuidade.** Três vidas iniciais, respawn com invulnerabilidade, game over ao esgotar, e estado crítico do casco abaixo de 30 % com fumaça, alarme sonoro e aviso piscando no HUD.

## Conteúdo

**Cinco naves jogáveis**, cada uma com casco, escudo, regeneração, velocidade, aceleração, chance e multiplicador de crítico, e multiplicadores de dano e de Ultimate próprios.

| Nave | Perfil | Como se obtém |
|---|---|---|
| SF-01 Vanguard | Balanceada | Inicial |
| SF-02 Falcon | Rápida, frágil | 2 500 créditos |
| SF-03 Titan | Resistente, lenta, regenera escudo | 4 000 créditos |
| SF-04 Phantom | Especialista em crítico (25 % / x3) | 6 000 créditos |
| Nova-X | Híbrida experimental: teto de score alto, defesa fina, Overdrive exigente (reequilibrada, D-023) | Concluir a campanha |

**Sete armas**, todas com cinco níveis de evolução dentro da fase.

| Arma | Característica | Custo |
|---|---|---|
| Laser | Tiro base, vira leque nos níveis altos | Inicial |
| Laser Duplo | Dois canos sempre | 1 500 |
| Plasma | Projétil grande, lento, dano alto | 3 000 |
| Spread Shot | Leque de 3 a 8 tiros | 2 500 |
| Railgun | Atravessa até 3 inimigos, 32 u/s | 4 500 |
| Mísseis | Teleguiados com dano em área | 5 000 |
| Canhão de Energia | Carrega 1,2 s para x4 de dano | 7 000 |

**Inimigos.** Drone, Interceptor, Bomber com bombas de área, Kamikaze que persegue e explode, Shield com escudo próprio, Turret que fixa posição e dispara anéis, drone de suprimento com drop garantido e asteroide destrutível. Mais cinco versões Elite que valem 500 pontos, dão um componente e sobrevivem à Ultimate.

**Cinco mini-chefes.** Sentinel-X com ataque giratório, Widow com teias que deixam a nave lenta, Reaper Wing com investidas rápidas, e as variantes reforçadas Sentinel-X Mk.II e Widow Prime.

**Quatro chefes.** The Destroyer com canhões laterais, mísseis e laser frontal telegrafado. Leviathan com corpo de oito segmentos que seguem a cabeça. Hive Queen que invoca minions com limite de tela. Omega Core com quatro fases e três transformações de forma.

**Cinco fases.** Setor Orbital com satélites destruídos e a Terra ao fundo, Campo de Asteroides com rochas móveis, Nebulosa Violeta com visibilidade reduzida, Fortaleza Mecânica com grade de canhões e Núcleo da Colmeia como assalto final.

**Seis power-ups** nas cores do GDD: poder da arma, escudo, dano temporário, velocidade, energia da Ultimate e invencibilidade.

## Progressão e sistemas

**Pontuação.** Comum 100, elite 500, mini-chefe 2 500 e chefe 10 000, com multiplicador de x1 a x10 que sobe a cada quatro abates sem dano e zera ao levar dano. Pontos concedidos uma única vez por morte.

**Ultimate.** Barra que enche com abates, escalada por nave e por upgrades, ativável só quando cheia. Elimina inimigos comuns, causa dano configurável a elites e chefes e limpa os projéteis inimigos.

**Economia.** Cada partida rende créditos por base da fase, rank, risco limitado e primeira conclusão (separados do score, D-027), mais experiência e componentes. Nível de piloto por curva quadrática.

**Árvore de upgrades permanentes.** Dez nós em cinco níveis cada, cobrindo dano, cadência, alcance, capacidade e regeneração de escudo, casco, velocidade, aceleração, recarga e potência da Ultimate. Os últimos níveis também custam componentes.

**Modos de jogo.** Campanha, Sobrevivência com ondas procedurais infinitas e mini-chefe a cada oito ondas, Boss Rush com os quatro chefes em sequência, e Desafio Diário cuja semente vem da data, de modo que todos jogam a mesma sequência no dia.

**Ranking local.** Dez melhores por modo, com nave, fase ou onda e data. Sem rede.

**Conquistas.** As cinco do GDD, avaliadas por regra pura e persistidas no save, com aviso animado na tela.

**Save.** Versão 2 em JSON no diretório persistente, com migração automática da versão 1, tolerância a arquivo ausente ou corrompido e opção de redefinir progresso.

## Telas

Splash com cartão do estúdio e título. Menu principal com Jogar, Hangar, Melhorias, Ranking, Configurações, Créditos e Sair. Seleção de missão com as cinco fases e os três modos extras. Hangar com prévia em holograma e comparação de atributos. Briefing com objetivos e equipamento. HUD nas cinco posições do GDD. Pausa, vitória com recompensas, game over e créditos.

## Apresentação

Quatro shaders próprios: holograma para o Hangar, escudo com pulso, aditivo para o brilho neon e feixe de energia para os lasers de chefe. Partículas de faísca com pooling e fumaça no estado crítico. Fundo procedural por fase com camadas de estrelas, silhueta e névoa. Áudio sintetizado em tempo real: 25 efeitos, 10 trilhas e 5 ambientes, todos substituíveis por assets finais sem tocar em código.

## Qualidade

Sem `FindObjectOfType` em gameplay. Pooling para projéteis, inimigos, explosões, partículas e itens. Regras de jogo em classes puras sem dependência da engine, testadas dentro e fora do Unity. Cenas, prefabs, sprites e dados gerados por script, o que torna o projeto reprodutível a partir do código.

---

# Parte 2 — O que falta para publicar

Nada aqui é trabalho de código de jogo: é empacotamento, contas, arte final e conformidade. Ordenado por bloqueio.

## Bloqueadores absolutos (sem isso a loja rejeita)

### 1. Arte final substituindo os placeholders

Hoje todos os sprites são formas brancas geradas por código e tintadas em tempo real. As lojas não rejeitam por isso, mas o jogo não é vendável assim. A troca não exige código: cada definição de nave, inimigo, chefe e arma tem um campo de sprite.

**Necessário:** naves, inimigos, chefes, projéteis, power-ups, fundos e elementos de interface.

### 2. Áudio final

Todos os sons são sintetizados no boot. Substituir preenchendo os espaços em `AudioLibrary.asset`; há um interruptor para desligar os placeholders. O GDD pede cinco estilos de trilha, mais chefe e chefe final.

**Cuidado jurídico:** só use áudio com licença clara para uso comercial, e guarde os comprovantes.

### 3. Ícone do aplicativo

Nenhum ícone está configurado: são 18 espaços vazios entre Android e iOS.

| Plataforma | Exigência |
|---|---|
| Android | Ícones adaptativos, camadas de 432x432 |
| Google Play (listagem) | 512x512 PNG de 32 bits |
| iOS | Conjunto completo, com 1024x1024 sem transparência e sem cantos arredondados |

### 4. Assinatura de release do Android

O projeto está com assinatura de depuração. A Play Store recusa APK assinado em modo debug.

**A fazer:** criar um keystore de release, guardar a senha em local seguro fora do repositório, ativar o keystore personalizado nas configurações do player e habilitar o Play App Signing na primeira publicação.

### 5. Android App Bundle em vez de APK

O script de build gera APK. A Play Store exige App Bundle para aplicativos novos.

**A fazer:** ativar a opção de app bundle no script de build e gerar um `.aab`. O código já está pronto; é uma linha.

### 6. API alvo do Android

A configuração está em "automática". A Play Store exige um nível de API alvo recente e o valor sobe todo ano, então precisa ser fixado explicitamente e revisado a cada ciclo.

**A fazer:** fixar o nível alvo no valor que a Play exigir na data do envio e rebuildar. O mínimo já está em 25, o que cobre praticamente todos os aparelhos ativos.

### 7. Contas de desenvolvedor

| Loja | Custo | Observações |
|---|---|---|
| Google Play | 25 dólares, pagamento único | Contas pessoais criadas recentemente exigem teste fechado com 12 testadores por 14 dias antes de liberar produção |
| Apple | 99 dólares por ano | Necessária mesmo para aplicativo gratuito |

### 8. Máquina com macOS para o iOS

O build iOS gera um projeto Xcode e só roda em macOS. Sem um Mac, ou serviço de build em nuvem, não há versão para iPhone.

**A fazer:** compilar no Xcode, configurar time de desenvolvimento, perfis de provisionamento e enviar via App Store Connect.

### 9. Política de privacidade

As duas lojas exigem a URL de uma política de privacidade acessível publicamente, mesmo quando o aplicativo não coleta nada.

**Situação favorável:** o jogo é totalmente offline e não coleta dado nenhum. A política pode declarar exatamente isso, o que simplifica os formulários das duas lojas.

### 10. Formulários de conformidade

| Loja | Formulário | Resposta esperada no estado atual |
|---|---|---|
| Google Play | Seção de segurança de dados | Nenhuma coleta, nenhum compartilhamento |
| Google Play | Classificação indicativa por questionário | Violência fantasiosa contra naves e robôs |
| Apple | Rótulos de privacidade | Nenhum dado coletado |
| Apple | Classificação etária | Violência de desenho, infrequente |
| Ambas | Declaração de anúncios | Não contém anúncios |

Se um dia entrarem anúncios, análise de uso ou compras, todos esses formulários mudam.

## Necessário para a listagem

### Materiais gráficos e texto

| Item | Google Play | App Store |
|---|---|---|
| Capturas de tela do celular | 2 a 8, proporção do aparelho | Obrigatórias para 6,7 e 6,5 polegadas |
| Capturas de tablet | Recomendadas | Obrigatórias se aceitar iPad |
| Banner de destaque | 1024x500 | Não se aplica |
| Vídeo de prévia | Opcional, via YouTube | Opcional, gravado no aparelho |
| Descrição curta | Até 80 caracteres | Subtítulo, até 30 |
| Descrição completa | Até 4 000 caracteres | Até 4 000 |
| Palavras-chave | Não se aplica | Até 100 caracteres |

O jogo está em inglês na interface. Decidir se a listagem sai em português, inglês ou ambos.

### Testes em aparelhos reais

O jogo nunca rodou em um celular: as validações foram automatizadas e em modo headless.

**Mínimo recomendado:** um Android de entrada e um intermediário, mais um iPhone. Verificar taxa de quadros com muitos projéteis, legibilidade em tela pequena, precisão do arrasto, comportamento do recorte de tela e o consumo de bateria.

O roteiro manual está em `docs/QA_CHECKLIST.md`, com uma seção específica para os recursos desta versão.

## Recomendado antes de lançar

**Playtest de balanceamento.** Os valores marcados com aviso em `docs/BALANCING.md` nunca foram jogados por uma pessoa. Suspeitas registradas: Laser Duplo e Mísseis fortes demais pelo preço, e a Phantom possivelmente melhor que a Nova-X lendária.

**Ciclo de economia.** Verificar quantas partidas são necessárias para comprar cada nave e maximizar os upgrades. Maximizar tudo custa cerca de 63 000 créditos e 30 componentes, um número escolhido sem dados de jogo real.

**Splash da Unity.** A licença Personal obriga a exibir a marca da Unity na abertura. Remover exige licença Pro.

**Vibração.** O GDD não pede, mas é padrão do gênero em celular.

**Tutorial.** A fase 1 tem instruções no briefing; observar se um jogador novo entende o arrasto sem ajuda.

**Nome e marca.** Conferir se "Starfall Defense" está livre nas duas lojas antes de fechar a arte.

## Não bloqueiam o lançamento

Continuam no backlog e podem entrar depois: coop online, ranking global, clãs, eventos semanais, passe de temporada, editor de fases e integrações com Steam. Ranking e conquistas já existem em versão local, então migrar para servidor troca só a fonte de dados.

---

# Resumo executivo

| Frente | Situação |
|---|---|
| Jogo | Completo conforme o GDD. Cinco fases, quatro chefes, cinco naves, sete armas, três modos extras, progressão e conquistas |
| Código | Compila sem erros, 133 testes automatizados passando, build Android funcionando |
| Arte e áudio | Placeholders funcionais em toda parte. É a maior frente de trabalho restante |
| Empacotamento | Falta ícone, assinatura de release, app bundle e API alvo fixa. Trabalho de horas, não de semanas |
| Contas e conformidade | Nada iniciado. Depende de decisão comercial e de pagamento das taxas |
| iOS | Bloqueado por falta de macOS |
| Validação em aparelho | Nunca executada |

**Caminho mais curto até a Play Store:** arte e áudio finais, ícone, keystore de release, app bundle com API alvo fixa, política de privacidade publicada, conta de desenvolvedor, teste fechado com 12 pessoas por 14 dias, então produção.

**Caminho até a App Store:** o mesmo trabalho de arte e ícone, mais acesso a um Mac com Xcode e a assinatura anual da Apple.

---

# Adendo — Fase B do Plano Mestre (núcleo STAR RISK)

Implementado após a redação original deste documento:

- **Zona de Risco** com cinco estados (Seguro, Alerta, Perigo, Extremo, Overdrive), sensor por distância com histerese, multiplicador de score x1 a x8, vinheta de tela, rótulo acessível e sons de transição.
- **Overdrive** conquistado por habilidade: medidor, ativação automática, 8 s de duração, penalidade por dano, reações por facção (cadência, crítico, regeneração), atração de itens, carga acelerada do Ultimate e camada musical.
- **Graze** com um registro por projétil, confirmação de saída sem dano, pontos escalados pelo risco e opção de acessibilidade.
- **Score composto** conforme o plano: abates × combo × risco + graze + objetivo + tempo + sem dano, com **ranks D a SSS** por fase.
- **Economia separada do score:** créditos por base de fase, rank, risco limitado e primeira conclusão. Linhas do placar carimbam seed, versão de balanceamento e revive.
- **Nova-X reequilibrada** e **facções** definidas em dados (Federação: Vanguard, Falcon, Titan, Nova-X; Cyber: Phantom; Biomecânica: fase C).
- **Configurações novas:** intensidade do screen shake, efeitos reduzidos, feedback de graze e exibição da hitbox.
- **Tutorial contextual** do laço de risco na fase 1.

Continua pendente, na ordem do plano: confirmação do nome (SR-PROD-001), teste em aparelho real (SR-QA-001), nave Biomecânica e especiais por facção (fase C), fatia de arte final (fase D), dez fases e dez chefes (fase E), monetização, analytics e privacidade (fase F), e preparação de loja (fase G).

# Adendo — Fase C do Plano Mestre (facções)

- **Três facções jogáveis com kit próprio:** Federação (Vanguard, Falcon, Titan, Nova-X) com passiva de precisão e Orbital Strike; Biomecânica (BX-01 Symbiont) com Spore Launcher, lifesteal por abate e Spore Swarm; Corporação Cyber (CX-7 Nexus e Phantom) com marcação de alvos, drone companheiro e EMP Burst.
- **Sete naves e oito armas** no Hangar, que agora mostra facção, Ultimate e passiva de cada nave.
- **Decisões do responsável:** nome mantido como Starfall Defense (D-029); validação em aparelho adiada para o produto completo (D-030).

Continua pendente, na ordem do plano: fatia de arte final (fase D), dez fases e dez chefes com builds temporárias (fase E), monetização, analytics e privacidade (fase F) e preparação de loja (fase G). O teste em aparelho fica para o fim, por decisão.
