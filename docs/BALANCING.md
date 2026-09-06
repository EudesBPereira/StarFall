# Balanceamento inicial

Valores de partida. Todos vivem em ScriptableObjects (`Assets/_Project/ScriptableObjects`) e são gerados por `ProjectBootstrap.CreateData`. **Nada aqui é regra definitiva**; os pontos marcados com ⚠️ exigem playtest.

## Jogador — SF-01 Vanguard (`Ships/Vanguard.asset`)

| Parâmetro | Valor | Justificativa |
|---|---|---|
| Casco | 100 | Aguenta ~5 tiros de drone após o escudo |
| Escudo | 50 | Absorve 2–3 impactos antes do casco |
| Velocidade | 9 u/s | Cruza a largura (10 u) em ~1,1 s |
| Aceleração / desaceleração | 60 / 80 u/s² | Resposta "arcade" sem deslizar |
| Sensibilidade do toque | 1,4 (0,5–3) | 1 px de dedo = 1,4 px de nave ⚠️ |
| Invulnerabilidade ao sofrer dano | 1,0 s | Evita morrer em rajadas |
| Invulnerabilidade no respawn | 2,5 s | |
| Atraso do respawn | 1,5 s | |
| Vidas iniciais / máximo | 3 / 9 | |

## Laser (`Weapons/Laser.asset`)

| Parâmetro | Valor |
|---|---|
| Dano | 4 (+15 % por nível acima de 1) |
| Cadência | 0,14 s (≈ 28,6 DPS no nível 1) |
| Velocidade / vida do projétil | 18 u/s / 2,5 s |
| Níveis | 1: 1 tiro · 2: 2 paralelos · 3: 1 + 2 angulados (9°) · 4: 2 + 2 · 5: 3 + 2 |

⚠️ Verificar se o nível 5 trivializa o Destroyer.

## Ultimate

| Parâmetro | Valor |
|---|---|
| Energia máxima | 100 |
| Energia por abate | Drone 6 · Interceptor 7 · Bomber 10 · Kamikaze 6 · Shield 8 · asteroide 0 |
| Dano em elites/chefes | 150 |
| Limpa projéteis inimigos | sim |

Com ~15 abates comuns a barra enche; roxo dá +40.

## Inimigos (`Enemies/*.asset`)

| Inimigo | Casco | Escudo | Contato | Movimento | Ataque | Pontos | Energia |
|---|---|---|---|---|---|---|---|
| Drone | 12 | 0 | 15 | Reto, 2,6 u/s | Frontal a cada 2,6 s, dano 8, vel. 5 | 100 | 6 |
| Supply Drone | 14 | 0 | 15 | Reto, 2,2 u/s | Nenhum | 100 | 6 |
| Interceptor | 16 | 0 | 18 | Ziguezague 4 u/s, amp 1,8, freq 3 | Mirado, rajada de 2, a cada 1,5 s, dano 8, vel. 7 | 100 | 7 |
| Bomber | 45 | 0 | 25 | Reto, 1,4 u/s | Leque de 3 (40°) a cada 2,8 s, dano 18, vel. 4, projétil 1,7x | 100 | 10 |
| Kamikaze | 10 | 0 | 25 (autodestrói) | Perseguição 5 u/s, giro 120°/s | Nenhum | 100 | 6 |
| Shield | 20 | 30 | 18 | Desce, segura a 28 % do topo por 7 s, orbita 2,2 u/s | Mirado a cada 2 s, dano 10, vel. 6 | 100 | 8 |
| Asteroide | 40 | 0 | 20 | Reto, 1,8 u/s | Nenhum (obstáculo) | 25 | 0 |

- Todos usam a tabela de drop padrão (14 %); o Supply Drone usa a tabela rica (75 %).
- Pesos de drop: Azul 3, Verde 3, Vermelho 2, Amarelo 2, Roxo 2, Branco 1.

## Mini-chefe Sentinel-X (`Bosses/SentinelX.asset`) — 2.500 pontos

| Fase | Entra em | Movimento | Ataques |
|---|---|---|---|
| 1 | 100 % | Patrulha lateral amp 3, freq 0,8 | Anel de 10 a cada 1,8 s (dano 12, vel. 4,5) |
| 2 "Overdrive" | ≤ 50 % | amp 3,5, freq 1,4 | Anel de 14 a cada 1,4 s + leque mirado de 3 (30°), rajada 2, a cada 2,2 s |

