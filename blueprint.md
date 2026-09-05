Você atuará como Tech Lead, Game Developer Sênior, Game Designer técnico e QA de um projeto desenvolvido em Unity 6.

Seu objetivo é analisar o repositório atual e desenvolver o MVP jogável do game STARFALL DEFENSE, um shoot'em up 2D para Windows, seguindo rigorosamente os requisitos, prioridades, arquitetura e critérios de aceite descritos neste prompt.

==================================================
1. REGRAS DE EXECUÇÃO
==================================================

1. Antes de alterar qualquer arquivo:
   - Inspecione toda a estrutura do repositório.
   - Identifique a versão do Unity registrada em ProjectSettings/ProjectVersion.txt.
   - Identifique os pacotes instalados em Packages/manifest.json.
   - Localize cenas, scripts, prefabs, ScriptableObjects, assets, testes e documentação existentes.
   - Preserve implementações funcionais já existentes.
   - Não substitua arquivos sem analisar dependências e referências.

2. Trabalhe incrementalmente:
   - Divida o trabalho em etapas pequenas e verificáveis.
   - Implemente primeiro um vertical slice jogável.
   - Após cada etapa, revise possíveis erros de compilação, referências ausentes, conflitos de namespace e dependências.
   - Não implemente funcionalidades da versão 2.0 antes de concluir o MVP.

3. Não invente que executou o Unity Editor, testes ou builds caso essas ações não estejam disponíveis no ambiente.
   - Diferencie claramente o que foi implementado.
   - Informe o que foi validado por inspeção estática.
   - Informe o que ainda deverá ser validado no Unity Editor.

4. Não utilize assets protegidos, copiados ou extraídos de jogos de referência.
   - Space Invaders, Galaga, Gradius, R-Type e Sky Force Reloaded são apenas referências de gênero e experiência.
   - Use assets próprios, primitivas, sprites temporários ou placeholders claramente identificados.
   - Não inclua logotipos de terceiros como arquivos do projeto.

5. Evite dependências externas desnecessárias.
   - Priorize recursos nativos da Unity 6.
   - Utilize C# organizado, modular, documentado e preparado para expansão.
   - Não use APIs obsoletas quando houver alternativas compatíveis com Unity 6.

6. Não concentre toda a lógica em um único GameManager.
   - Separe responsabilidades.
   - Prefira composição a heranças profundas.
   - Use eventos ou interfaces para reduzir acoplamento.
   - Evite chamadas frequentes a FindObjectOfType, FindAnyObjectByType e GameObject.Find durante o gameplay.
   - Não use alocação excessiva de memória em Update.
   - Use object pooling para projéteis, inimigos, explosões e itens coletáveis.

7. Quando um requisito estiver ambíguo:
   - Adote a solução mais simples para o MVP.
   - Registre a decisão em docs/DECISIONS.md.
   - Não bloqueie a implementação por detalhes cosméticos.
   - Faça perguntas somente se a ambiguidade impedir tecnicamente a continuação.

==================================================
2. VISÃO DO PRODUTO
==================================================

Nome: STARFALL DEFENSE
Versão inicial: MVP 1.0
Gênero: Shoot'em Up 2D
Plataforma inicial: Windows PC
Engine: Unity 6, projeto 2D
Público: jogadores casuais e fãs de arcade
Estilo: ficção científica futurista, neon e influência cyberpunk

Premissa:
No ano de 2237, a humanidade enfrenta uma invasão promovida por uma civilização mecânica conhecida como The Swarm. O jogador pilota a nave experimental SF-01 Vanguard e deve destruir as forças invasoras antes que alcancem o sistema solar.

Pilares:
- Combate rápido e responsivo.
- Progressão constante.
- Melhorias de poder durante as fases.
- Mini-chefes e chefes com padrões identificáveis.
- Pontuação e multiplicador que incentivem replay.
- Clareza visual mesmo com muitos projéteis na tela.

==================================================
3. ESCOPO OBRIGATÓRIO DO MVP
==================================================

Implemente somente o seguinte escopo como prioridade:

- 1 nave jogável: SF-01 Vanguard.
- 1 arma principal: laser simples.
- 5 tipos de inimigos.
- 1 mini-chefe: Sentinel-X.
- 1 chefe principal: The Destroyer.
- 3 fases jogáveis.
- Sistema de vidas.
- Escudo e integridade da nave.
- Sistema de pontuação.
- Multiplicador de pontuação de x1 até x10.
- Power-ups.
- Barra de energia e Ultimate Skill.
- Menu principal.
- Pausa.
- HUD.
- Tela de vitória.
- Tela de Game Over.
- Configurações básicas de áudio.
- Música e efeitos sonoros por meio de placeholders quando os assets finais não existirem.
- Salvamento local do progresso e das configurações essenciais.
- Suporte inicial a teclado e controle, se o Input System estiver disponível no projeto.

