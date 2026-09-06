# STAR RISK — Plano Mestre de Implementação e Alinhamento do Produto

> Fonte de direção do produto (recebido em 2026-09-05). O GDD (`document.md`) continua sendo a especificação detalhada.
> Decisões de implementação derivadas deste plano ficam em `docs/DECISIONS.md` (D-023 em diante).
>
> **Decisões do responsável do produto sobre este plano:** o nome comercial fica **Starfall Defense** (D-029, §3.1 superado); o teste em aparelho real (§18 P0, SR-QA-001) será feito com o produto completo (D-030).


**Nome interno atual do projeto:** Starfall Defense
**Nome comercial provisório do produto:** STAR RISK
**Versão do produto avaliada:** 0.2.0
**Versão deste documento:** 1.0
**Status:** Diretriz oficial para conclusão do produto
**Plataformas:** Android e iOS
**Engine:** Unity 6000.3.23f1, Unity 6.3 LTS
**Responsável pelo produto:** Eudes Barbosa Pereira
**Tipo de documento:** Plano executivo, funcional, técnico e comercial
**Fonte analisada:** `RELEASE_STATUS.md`

---

## 1. Finalidade deste documento

Este documento define exatamente o que o time deve manter, corrigir, remover, desenvolver, validar e publicar para transformar o projeto atual em um produto comercial completo.

A partir desta versão:

- O projeto deixa de ser tratado como brainstorming.
- As decisões descritas aqui são requisitos de produto.
- Novas funcionalidades não devem ser implementadas sem avaliação de impacto.
- Funcionalidades já criadas, mas fora da proposta principal, não têm prioridade sobre o núcleo do jogo.
- Nenhum item pode ser considerado concluído apenas porque compila.
- Cada item precisa cumprir critérios funcionais, visuais, técnicos e de experiência.
- O jogo não deve ser enviado às lojas antes de validação em aparelhos reais.
- Monetização, analytics e serviços online devem ser tratados como partes do produto, não como atividades posteriores.

## 2. Visão definitiva do produto

STAR RISK é um shoot'em up 2D vertical para dispositivos móveis, com: movimento livre da nave; disparo automático; combate arcade hardcore; pixel art moderna; foco em pontuação e domínio mecânico; fases parcialmente aleatórias; chefes com múltiplas fases; naves de diferentes facções; progressão permanente moderada; builds temporárias dentro das missões; sistema de Zona de Risco; sistema de Overdrive; monetização free-to-play ética; anúncios recompensados opcionais; compra premium para remoção de anúncios; loja de itens cosméticos; rankings que preservam a competição por habilidade.

### 2.1 Promessa principal

> Quanto mais perto do perigo o jogador combate, maior será sua pontuação, sua energia e sua recompensa.

### 2.2 Posicionamento

Não apresentar como: tower defense; shooter casual automático; jogo de sobrevivência como modo principal; jogo baseado em compra de poder; jogo no qual uma nave final invalida todas as anteriores.

Apresentar como: um arcade espacial mobile de alta intensidade em que habilidade, risco e domínio dos padrões definem o resultado.

## 3. Decisões definitivas de produto

### 3.1 Nome

Divergência entre STAR RISK (original) e Starfall Defense (atual). Antes da arte final: pesquisar disponibilidade nas lojas, domínio e redes, conflito de marca; confirmar; aplicar em projeto, identificadores, menus, documentação e loja. Recomendação: manter **STAR RISK**; evitar "Defense" (sugere tower defense).

### 3.2 Orientação

Retrato, proporção principal 9:16.

### 3.3 Plataforma

Android primeiro; iOS após validação, disponibilidade de macOS e configuração da App Store.

### 3.4 Render pipeline

Built-in hoje. Não migrar por preferência; avaliar URP 2D na fatia visual; migrar só com benefício demonstrado (iluminação 2D, Pixel Perfect Camera, shaders, desempenho) e antes da arte final de todas as fases; nunca depois dos shaders e materiais finais aprovados.

### 3.5 Escopo de lançamento

Dez fases; dez chefes; três facções iniciais; ao menos uma nave por facção; Zona de Risco; Overdrive; progressão permanente; builds temporárias; ranks D, C, B, A, S, SS e SSS; loja cosmética; anúncios recompensados opcionais; remoção de anúncios premium; salvamento local; ranking online ou plano claramente marcado como posterior, sem alegar ranking global na loja.