Casco 900, entrada 2,5 s (invulnerável durante a entrada). ⚠️ tempo de luta alvo: 40–60 s.

## Chefe The Destroyer (`Bosses/Destroyer.asset`) — 10.000 pontos

| Fase | Entra em | Movimento | Ataques |
|---|---|---|---|
| 1 | 100 % | amp 2,5, freq 0,6 | Canhões laterais alternados a cada 0,9 s (dano 12, vel. 7) |
| 2 "Missile bays open" | ≤ 70 % | amp 3, freq 0,9 | Canhões (2 tiros, 12°) a cada 0,8 s + 2 mísseis teleguiados (giro 60°/s, dano 18) a cada 3,5 s |
| 3 "Core exposed" | ≤ 40 % | amp 3,5, freq 1,2 | Canhões (3 tiros, 20°) a cada 0,7 s + mísseis a cada 4 s + laser frontal: aviso 1,2 s, feixe 1,6 s, largura 1,2 u, 45 dano/s, a cada 6 s |

Casco 2.800, entrada 3 s. ⚠️ tempo de luta alvo: 90–120 s; verificar legibilidade dos mísseis com a névoa da fase 3.

## Ondas e fases

- Intervalos entre spawns: 0,3–1,5 s; ondas esperam a tela limpar (timeout de segurança 30–50 s).
- Fase 1: 7 ondas, apenas Drone/Interceptor/Supply. Bônus 1.000.
- Fase 2: 6 ondas + campo de asteroides (1,4 s ± 40 %) + Sentinel-X. Bônus 2.000.
- Fase 3: 4 ondas densas com os 5 tipos + névoa (alpha 0,22) + Destroyer. Bônus 5.000.

## Pontuação

- Comum 100 · Elite (futuro) 500 · Mini-chefe 2.500 · Chefe 10.000.
- Multiplicador +1 a cada 4 abates sem dano, máx. x10; reset a x1 ao sofrer dano.

## Pontos que exigem playtest ⚠️

1. Sensibilidade de arrasto padrão (1,4) em telas pequenas vs. tablets.
2. Densidade da fase 3 com névoa: legibilidade dos projéteis roxos do Bomber.
3. Frequência de drop (14 %) — pode ser alta com ondas de 8 drones.
4. Duração dos chefes com laser nível 5 + Vermelho.
5. Dano de contato do Bomber (25) vs. escudo 50.

---

# Versão 1.0 completa (GDD `document.md`)

Adições da entrega completa do GDD. Todos os valores vivem em `ContentFactory.CreateData`.

## Naves (`ScriptableObjects/Ships`, GDD §15)

| Nave | Casco | Escudo | Regen | Veloc. | Crítico | Dano | Carga Ult. | Poder Ult. | Custo |
|---|---|---|---|---|---|---|---|---|---|
| SF-01 Vanguard | 100 | 50 | 0 | 9,0 | 5 % x2,0 | x1,00 | x1,00 | x1,00 | inicial |
| SF-02 Falcon | 75 | 40 | 0 | 12,0 | 8 % x2,0 | x1,00 | x1,15 | x0,90 | 2 500 |
| SF-03 Titan | 160 | 90 | 1,5/s | 6,8 | 3 % x1,8 | x1,10 | x0,90 | x1,30 | 4 000 |
| SF-04 Phantom | 85 | 45 | 0,5/s | 10,0 | 25 % x3,0 | x0,95 | x1,00 | x1,00 | 6 000 |
| Nova-X | 130 | 80 | 2,0/s | 11,0 | 15 % x2,5 | x1,20 | x1,25 | x1,50 | campanha |

Nova-X não é comprável: desbloqueia ao concluir a fase 5. ⚠️ Verificar se Phantom com 25 % de crítico supera a Nova-X.

## Armas (`ScriptableObjects/Weapons`, GDD §4)