Não implementar nesta etapa:
- Coop online.
- Ranking global.
- Clãs.
- Battle Pass.
- Eventos semanais.
- Editor de fases.
- Integrações com Steam.
- Cloud Save.
- Compras.
- Serviços online.
- Modos Sobrevivência, Boss Rush e Desafio Diário.
- As cinco naves jogáveis.
- A árvore completa de progressão permanente.

Esses itens devem entrar apenas no backlog futuro.

==================================================
4. PREMISSAS DO GAMEPLAY
==================================================

Caso o projeto ainda não determine essas características, utilize:

- Orientação em paisagem, proporção base 16:9.
- Câmera ortográfica.
- Nave posicionada inicialmente na região inferior da tela.
- Inimigos entrando principalmente pela parte superior.
- Movimento do jogador em oito direções.
- Movimento suave, com limites determinados pela área visível da câmera.
- Disparo principal contínuo enquanto o comando estiver pressionado.
- Teclado:
  - WASD e setas direcionais para movimento.
  - Espaço para disparar.
  - Tecla E para Ultimate.
  - Esc para pausar.
- Controle:
  - Analógico esquerdo ou direcional para movimento.
  - Botão principal para disparo.
  - Botão secundário para Ultimate.
  - Start/Menu para pausa.
- Todas as teclas devem ficar centralizadas em uma camada de entrada que permita remapeamento futuro.

Crie valores iniciais como dados configuráveis, não como números espalhados pelos scripts.

==================================================
5. MECÂNICAS DO JOGADOR
==================================================

A nave SF-01 Vanguard deve possuir:

- Movimento responsivo em oito direções.
- Aceleração e desaceleração suaves.
- Restrição aos limites da câmera.
- Três vidas no início de uma nova campanha.
- Escudo que absorve dano antes do casco.
- Integridade do casco.
- Curto período de invulnerabilidade após receber dano.
- Feedback visual ao sofrer dano.
- Destruição da nave quando a integridade chegar a zero.
- Consumo de uma vida e respawn quando ainda houver vidas.
- Game Over quando não houver vidas restantes.
- Laser simples como arma inicial.
- Cadência, dano, velocidade e tempo de vida dos projéteis configuráveis.
- Barra de energia carregada ao destruir inimigos.
- Ultimate ativável somente quando a energia estiver cheia.
- Ultimate que elimine inimigos comuns visíveis e cause dano configurável a elites, mini-chefe e chefe.

Evite estados inconsistentes durante respawn, pausa, vitória ou Game Over.

==================================================
6. SISTEMA DE DANO
==================================================

Crie um contrato genérico de dano, por exemplo IDamageable, ou uma solução equivalente.

O sistema deve suportar:

- Dano ao jogador.
- Dano a inimigos.
- Dano a mini-chefes e chefes.
- Escudos.
- Invulnerabilidade temporária.
- Morte.
- Pontuação atribuída somente uma vez.
- Origem do dano.
- Possibilidade futura de dano crítico, perfuração e tipos de dano.

Colisões devem utilizar camadas e matriz de colisão apropriadas para evitar verificações desnecessárias.

==================================================
7. INIMIGOS DO MVP
==================================================

Implemente cinco tipos:

1. Drone
   - Vida baixa.
   - Movimento vertical ou em formação simples.
   - Baixa frequência de ataque.
   - Vale 100 pontos.

2. Interceptor
   - Movimento mais rápido.
   - Mudanças laterais.
   - Ataques frequentes.
   - Vale 100 pontos no MVP, salvo configuração diferente nos dados.

3. Bomber
   - Movimento lento.
   - Mais resistência.
   - Dispara projéteis maiores ou explosivos.
   - Vale 100 pontos no MVP, salvo configuração diferente nos dados.

4. Kamikaze
   - Identifica a posição do jogador.
   - Persegue a nave.
   - Causa dano por contato.
   - É destruído após o impacto.
   - Vale 100 pontos.

5. Shield
   - Possui escudo próprio.
   - O escudo deve ser destruído antes do casco.
   - Deve apresentar feedback visual distinto.
   - Vale 100 pontos.

