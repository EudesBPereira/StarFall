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