| Arma | Dano | Cadência | DPS nv.1 | Especial | Custo |
|---|---|---|---|---|---|
| Laser | 4 | 0,14 s | 28,6 | 5 níveis: 1→3 tiros + leque | inicial |
| Laser Duplo | 3,5 | 0,13 s | 53,8 | sempre dois canos | 1 500 |
| Plasma | 11 | 0,32 s | 34,4 | projétil grande e lento | 3 000 |
| Spread Shot | 3 | 0,20 s | 45,0 | leque de 3→8 tiros | 2 500 |
| Railgun | 16 | 0,55 s | 29,1 | perfura 3 alvos, 32 u/s | 4 500 |
| Mísseis | 12 | 0,45 s | 53,3 | teleguiado + área 0,9 u | 5 000 |
| Canhão de Energia | 10 | 0,60 s | 16,7 | carga 1,2 s → x4 dano, x2,6 tamanho | 7 000 |

⚠️ Laser Duplo e Mísseis parecem fortes demais pelo preço; medir em playtest.

## Árvore de upgrades (`UpgradeCatalog`, GDD §14)

Cinco níveis por nó. Custo do próximo nível = `base * (nível+1)^1,6`; a partir do nível 3 também custa componentes (1, 2 e 3).

| Nó | Base | Bônus por nível |
|---|---|---|
| Dano da arma | 400 | +8 % |
| Cadência | 400 | +7 % |
| Alcance | 300 | +10 % |
| Capacidade de escudo | 350 | +12 % |
| Regeneração de escudo | 450 | +1,0 escudo/s |
| Integridade do casco | 350 | +10 % |
| Velocidade | 300 | +5 % |
| Aceleração | 250 | +8 % |
| Recarga da Ultimate | 500 | +10 % |
| Potência da Ultimate | 600 | +15 % |

Maximizar tudo custa ≈ 63 000 créditos e 30 componentes. ⚠️ Ajustar após medir créditos por run.

## Progressão (`ProgressionRules`, GDD §13)

| Regra | Valor |
|---|---|
| Créditos | `pontuação/10 + bônus_da_fase/5`, x0,5 quando o jogador perde |
| XP | 5 por abate, 20 por elite, 200 por chefe, +150 ao concluir |
| Componentes | só de elites (1), mini-chefes (1) e chefes (3–5) |
| Nível de piloto | nível n exige `100·(n-1)²` de XP |

## Inimigos adicionais

| Inimigo | Casco | Escudo | Contato | Pontos | Comportamento |
|---|---|---|---|---|---|
| Turret | 55 | 0 | 25 | 100 | desce até a linha e fica; anel de 8 projéteis a cada 2,4 s |
| Supply Drone | 8 | 0 | 0 | 50 | zigue-zague lento, drop garantido |
| Elites (5 tipos) | x2,4 | x2,0 | x1,3 | 500 | +15 % velocidade, ataque 25 % mais rápido, 1 componente |

Elites sobrevivem à Ultimate (recebem 150 de dano em vez de morrer).

## Mini-chefes (GDD §10) — 2 500 pontos cada

| Mini-chefe | Casco | Fases | Padrões |
|---|---|---|---|
| Sentinel-X | 900 | 2 | anel de 10→14 projéteis + tiro mirado |
| Widow | 1 100 | 2 | teia (deixa lento 3 s) + mirado; fase 2 acrescenta anel |
| Reaper Wing | 1 000 | 2 | investidas rápidas + rajadas; fase 2 acrescenta jato varrendo |
| Sentinel-X Mk.II | 1 800 | 2 | variante 30 % mais rápida (fase 4) |
| Widow Prime | 2 200 | 2 | variante 35 % mais rápida (fase 5) |

## Chefes (GDD §11) — 10 000 pontos cada

| Chefe | Casco | Escudo | Fases | Padrões |
|---|---|---|---|---|
| The Destroyer | 2 400 | 0 | 3 | canhões laterais · mísseis · laser frontal (aviso 1,2 s, 45 dano/s) |
| Leviathan | 2 800 | 0 | 3 | corpo de 8 segmentos, jato contínuo, anel, mirado |
| Hive Queen | 3 400 | 200 | 3 | invoca drones/kamikazes/interceptores (limite 8), anéis |
| Destroyer Mk.II | 4 200 | 0 | 3 | variante 15 % mais rápida (fase 4) |
| Omega Core | 6 000 | 400 | 4 | transforma-se 3 vezes; laser, mísseis, invocação, anéis |