Todos os atributos devem ser configuráveis por ScriptableObjects ou estrutura de dados equivalente.

Crie uma base compartilhada somente para comportamentos realmente comuns. Estratégias de movimento e ataque devem poder ser combinadas ou substituídas sem duplicação extensa de código.

==================================================
8. MINI-CHEFE E CHEFE
==================================================

Mini-chefe Sentinel-X:
- Entrada destacada.
- Barra de vida própria.
- Movimento lateral.
- Ataque giratório ou padrão circular de projéteis.
- Ao menos duas fases comportamentais simples.
- Vale 2.500 pontos.
- Deve ser enfrentado na Fase 2 ou em uma posição configurável pela definição da fase.

Chefe The Destroyer:
- Entrada cinematográfica curta sem retirar o controle por tempo excessivo.
- Barra de vida.
- Canhões laterais.
- Mísseis.
- Laser frontal com antecipação visual.
- Ao menos três padrões de ataque.
- Mudança de comportamento conforme a vida diminui.
- Vale 10.000 pontos.
- Deve encerrar a Fase 3 no MVP.

Projéteis de chefes devem ser legíveis, possuir antecipação visual quando necessário e evitar dano inevitável.

==================================================
9. FASES E ONDAS
==================================================

Implemente três fases:

Fase 1, Setor Orbital:
- Tutorial contextual mínimo.
- Introdução a Drone, Interceptor e power-ups.
- Fundo com espaço próximo à Terra e satélites destruídos, usando placeholders.
- Encerramento por objetivo ou onda final.

Fase 2, Campo de Asteroides:
- Introdução a Bomber, Kamikaze e Shield.
- Obstáculos ou asteroides móveis simples.
- Mini-chefe Sentinel-X.

Fase 3, Nebulosa Violeta:
- Maior densidade de inimigos.
- Visibilidade reduzida de forma moderada, sem prejudicar a leitura de projéteis.
- Mistura dos cinco inimigos.
- Chefe The Destroyer.

Utilize um sistema orientado por dados para definir:
- Ordem das ondas.
- Tipos de inimigos.
- Quantidade.
- Intervalos.
- Posições de spawn.
- Duração.
- Eventos.
- Mini-chefe.
- Chefe.
- Recompensas.

Não codifique todas as ondas diretamente em condicionais nos scripts.

==================================================
10. POWER-UPS
==================================================

Implemente os seguintes power-ups como itens configuráveis:

- Azul: aumenta temporariamente ou por nível o poder do laser durante a fase.
- Verde: recupera escudo.
- Vermelho: concede dano extra temporário.
- Amarelo: aumenta a velocidade temporariamente.
- Roxo: concede energia especial ou uma ativação especial simplificada.
- Branco: concede invencibilidade temporária.

Para o MVP:
- Defina duração, intensidade e chance de drop nos dados.
- Exiba feedback visual e textual curto ao coletar.
- Garanta que efeitos temporários sejam removidos corretamente.
- Defina regras claras para efeitos repetidos, como renovar duração ou limitar acúmulo.
- Registre essas regras em docs/DECISIONS.md.

==================================================
11. PONTUAÇÃO E MULTIPLICADOR
==================================================

Pontuação base:
- Inimigo comum: 100.
- Elite, se utilizada posteriormente: 500.
- Mini-chefe: 2.500.
- Chefe: 10.000.

Multiplicador:
- Começa em x1.
- Aumenta ao destruir inimigos sem sofrer dano.
- Limite máximo de x10.
- Retorna para x1 quando o jogador sofre dano.
- A pontuação concedida deve considerar o multiplicador atual.
- O HUD deve atualizar sem inconsistências.
- Mortes não podem conceder pontos duas vezes.

Crie eventos para que o sistema de pontuação não fique diretamente acoplado a cada inimigo.

==================================================
12. INTERFACE E FLUXO
==================================================

Implemente o fluxo:

Boot/Splash
→ Menu Principal
→ Seleção ou início com Vanguard
→ Briefing da fase
→ Gameplay
→ Vitória ou Game Over
→ Retorno ao menu ou próxima fase

Menu Principal:
- Jogar.
- Continuar, caso exista progresso válido.
- Configurações.
- Créditos.
- Sair.

HUD:
- Superior esquerdo: vidas, integridade e escudo.
- Superior direito: pontuação e multiplicador.
- Centro inferior: energia da Ultimate.
- Inferior esquerdo: arma atual.
- Inferior direito: indicador especial.