## 4. Estado atual validado

Fundação: 101 scripts, ~11.800 linhas, 13 arquivos de lógica pura, 8 de teste, 87 ScriptableObjects, 41 sprites placeholder, 15 prefabs, 3 cenas, 4 shaders, build Android IL2CPP/ARM64, testes aprovados, pooling, save versionado, cinco fases, quatro chefes, cinco naves, sete armas, campanha/sobrevivência/Boss Rush/diário.

### 4.1 Classificação

Protótipo funcional avançado com conteúdo significativo, ainda não validado como produto comercial: nunca testado em celular; arte e áudio placeholder; diferencial principal não implementado; sem monetização; 5 fases em vez de 10; 4 chefes em vez de 10; facções não estruturadas; identidade comercial inconsistente; balanceamento não validado.

## 5. Lacunas críticas

### 5.1 Zona de Risco — P0, bloqueadora de identidade

Sistema de risco baseado em distância dos inimigos, distância dos projéteis, quantidade de ameaças próximas, permanência em proximidade perigosa, graze e estado atual do jogador.

| Estado | Multiplicador-base | Cor principal |
|---|---:|---|
| Seguro | x1 | Azul neutro |
| Alerta | x2 | Amarelo |
| Perigo | x3 | Laranja |
| Extremo | x5 | Magenta |
| Overdrive | Até x8 | Ciano e magenta |

Critérios de aceite: percepção sem olhar constantemente o HUD; reação estável sem oscilação visual; influencia score; influencia carga do especial ou Overdrive; pode influenciar recompensa dentro de limites; pesos de risco por tipo de inimigo; chefes com zonas de risco por partes; sem perda perceptível de desempenho; testes de lógica; explicação interativa na primeira fase.

### 5.2 Overdrive — P0, bloqueadora de identidade

Estado temporário conquistado por habilidade. O medidor sobe ao permanecer em Perigo ou Extremo, executar graze, eliminar a curta distância, manter combo e destruir partes de chefe sem dano. Desce na zona segura, ao receber dano, quando a nave é destruída e enquanto o Overdrive está ativo.

Benefícios: multiplicador adicional de pontuação; pequeno aumento de cadência ou eficiência própria da facção; atração de itens de pontuação; carga acelerada do Ultimate; feedback audiovisual intensificado.

Restrições: não tornar invencível; não limpar a tela ao ativar; não substituir o Ultimate; não transformar build fraca em vitória; não esconder projéteis com excesso de efeitos.

Critérios de aceite: entrada, estado ativo e encerramento claros; música recebe camada adicional; motor, rastro, HUD e efeitos respondem; feedback visual reduzível nas configurações; funciona em 30 e 60 FPS; receber dano tem consequência clara; as três facções reagem de formas diferentes.

### 5.3 Graze — P1

Ocorre quando um projétil inimigo passa muito próximo da hitbox sem causar dano. Recompensas: pontos; pequena carga de Overdrive; som curto; indicador visual discreto; registro estatístico. Regras: cada projétil concede graze uma vez; projéteis lentos não geram graze repetido; invulnerabilidade não permite exploração ilimitada; graze durante revive não pontua. Critérios: sem registro duplicado; feedback compreensível; área maior que a hitbox e menor que uma zona injusta; opção de acessibilidade visual.

## 6. Facções e naves

**Federação Humana** (militar, precisa, equilibrada): Vanguard, Falcon, Titan. Acertos consecutivos melhoram eficiência; risco melhora precisão e velocidade; Overdrive melhora disciplina de tiro e recarga.

**Biomecânica Alienígena** (orgânica, regenerativa, adaptativa, controle de multidões): ao menos uma nave nova; arma de esporos ou projéteis vivos; absorção ou regeneração limitada; especial de enxame.

**Corporação Cyber** (neon, precisão, drones, marcação): ao menos uma nave nova; drone assistente; mecânica de marcação; especial EMP.