Transformações do Omega Core trocam o sprite e dão 1,4–1,8 s de invulnerabilidade.

## Modos extras (GDD §23)

| Modo | Regras |
|---|---|
| Sobrevivência | ondas procedurais infinitas; dificuldade `1 + 0,06·onda`; mini-chefe a cada 8 ondas |
| Boss Rush | os 4 chefes principais em sequência; desbloqueia ao derrotar qualquer chefe |
| Desafio Diário | semente = data; inimigos 15 % mais rápidos, 40 % menos drops; um recorde por dia |

## Fases 4 e 5 (GDD §12)

| Fase | Bônus | Introduz | Chefe |
|---|---|---|---|
| 4 — Fortaleza Mecânica | 2 500 | turrets, elites em massa | Destroyer Mk.II (mini: Sentinel-X Mk.II) |
| 5 — Núcleo da Colmeia | 4 000 | ondas densas com todos os tipos | Omega Core (mini: Widow Prime) |

---

# Fase B do Plano Mestre — Zona de Risco, Overdrive, Graze, Ranks e Economia

Valores em `RiskSettings.Default`, `OverdriveSettings.Default`, `GrazeRules`, `StageResultRules` e `ProgressionRules`. ⚠️ Nenhum deles foi jogado por uma pessoa ainda.

## Zona de Risco (`RiskModel`)

| Parâmetro | Valor |
|---|---|
| Raio do sensor | 3,6 u |
| Contribuição por objeto | `peso × (1 − d/R)²` |
| Peso de projétil inimigo | 0,55 |
| Peso de inimigo | Drone/Interceptor/Shield 1,0 · Bomber 1,2 · Turret 1,2 · Kamikaze 1,6 · Elite 1,4 · Chefe 2,0 (×1,25) · Asteroide 0,7 · Supply 0,2 |
| Limiar Alerta / Perigo / Extremo | 0,25 / 0,6 / 1,1 |
| Subida / descida do nível | 3,5 / 1,2 por segundo |
| Retenção mínima de estado | 0,35 s |
| Multiplicador | Seguro x1 · Alerta x2 · Perigo x3 · Extremo x5 · Overdrive ×1,6 até x8 |

⚠️ Um único drone a 1 u já dá Alerta; dois kamikazes próximos chegam a Extremo. Verificar se Extremo é alcançável demais na fase 1.

## Overdrive (`OverdriveModel`)

| Parâmetro | Valor |
|---|---|
| Medidor | 100 |
| Ganho em Perigo / Extremo | 7 / 14 por segundo |
| Drenagem em Seguro | 5 por segundo (Alerta segura) |
| Graze / abate próximo (≤ 2,2 u) / abate em combo / parte de chefe | +4 / +5 / +1 / +10 |
| Dano carregando / ativo | −30 / metade do tempo restante |
| Duração ativa | 8 s |
| Cadência Federação | Vanguard x1,15 · Falcon x1,2 · Titan x1,1 · Nova-X x1,3 |
| Crítico Cyber (Phantom) | +20 % |
| Carga do Ultimate | x1,5 |
| Ganho da Nova-X | x0,7 |

Do zero, 7 s em Extremo ativam o Overdrive. ⚠️ Medir quantas ativações por fase um jogador mediano consegue.

## Graze (`GrazeRules`)

| Parâmetro | Valor |
|---|---|
| Margem além da hitbox | 0,42 u (hitbox 0,28 → anel de 0,70 u) |
| Pontos | 50 × multiplicador de risco, fora do combo |
| Velocidade mínima do projétil | 2,5 u/s |
| Invulnerável | nunca pontua |

## Ranks e composição do score (`StageResultRules`)

| Parâmetro | Valor |
|---|---|
| Bônus sem dano | 5 000 |
| Bônus de tempo | 40 pontos/s abaixo do par, teto 6 000 |
| Limiares | C 25 % · B 45 % · A 70 % · S 100 % · SS 140 % · SSS 200 % do alvo da fase |