Pausa:
- Continuar.
- Configurações.
- Reiniciar fase.
- Voltar ao menu.

Vitória:
- Pontuação.
- Maior multiplicador.
- Inimigos destruídos.
- Dano recebido.
- Vidas restantes.
- Próxima fase ou menu.

Game Over:
- Pontuação final.
- Reiniciar.
- Voltar ao menu.

A interface deve:
- Funcionar com mouse, teclado e controle.
- Possuir navegação por foco.
- Ser legível em 16:9.
- Utilizar TextMeshPro se já estiver disponível.
- Separar apresentação visual da lógica de gameplay.

==================================================
13. ÁUDIO E EFEITOS
==================================================

Crie uma arquitetura simples para:

- Música de menu.
- Música por fase.
- Música de chefe.
- Efeitos de laser.
- Impactos.
- Explosões.
- Coleta de power-up.
- Dano.
- Ultimate.
- Seleção, confirmação e erro na interface.

Configurações:
- Volume geral.
- Música.
- Efeitos.
- Persistência local das preferências.

Na ausência de arquivos finais:
- Crie referências e pontos de integração.
- Utilize placeholders existentes no repositório quando legalmente apropriados.
- Não fabrique arquivos de áudio inválidos.
- Não deixe NullReferenceException quando um clipe não estiver atribuído.

Efeitos visuais mínimos:
- Propulsão da nave.
- Disparo.
- Impacto.
- Explosão.
- Escudo atingido.
- Coleta de power-up.
- Ultimate.
- Entrada de mini-chefe e chefe.

==================================================
14. ARQUITETURA SUGERIDA
==================================================

Adapte a arquitetura à estrutura existente. Se o projeto estiver vazio, considere módulos equivalentes a:

Assets/_Project/
  Art/
  Audio/
  Materials/
  Prefabs/
    Player/
    Enemies/
    Bosses/
    Projectiles/
    PowerUps/
    UI/
  Scenes/
  ScriptableObjects/
    Ships/
    Weapons/
    Enemies/
    Waves/
    Stages/
    PowerUps/
  Scripts/
    Core/
    Input/
    Player/
    Combat/
    Enemies/
    Bosses/
    Waves/
    Scoring/
    PowerUps/
    UI/
    Audio/
    Save/
    Pooling/
  Tests/
    EditMode/
    PlayMode/

Sistemas esperados, caso sejam necessários:
- Game flow ou state machine.
- Scene loader.
- Input abstraction.
- Player controller.
- Weapon controller.
- Health, shield e damage.
- Projectile.
- Object pool.
- Enemy controller.
- Enemy movement e attack strategies.
- Wave director.
- Stage definition.
- Boss phase controller.
- Score manager.
- Power-up system.
- HUD presenter.
- Audio manager.
- Save service.

Evite singletons globais em excesso. Quando utilizar um serviço persistente, documente a justificativa.

==================================================
15. DADOS E BALANCEAMENTO INICIAL
==================================================

Centralize valores de balanceamento em ScriptableObjects ou arquivos de configuração adequados.

Caso não existam valores definidos, escolha números iniciais razoáveis e documente-os em:

docs/BALANCING.md

Inclua:
- Vida do jogador.
- Escudo.
- Velocidade.
- Dano do laser.
- Cadência.
- Vida dos inimigos.
- Dano dos inimigos.
- Chance de power-up.
- Duração dos efeitos.
- Energia recebida por abate.
- Vida e fases dos chefes.
- Intervalos das ondas.

Esses números são ponto de partida, não regras definitivas.

==================================================
16. SALVAMENTO
==================================================

Implemente persistência local simples para:
- Fase desbloqueada.
- Maior pontuação local.
- Configurações de áudio.
- Preferências essenciais.
- Identificador da versão do save.

O sistema deve:
- Lidar com save inexistente.
- Lidar com dados inválidos sem impedir a inicialização.
- Permitir redefinir progresso.
- Não armazenar dados pessoais.
- Não depender de conexão.
- Estar preparado para migração futura.

==================================================
17. TESTES E QUALIDADE
==================================================

Crie testes automatizados para lógica independente do Unity Editor sempre que possível.

Prioridades de teste:
- Aplicação de dano ao escudo antes do casco.
- Morte e consumo de vidas.
- Multiplicador limitado a x10.
- Reset do multiplicador ao sofrer dano.
- Pontuação concedida somente uma vez.
- Ativação da Ultimate somente com energia completa.
- Expiração de power-ups temporários.
- Progressão de fases.
- Carregamento seguro do save.