Destino das naves: Vanguard inicial da Federação; Falcon variação rápida; Titan variação resistente; Phantom → transformar em Cyber, manter como protótipo furtivo humano ou reduzir crítico se dominar o meta; Nova-X deixa de ser "superior em tudo" → híbrida experimental, alto potencial de score, defesa limitada, Overdrive mais exigente, difícil de controlar.

Critérios: silhueta, paleta, arma primária, passiva e Ultimate próprios por facção; reação à Zona de Risco e ao Overdrive; nenhuma facção melhor em tudo; cada nave viável após a campanha; o jogador explica a diferença após uma partida.

## 7. Campanha

Meta: dez fases e dez chefes.

Reaproveitamento: Setor Orbital → fase 1 ou 2; Campo de Asteroides → 1 ou 3; Nebulosa Violeta → 4; Fortaleza Mecânica → 6; Núcleo da Colmeia → 8 ou 9; The Destroyer chefe militar; Leviathan chefe biomecânico; Hive Queen chefe da Colmeia; Omega Core final ou penúltimo.

Estrutura proposta: 1 Cinturão de Ferro; 2 Colônia Silenciosa; 3 Muralha Orbital; 4 Nebulosa Viva; 5 Corredor Pirata; 6 Fábrica Autônoma; 7 Ruínas de Aether; 8 Fenda Dimensional; 9 Núcleo Estelar; 10 Fortaleza Omega.

Cada fase: identidade visual única; paleta própria; trilha ou variação; ao menos três formações exclusivas; ao menos um evento ambiental; ao menos um encontro aleatório; curva clara de intensidade; momento de aprendizado; momento de domínio; preparação antes do chefe; recompensas próprias; rank e pontuação calibrados; seed reproduzível; teste de impossibilidade de padrões.

Chefes faltantes: seis adicionais (ou cinco se Omega Core render dois encontros realmente diferentes). Não conta como chefe separado: mudança visual simples, recoloração, mais vida, Elite sem mecânica nova.

Cada chefe: entrada curta; silhueta reconhecível; duas ou três fases mecânicas; ataques telegrafados; pontos fracos; interação com Zona de Risco; ao menos uma oportunidade clara de alto risco; frenesi final; sequência de derrota; score por partes destruídas; testes de transição; validação sem receber dano por um jogador habilidoso; ausência de padrões inevitáveis.

## 8. Modos de jogo

Campanha: modo principal, P0. Desafio Diário: manter como retenção (mesma seed para todos, sem vantagem paga, ranking separado, validado contra manipulação). Boss Rush: manter, liberar após progressão. Sobrevivência: manter tecnicamente como beta interno até a campanha completa; pós-lançamento se faltar orçamento de arte e balanceamento; não mencionar na loja até validado.

## 9. Pontuação, combo e ranking

### 9.1 Pontuação atual

100 por comum; 500 por Elite; 2 500 por mini-chefe; 10 000 por chefe; multiplicador até x10; aumento a cada quatro eliminações; reinício ao receber dano.

### 9.2 Ajuste necessário

O score final deve combinar:

```text
Pontos base
× multiplicador de combo
× multiplicador de risco
+ graze
+ partes destruídas
+ bônus de objetivo
+ bônus de tempo
+ bônus sem dano
+ bônus de dificuldade
```


### 9.3 Ranks

Implementar D, C, B, A, S, SS e SSS. Cada fase deve possuir limites próprios.

### 9.4 Integridade competitiva

Separar ou identificar resultados com: revive, nave, dificuldade, versão de balanceamento, build, fase, seed, modo, offline ou online.

### 9.5 Ranking online

O ranking local é adequado para protótipo, mas insuficiente para retenção competitiva de longo prazo. Requisito recomendado para lançamento ou primeira atualização: ranking online por fase, por nave, por dificuldade, de Desafio Diário, e placar separado para partidas com revive.

Se o ranking online não estiver pronto no lançamento: não anunciar "ranking global"; preservar resultados locais; preparar modelo de dados para migração; não bloquear o lançamento apenas por esse item, desde que a comunicação seja correta.

## 10. Progressão

### 10.1 Estado atual

Melhorias permanentes de dano, cadência, alcance, escudo, regeneração, casco, velocidade, aceleração, recarga e potência de Ultimate.

### 10.2 Risco de design