| Fase | Par (s) | Alvo de rank S | Créditos-base |
|---|---:|---:|---:|
| 1 Orbital | 170 | 18 000 | 300 |
| 2 Asteroides | 200 | 26 000 | 380 |
| 3 Nebulosa | 220 | 34 000 | 460 |
| 4 Fortaleza | 240 | 44 000 | 560 |
| 5 Colmeia | 280 | 60 000 | 700 |

⚠️ Alvos estimados por contagem de inimigos × 100 × combo médio 4 × risco médio 2. Recalibrar após playtest.

## Economia (`ProgressionRules`, plano §11.5)

| Parcela | Regra |
|---|---|
| Base | créditos-base da fase (40 % em derrota) |
| Rank | D 0 · C 50 · B 120 · A 220 · S 350 · SS 500 · SSS 700 (metade com revive) |
| Risco | 2 créditos por segundo em Perigo/Extremo, teto 150 |
| Primeira conclusão | = créditos-base |
| Modos infinitos | 12 por onda, teto 600 |
| XP | 5 por abate, 20 por elite, 200 por chefe, 2 por graze, +150 ao concluir |

Uma campanha completa em rank A com primeira conclusão rende ≈ 5 300 créditos: compra a Falcon (2 500), não a Titan e a Phantom juntas.

## Nova-X reequilibrada (SR-BAL-001)

| Atributo | Antes | Agora |
|---|---:|---:|
| Casco | 130 | 90 |
| Escudo | 80 | 40 |
| Regeneração | 2,0/s | 0 |
| Velocidade | 11 | 11 |
| Crítico | 15 % x2,5 | 12 % x2,2 |
| Dano | x1,20 | x1,15 |
| Carga do Ultimate | x1,25 | x1,00 |
| Potência do Ultimate | x1,50 | x1,40 |
| Ganho de Overdrive | x1,0 | x0,7 |
| Cadência em Overdrive | x1,15 | x1,30 |

Função: teto de score mais alto quando o piloto sustenta Extremo; defesa menor que a Vanguard. ⚠️ Comparar com Phantom (25 % de crítico) em playtest.

---

# Fase C — Facções

## Naves novas

| Nave | Facção | Casco | Escudo | Regen | Veloc. | Crítico | Dano | Ult. | Kit | Custo |
|---|---|---:|---:|---:|---:|---|---:|---|---|---:|
| BX-01 Symbiont | Biomecânica | 110 | 30 | 1,0/s | 8,5 | 4 % x1,8 | x1,00 | Spore Swarm | lifesteal 2 %/abate; Overdrive +8 escudo/s | 3 500 |
| CX-7 Nexus | Cyber | 85 | 60 | 0,8/s | 10,0 | 10 % x2,0 | x0,90 | EMP Burst | drone (4 dano / 0,5 s) + marcação | 5 000 |

Phantom passa a Cyber: marca alvos, Ultimate EMP, Overdrive +20 % de crítico.

## Kits

| Regra | Valor |
|---|---|
| Precisão (Federação) | +1 % por acerto consecutivo, teto +25 % |
| Marcação (Cyber) | 4 s, +25 % de dano recebido |
| EMP | 3 s de atordoamento (chefes 1,5 s), 90 de dano, marca 6 s, limpa projéteis |
| Lifesteal (Biomecânica) | 2 % do casco máximo por abate, teto 5 % |
| Spore Swarm | 10 esporos, 45 de dano, área 0,7 u, 6 s de vida, 260°/s |
| Spore Launcher | 3 de dano, 0,18 s, 7,5 u/s, teleguiado 200°/s, 2→7 esporos por nível |

⚠️ EMP + marcação em chefe pode ser forte demais (efetivamente +25 % de dano por 6 s a cada Ultimate). Medir no playtest final.

---

# Fase E — Dez fases, dez chefes, builds temporárias

## Curva da campanha