Faça também uma checklist manual em docs/QA_CHECKLIST.md cobrindo:
- Movimento.
- Disparo.
- Colisões.
- Pausa.
- Respawn.
- Vitória.
- Game Over.
- Transição de cenas.
- Áudio.
- Navegação por controle.
- Diferentes resoluções.
- Ausência de referências nulas.
- Reinício de fase.
- Retorno ao menu.

==================================================
18. CRITÉRIOS DE ACEITE DO VERTICAL SLICE
==================================================

Antes de expandir para as três fases, entregue um vertical slice que permita:

1. Abrir o menu principal.
2. Iniciar uma partida.
3. Controlar a Vanguard em oito direções.
4. Manter a nave dentro da tela.
5. Disparar laser.
6. Enfrentar ao menos Drone e Interceptor.
7. Receber dano no escudo e depois no casco.
8. Perder uma vida e reaparecer.
9. Coletar ao menos um power-up.
10. Acumular pontuação e multiplicador.
11. Carregar e utilizar a Ultimate.
12. Pausar e continuar.
13. Concluir uma onda.
14. Exibir vitória ou Game Over.
15. Retornar ao menu sem erros.

Somente depois desses critérios serem atendidos, avance para:
- Cinco inimigos.
- Sentinel-X.
- The Destroyer.
- Três fases.
- Polimento.

==================================================
19. BACKLOG E ORDEM DE IMPLEMENTAÇÃO
==================================================

Organize o trabalho nesta ordem:

Fase técnica 0:
- Auditoria do repositório.
- Registro de riscos.
- Definição da arquitetura.
- Correção de erros de compilação existentes.

Fase técnica 1:
- Estrutura base.
- Entrada.
- Movimento.
- Câmera.
- Combate.
- Dano.
- Pooling.

Fase técnica 2:
- Inimigos básicos.
- Spawner.
- Ondas.
- Pontuação.
- Multiplicador.

Fase técnica 3:
- Vidas.
- Respawn.
- Power-ups.
- Ultimate.
- HUD.

Fase técnica 4:
- Menu.
- Pausa.
- Vitória.
- Game Over.
- Salvamento.
- Áudio.

Fase técnica 5:
- Cinco inimigos.
- Três fases.
- Sentinel-X.
- The Destroyer.

Fase técnica 6:
- Testes.
- Correções.
- Balanceamento inicial.
- Otimização.
- Build para Windows.
- Documentação.

==================================================
20. DOCUMENTAÇÃO OBRIGATÓRIA
==================================================

Mantenha ou crie:

README.md
- Objetivo do projeto.
- Versão necessária da Unity.
- Como abrir.
- Como executar.
- Controles.
- Estrutura geral.
- Como criar inimigo, onda ou fase.
- Como gerar um build Windows.

docs/ARCHITECTURE.md
- Sistemas.
- Responsabilidades.
- Fluxo de eventos.
- Dependências.
- Motivos das principais escolhas.

docs/DECISIONS.md
- Decisões tomadas para requisitos ambíguos.
- Alternativas consideradas.
- Impactos.

docs/BALANCING.md
- Valores iniciais.
- Justificativas.
- Pontos que exigem playtest.

docs/QA_CHECKLIST.md
- Checklist de validação manual.

docs/BACKLOG.md
- MVP.
- Melhorias posteriores.
- Itens da visão 2.0.
- Débitos técnicos conhecidos.

==================================================
21. FORMATO DA SUA RESPOSTA
==================================================

Comece apresentando:

1. Diagnóstico do repositório.
2. Estado atual do projeto.
3. Principais riscos técnicos.
4. Arquitetura proposta ou identificada.
5. Plano de implementação em ordem.
6. Arquivos que pretende criar ou alterar.
7. Critérios de aceite da etapa atual.

Depois, implemente diretamente a primeira etapa viável.

Ao final de cada ciclo, apresente:

- Resumo do que foi implementado.
- Arquivos criados.
- Arquivos modificados.
- Decisões técnicas.
- Testes adicionados.
- Validações realmente executadas.
- Itens que dependem do Unity Editor.
- Erros ou limitações encontrados.
- Próxima etapa recomendada.

Não responda apenas com orientações genéricas.
Não gere somente pseudocódigo.
Quando o acesso ao repositório permitir, produza implementação real, compilável e integrada ao projeto.
Não declare o MVP como concluído enquanto todos os critérios de aceite obrigatórios não estiverem atendidos.