Melhorias permanentes muito fortes podem trivializar fases iniciais, tornar o ranking dependente de tempo acumulado, invalidar naves frágeis, criar pressão para venda de recursos e aproximar o produto de pay-to-win.

### 10.3 Decisão

Separar dois contextos. **Campanha com progressão:** upgrades funcionam normalmente, dentro de limites moderados. **Score competitivo:** atributos normalizados, ranking por nível de nave ou modo Score Attack com loadout fixo. Recomendação: na versão inicial, ranking por fase, dificuldade, nave e nível; em atualização futura, Score Attack padronizado.

### 10.4 Limites

Nenhum upgrade de dano em escala descontrolada. Nenhum escudo que torne a nave praticamente invulnerável. O primeiro mundo continua relevante para score. A progressão amplia escolhas e consistência, não elimina habilidade.

## 11. Economia

### 11.1 Recursos

Manter créditos, experiência e componentes. Adicionar moeda premium ou tokens cosméticos somente com necessidade comprovada. Evitar excesso de moedas.

### 11.2 Entradas

Conclusão de fase, rank, objetivo secundário, chefe, primeira vitória, desafio diário, conquista, Zona de Risco, anúncio recompensado opcional.

### 11.3 Saídas

Desbloqueio de nave, de arma, melhorias permanentes, cosméticos por jogo, evolução do hangar.

### 11.4 Fórmula atual

"Créditos = pontuação ÷ 10" deve ser removida ou limitada: facilita inflação; torna score e economia inseparáveis; jogadores avançados acumulam rápido demais; alterar pontuação muda toda a economia.

### 11.5 Nova abordagem

```text
Recompensa-base da fase
+ bônus de dificuldade
+ bônus de rank
+ bônus de risco limitado
+ bônus de primeira conclusão
+ objetivo secundário
```

A pontuação continua sendo utilizada para ranking, não como conversão direta ilimitada em créditos.

### 11.6 Telemetria necessária

Créditos ganhos por sessão; créditos gastos; tempo para desbloquear cada nave e cada arma; custo médio de upgrade; ponto de abandono; recursos antes e depois de anúncios; percentual que conclui a campanha.

## 12. Monetização definitiva

### 12.1 Objetivo comercial

Receita recorrente sem vender vitória ou prejudicar a credibilidade arcade.

### 12.2 Modelo

Gratuito para baixar e jogar. Receita de anúncios recompensados opcionais, compra premium de remoção de anúncios, cosméticos, pacotes de apoio sem vantagem competitiva e conteúdo visual futuro.

### 12.3 Anúncios recompensados

**Posicionamento 1: Revive.** Uma vez por missão; só por escolha explícita; não aparece automaticamente; retorna com proteção temporária; reduz bônus de missão; marca o score como "com revive"; score vai para placar separado ou é inelegível para o ranking principal.

**Posicionamento 2: Recompensa bônus.** Após a missão: dobrar apenas créditos-base ou bônus limitado; não duplicar Núcleos raros, conteúdo premium nem recompensa de primeira conclusão; não afetar score.

**Posicionamento 3: Caixa bônus.** Recompensa claramente informada; limite diário; sem probabilidade escondida; sem dinheiro real direto para sorteio na primeira versão; preferir créditos, componentes ou cosmético fragmentado.

### 12.4 Frequência

Nenhum anúncio obrigatório durante uma fase, ao abrir o jogo, entre telas sem ação do jogador ou imediatamente após derrota sem consentimento. Limite por tipo; intervalo mínimo; modo offline totalmente jogável.

### 12.5 Produto Premium — "Pacote Comandante"

Remove anúncios não recompensados, se existirem; alternativa automática ou diária a determinados bônus; skin exclusiva; rastro exclusivo; badge de apoiador; tema de hangar. Nenhum aumento de dano, escudo ou score.

Se só houver anúncios voluntários, o pacote deve conceder quantidade limitada diária de bônus equivalentes, substituir o anúncio de revive por um token diário, manter limites iguais e preservar placares separados para revive.

### 12.6 Loja cosmética

Itens: skins, rastros, cores de motor, explosões, efeitos de tiro, molduras, avatares, tema de HUD, tema de hangar. Regras: informar o que alteram; não mudar hitbox; efeitos pagos não escondem projéteis; preservar silhueta; alguns conquistáveis jogando; itens pagos nunca apresentados como necessários.