| Fase | Nome | Mini-chefe | Chefe | Mult. inimigos | Par (s) | Alvo S | Créditos-base |
|---|---|---|---|---:|---:|---:|---:|
| 1 | Iron Belt | Sentinel-X | Iron Warden | 1,00 | 190 | 18 000 | 300 |
| 2 | Silent Colony | Widow | The Destroyer | 1,05 | 210 | 24 000 | 360 |
| 3 | Orbital Wall | Reaper Wing | Bastion | 1,10 | 220 | 30 000 | 420 |
| 4 | Living Nebula | Widow Prime | Leviathan | 1,15 | 230 | 36 000 | 480 |
| 5 | Pirate Corridor | Sentinel-X Mk.II | Corsair Queen | 1,20 | 240 | 42 000 | 540 |
| 6 | Autonomous Factory | Reaper Prime | The Assembler | 1,25 | 250 | 48 000 | 600 |
| 7 | Aether Ruins | Widow Prime | Aether Guardian | 1,30 | 260 | 54 000 | 660 |
| 8 | Dimensional Rift | Reaper Prime | Rift Walker | 1,35 | 270 | 60 000 | 720 |
| 9 | Stellar Core | Sentinel-X Mk.II | Hive Queen | 1,40 | 280 | 68 000 | 800 |
| 10 | Omega Fortress | Destroyer Mk.II | Omega Core | 1,50 | 320 | 80 000 | 900 |

Campanha completa em rank A com primeira conclusão ≈ 12 800 créditos: compra Falcon, Symbiont e Titan; Phantom e Nexus exigem repetir fases ou ranks altos. ⚠️ Recalibrar após playtest.

## Chefes novos

| Chefe | Casco | Escudo | Fases | Partes (vida / pontos) | Padrões |
|---|---:|---:|---:|---|---|
| Iron Warden | 2 000 | 0 | 2 | 2 brocas (220 / 800) | anel 8→12, asteroides, mirado |
| Bastion | 2 600 | 0 | 2 | 3 baterias (240–260 / 700) | canhões laterais, leque frontal, anel, laser (fase 2) |
| Corsair Queen | 3 000 | 0 | 3 | — | minas (24 dano, área 1,3), canhões, capitães, jato |
| The Assembler | 3 400 | 150 | 2 | 2 fabricadoras (300 / 900) | invoca turrets e drones, jato, anel |
| Aether Guardian | 3 200 | 0 | 3 | 3 cristais blindam o núcleo (160 / 700) | teleporte, anel, leque, laser (fase 3) |
| Rift Walker | 3 600 | 200 | 3 | — | teleporte, fendas (kamikaze → interceptor → elite), anel, jato |

## Eventos ambientais

| Evento | Aviso | Efeito |
|---|---:|---|
| Solar flare | 1,4 s piscando | faixa de 2,2 u causa 22 dano/s por 1,8 s; nunca nos 30 % inferiores |
| Chuva de meteoros | 1 s | 8–14 asteroides a 2,2× a velocidade |
| Pulso de nebulosa | imediato (só visual) | névoa até 0,40 de alfa por 6 s |

## Módulos temporários (`BuildMods`)

| Módulo | Efeito por pilha | Pilhas |
|---|---|---:|
| Overclock | +12 % cadência | 3 |
| Heavy Rounds | +15 % dano | 3 |
| Piercing Tips | +1 perfuração | 3 |
| Wide Spread | +1 tiro angulado (±16°) | 3 |
| Reactive Plating | +25 escudo máximo, restaurado | 3 |
| Nano Repair | +1 % de casco por abate (teto 5 %) | 3 |
| Afterburner | +8 % velocidade | 3 |
| Risk Tuner | +20 % ganho de Overdrive | 3 |
| Magnet | itens atraídos | 1 |
| Lucky Core | +8 % crítico | 3 |
| Focus Lens | +15 % velocidade e alcance do projétil | 3 |
| Energy Cells | +20 % carga do Ultimate | 3 |

Sorteios por fase: 2 (após o mini-chefe e antes do chefe). Sobrevivência: a cada 5 ondas. Boss Rush: a cada 3 chefes. ⚠️ Heavy Rounds ×3 + Overclock ×3 ≈ +115 % de DPS; verificar se trivializa chefes tardios.

## Ajustes de armas (plano §19 fase E)

| Arma | Antes | Agora | Motivo |
|---|---|---|---|
| Laser Duplo | 3,5 de dano | 3,0 | DPS 54 → 46, fecha a distância para o Laser |
| Mísseis | 0,45 s | 0,55 s | DPS 53 → 44 com dano em área e busca |