### 12.7 O que não vender

Dano, escudo, chance crítica, pontuação, multiplicador de risco, Overdrive mais forte, invulnerabilidade competitiva, nave superior em tudo, resultado no ranking, revives ilimitados em placar competitivo.

### 12.8 Compras de recursos

Não implementar na primeira versão. Avaliar apenas após economia validada, retenção medida, ausência de paywall, separação campanha/score, revisão de progressão, controles contra gasto acidental e política de integridade.

### 12.9 Requisitos técnicos de monetização

SDK atrás de interface; anúncios de teste em Development e Staging; consentimento por região; política de privacidade e formulários atualizados; tratamento de indisponibilidade; nenhuma recompensa antes da confirmação; restauração de compras; validação de recibo; preços localizados; eventos de analytics; testes de compra interrompida, reinstalação, offline e conta infantil.

### 12.10 Métricas comerciais

Retenção D1/D7/D30; sessões por jogador; fases por sessão; taxa de conclusão; oferta, aceitação e conclusão de anúncio; receita média por jogador ativo; conversão premium e cosmética; uso de revive e impacto no abandono; economia antes e depois da monetização.

## 13. Arte e identidade visual

### 13.1 Estado atual

Placeholders gerados e tintados; identidade visual não validada.

### 13.2 Direção definitiva

Pixel art moderna; ficção científica; silhuetas fortes; iluminação controlada; partículas pixeladas; contraste entre gameplay e cenário; interface futurista; efeitos intensos sem perda de legibilidade.

### 13.3 Elementos obrigatórios

**Jogador:** silhueta identificável; hitbox clara no modo foco; contorno consistente; motor com feedback de velocidade; dano visível; inclinação.
**Inimigos:** categorias distinguíveis por formato; elites reconhecíveis sem depender só de cor; ataques telegrafados; projéteis inimigos separados dos aliados.
**Chefes:** componentes destrutíveis; mudanças visuais por fase; pontos fracos; entrada; transformação; destruição final.
**Cenários:** parallax; contraste menor que entidades; eventos legíveis; temas únicos; limite de ruído.

### 13.4 Paleta principal

| Uso | Cor |
|---|---|
| Espaço escuro | #07111F |
| Interface | #10283D |
| Energia aliada | #24D6FF |
| Alerta | #FFD166 |
| Perigo | #FF7B35 |
| Extremo | #FF3CAC |
| Dano crítico | #FF4D5A |
| Cura | #5CFF9D |
| Texto | #EAF6FF |

### 13.5 Critérios de aceite visual

Nave visível em explosões; projéteis perigosos distinguíveis em telas pequenas; paleta testada para daltonismo; efeitos de risco compreensíveis sem só cor; interface validada em recortes; arte testada em dispositivo de entrada; pixel art sem escala irregular; capturas comercialmente apresentáveis.

## 14. Áudio

### 14.1 Estado atual — sons e músicas sintetizados são placeholders.

### 14.2 Requisitos

Música do menu; por grupo de fases; de chefe; de chefe final; camadas dinâmicas de risco; camada de Overdrive; som de graze; som de mudança de risco; som de combo; som de Ultimate; alertas de ataque; feedback de escudo e casco; sons próprios das facções.

### 14.3 Critérios

Ataques perigosos com aviso sonoro; áudio não cansativo; disparos automáticos com variação controlada; Overdrive entra e sai suavemente; volumes separados de música, efeitos e interface; licenças arquivadas.

## 15. UX, tutorial e acessibilidade

### 15.1 Tutorial

O briefing não basta até ser validado. Tutorial contextual: arrastar; disparo automático; desviar; aproximar para aumentar risco; graze; Overdrive; selecionar upgrade; Ultimate; score e rank.

### 15.2 Configurações

Sensibilidade; posição relativa da nave; modo canhoto; vibração; intensidade do screen shake; quantidade de partículas; exibição da hitbox; volume de música; volume de efeitos; alto contraste; limite de FPS; qualidade gráfica.

### 15.3 Critérios

Jogador novo entende o movimento sozinho; dedo não esconde a nave; botões respeitam safe area; texto legível; nenhuma informação só por cor; pausa ao perder foco.

## 16. Qualidade técnica

### 16.1 Manter

Pooling; regras puras; ScriptableObjects; save versionado; migração; sem buscas globais; testes EditMode e PlayMode; build IL2CPP ARM64.

### 16.2 Ampliar testes

Zona de Risco; graze; Overdrive; multiplicador combinado; score com revive; elegibilidade de ranking; economia; compras; restauração; anúncio cancelado; anúncio indisponível; fase procedural por seed; transições dos dez chefes; migração futura; Nova-X reequilibrada; cosmético sem alterar hitbox.

### 16.3 Performance

Testar em Android de entrada, intermediário, avançado e um iPhone. Medir FPS, memória, picos de GC, carregamento, temperatura, bateria, máximo de projéteis e partículas, inicialização, retorno após minimizar.

## 17. Publicação e conformidade

### 17.1 Android

Keystore de release com backup; App Bundle; API alvo vigente; ícones adaptativos; ícone de loja; política de privacidade; segurança de dados; declaração de anúncios; produtos de compra; testadores fechados; cobrança em sandbox; screenshots; banner; descrição localizada.

### 17.2 iOS

macOS ou serviço de build; Xcode; certificados; provisionamento; App Store Connect; ícone sem transparência; screenshots; IAP; restauração; rótulos de privacidade; consentimento de rastreamento se exigido; TestFlight.

### 17.3 Política de privacidade

Não pode declarar "nenhuma coleta" após analytics, publicidade, compras, ranking, Cloud Save, identificadores ou diagnóstico. Deve refletir dados, finalidade, compartilhamento, provedores, retenção, direitos, contato e exclusão.

## 18. Priorização oficial

**P0 — identidade e validação:** confirmar nome; testar em aparelho real; Zona de Risco; Overdrive; integrar ao score; reestruturar facções; reequilibrar Nova-X; separar score da economia; tutorial do núcleo; fatia de arte final; validar movimento mobile; validar desempenho.

**P1 — produto comercial completo:** graze; ranks D a SSS; dez fases; dez chefes; builds temporárias; economia balanceada; anúncios recompensados; produto premium; loja cosmética; analytics; política de privacidade; arte final; áudio final; testes de compras; testes em múltiplos aparelhos.

**P2 — lançamento:** AAB; keystore; API alvo; ícones; screenshots; descrições; teste fechado; correções; ranking online se aprovado; configuração iOS.

**P3 — pós-lançamento:** Sobrevivência promovida; Boss Rush expandido; desafios semanais; facção Pirata; facção Ancestral; Score Attack padronizado; temporadas cosméticas; eventos; Cloud Save ampliado; rankings sazonais.

## 19. Ordem de implementação

**Fase A — alinhamento:** congelar funcionalidades; confirmar nome; atualizar GDD; mapear código; identificar sistemas ausentes; indicadores de produto; dispositivos-alvo.
**Fase B — núcleo definitivo:** Zona de Risco; graze; Overdrive; integração com score; integração audiovisual; tutorial; testes.
**Fase C — facções:** Federação; Biomecânica; Cyber; reorganização das naves; Nova-X horizontal; especiais; passivas; feedback próprio.
**Fase D — fatia comercial:** uma fase e um chefe com arte final; três naves representativas; UI e áudio finais parciais; monetização em sandbox; teste em dispositivos; playtest externo.
**Fase E — conteúdo:** dez fases; dez chefes; balancear armas e upgrades; eventos; builds; arte e áudio.
**Fase F — negócio e serviços:** analytics; ads; IAP; ranking; Cloud Save se aprovado; privacidade; consentimento; recibos; ambientes Development, Staging e Production.
**Fase G — loja:** ícones; screenshots; vídeo; textos; assinatura; AAB; teste fechado; TestFlight; correções críticas.

## 20. Definition of Done

**Funcionalidade:** implementada; compila; teste adequado; validada no Editor; validada em aparelho; feedback visual; feedback sonoro quando necessário; tratamento de erro; documentada; integrada ao save e ao analytics se aplicável; acessível; sem prejuízo de desempenho; aprovada pelo responsável.

**Fase:** arte final; ondas; variação; chefe; trilha; rank calibrado; sem situação impossível; concluída sem dano por jogador habilidoso; testada em aparelho-alvo; métricas.

**Item comercial:** funciona em sandbox, após reiniciar e após reinstalar; trata cancelamento e perda de conexão; restaura compra; atualiza save e analytics; refletido na política e nos formulários.

## 21. Critérios para considerar o produto pronto

Identidade STAR RISK consistente; Zona de Risco e Overdrive validados; graze funcional; três facções distintas; nenhuma nave superior em tudo; dez fases e dez chefes; curva de dificuldade validada; economia testada com jogadores; arte e áudio finais; tutorial funcional; desempenho nos aparelhos-alvo; anúncios opcionais; compras sem poder competitivo; restauração funcional; política publicada; formulários corretos; build assinada; AAB validado; sem erros críticos; testado por pessoas fora da equipe.

## 22. Itens que não devem desviar o time agora

Multiplayer; cooperativo; clãs; chat; PvP; editor de fases; Steam; passe de temporada; sistema social complexo; novas moedas; mais modos; mais armas além do necessário; mais naves humanas antes de Biomecânica e Cyber; shaders sem justificativa; expansão de Sobrevivência; ranking social; eventos semanais complexos.

## 23. Quadro de acompanhamento

Cada item: ID; título; prioridade; responsável; objetivo; descrição; dependências; critérios de aceite; testes; impacto em save, analytics, privacidade, performance e monetização; status; versão prevista.

Status: Backlog, Ready, In Progress, Code Review, QA, Device Validation, Product Review, Done, Blocked. Não usar "Done" antes da validação em aparelho quando afetar gameplay, interface, monetização ou desempenho.

## 24. Primeiros itens obrigatórios do backlog

| ID | Título | Prioridade | Critérios de aceite |
|---|---|---|---|
| SR-PROD-001 | Confirmar nome comercial | P0 | Nome pesquisado nas lojas; disponibilidade documentada; aprovado; GDD, projeto e documentos alinhados |
| SR-GAME-001 | Zona de Risco | P0 | Cinco estados; integração com score, HUD, áudio e VFX; testes; teste em aparelho |
| SR-GAME-002 | Overdrive (dep. SR-GAME-001) | P0 | Medidor; ativação; duração; encerramento; penalidade por dano; bônus por facção; feedback; testes |
| SR-GAME-003 | Graze | P1 | Um registro por projétil; pontuação; carga de Overdrive; feedback; testes de invulnerabilidade |
| SR-DESIGN-001 | Reestruturar facções | P0 | Federação, Biomecânica e Cyber definidas; naves mapeadas; passivas; Ultimates; integração com risco |
| SR-BAL-001 | Reequilibrar Nova-X | P0 | Não superior em tudo; limitação clara; função própria; comparação; teste com jogadores |
| SR-ECO-001 | Separar créditos de score | P0 | Nova fórmula; limite de bônus de risco; simulação; testes; saldo migrado |
| SR-MON-001 | Rewarded ads | P1 | Revive; bônus pós-fase; caixa limitada; falha tratada; consentimento; analytics; formulários |
| SR-MON-002 | Pacote Comandante | P1 | Produto; compra; validação; restauração; benefícios sem poder; política |
| SR-MON-003 | Loja cosmética | P1 | Catálogo; prévia; compra; equipamento; persistência; sem alterar hitbox; restauração |
| SR-QA-001 | Teste em aparelhos reais | P0 | Android de entrada e intermediário; relatórios de FPS e input; safe areas; temperatura e bateria; defeitos registrados |

## 25. Resumo executivo para o time

O projeto possui boa fundação técnica, conteúdo funcional e testes automatizados, mas não deve ser tratado como produto concluído. As prioridades não são mais modos ou armas, e sim: consolidar a identidade; Zona de Risco; Overdrive; graze; três facções; corrigir a Nova-X; separar economia e score; validar o controle em celular; fatia visual final; monetização ética; dez fases e dez chefes; testar, balancear e preparar as lojas.

O produto só será comercialmente completo quando gameplay, identidade visual, monetização, economia, desempenho, conformidade e publicação estiverem integrados e validados.

**Diretriz final:** proteger o diferencial de STAR RISK. Toda decisão precisa fortalecer habilidade, risco, pontuação, variedade de naves e justiça competitiva.
