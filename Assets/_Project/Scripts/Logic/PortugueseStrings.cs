// Pure C# - no UnityEngine dependency. Shared by runtime and tests.
using System.Collections.Generic;

namespace Starfall.Logic
{
    /// <summary>Portuguese (Brazil) table: English source text → translation. Keys must match the code/data verbatim.</summary>
    public static class PortugueseStrings
    {
        public static readonly IReadOnlyDictionary<string, string> Table = new Dictionary<string, string>
        {
            // ---- Menu principal ----
            ["PLAY"] = "JOGAR", ["HANGAR"] = "HANGAR", ["UPGRADES"] = "MELHORIAS", ["RANKING"] = "RANKING",
            ["SETTINGS"] = "CONFIGURAÇÕES", ["CREDITS"] = "CRÉDITOS", ["QUIT"] = "SAIR", ["BACK"] = "VOLTAR",
            ["HIGH SCORE  {0}"] = "RECORDE  {0}",
            ["PILOT LV.{0}   {1} CR   {2} PARTS"] = "PILOTO NV.{0}   {1} CR   {2} PEÇAS",
            ["MADE WITH UNITY  -  PLACEHOLDER ASSETS"] = "FEITO COM UNITY  -  ASSETS PROVISÓRIOS",
            ["STARFALL TEAM"] = "STARFALL TEAM", ["presents"] = "apresenta", ["Presents"] = "Apresenta",
            ["TAP TO LAUNCH"] = "TOQUE PARA LANÇAR", ["PRESS SPACE / START TO LAUNCH"] = "ESPAÇO / START PARA LANÇAR",
            ["LAUNCH"] = "LANÇAR",

            // ---- Seleção de missão ----
            ["SELECT MISSION"] = "SELECIONAR MISSÃO", ["CAMPAIGN"] = "CAMPANHA", ["EXTRA MODES"] = "MODOS EXTRAS",
            ["SURVIVAL"] = "SOBREVIVÊNCIA", ["BOSS RUSH"] = "BOSS RUSH", ["DAILY"] = "DIÁRIO", ["DAILY CHALLENGE"] = "DESAFIO DIÁRIO",
            ["LOCKED"] = "BLOQUEADA", ["NEW"] = "NOVA",
            ["RANK {0}  BEST {1}{2}"] = "RANK {0}  MELHOR {1}{2}",
            ["SURVIVAL best wave {0}   |   BOSS RUSH best {1}   |   DAILY {2}"] = "SOBREVIVÊNCIA melhor onda {0}   |   BOSS RUSH melhor {1}   |   DIÁRIO {2}",
            ["today best {0}"] = "melhor de hoje {0}", ["not played today"] = "não jogado hoje",
            ["\nCAMPAIGN COMPLETE"] = "\nCAMPANHA COMPLETA",
            ["\nBoss Rush unlocks after defeating a boss."] = "\nBoss Rush abre após derrotar um chefe.",

            // ---- Briefing ----
            ["STAGE {0}: {1}"] = "FASE {0}: {1}", ["STAGE {0}"] = "FASE {0}",
            ["Endless waves. Every 8th wave brings a mini-boss.\nHow long can the fleet hold?"] = "Ondas infinitas. A cada 8 ondas, um mini-chefe.\nAté quando a frota resiste?",
            ["Every Swarm flagship, back to back.\nNo waves, no mercy."] = "Todas as naves-capitânia do Enxame, uma atrás da outra.\nSem ondas, sem piedade.",
            ["Today's seed: {0}\nFaster enemies, fewer drops. One attempt counts per day."] = "Semente de hoje: {0}\nInimigos mais rápidos, menos itens. Uma tentativa vale por dia.",
            ["OBJECTIVES\n"] = "OBJETIVOS\n",

            // ---- HUD ----
            ["SHIPS"] = "NAVES", ["HULL"] = "CASCO", ["SHIELD"] = "ESCUDO", ["SCORE"] = "PONTOS", ["OVERDRIVE"] = "OVERDRIVE",
            ["! HULL CRITICAL !"] = "! CASCO CRÍTICO !", ["ENERGY {0}%"] = "ENERGIA {0}%", ["ULTIMATE READY"] = "ULTIMATE PRONTA",
            ["{0} LV.{1}"] = "{0} NV.{1}", ["COMBO x{0}"] = "COMBO x{0}",
            ["STAGE {0}  WAVE {1}"] = "FASE {0}  ONDA {1}", ["BOSS {0}"] = "CHEFE {0}", ["WAVE {0}"] = "ONDA {0}",
            ["+{0} COMPONENT"] = "+{0} COMPONENTE", ["GRAZE {0}"] = "RASPÃO {0}", ["GRAZE"] = "RASPÃO",
            ["RISK {0}{1} x{2}"] = "RISCO {0}{1} x{2}", ["SAFE"] = "SEGURO", ["ALERT!"] = "ALERTA!", ["DANGER!"] = "PERIGO!", ["EXTREME!!"] = "EXTREMO!!",
            ["OVERDRIVE x{0}"] = "OVERDRIVE x{0}", ["OVERDRIVE {0}s"] = "OVERDRIVE {0}s", ["OVERDRIVE READY"] = "OVERDRIVE PRONTO",
            ["ALERT"] = "ALERTA", ["DANGER"] = "PERIGO", ["EXTREME"] = "EXTREMO", ["PARTS"] = "PEÇAS", ["Vanguard"] = "Vanguard",
            ["SPEED"] = "VELOCIDADE", ["DAMAGE"] = "DANO", ["INVINCIBLE"] = "INVENCÍVEL", ["SLOWED"] = "LENTO",
            ["BOSS"] = "CHEFE", ["LASER"] = "LASER",
            ["GRAZE! DODGE CLOSE TO BULLETS FOR BONUS POINTS AND OVERDRIVE"] = "RASPÃO! DESVIE RENTE AOS TIROS PARA PONTOS EXTRAS E OVERDRIVE",
            ["RISK ALERT x2 - CLOSER TO ENEMIES = MORE POINTS"] = "RISCO ALERTA x2 - PERTO DOS INIMIGOS = MAIS PONTOS",
            ["DANGER x3 - OVERDRIVE IS CHARGING"] = "PERIGO x3 - O OVERDRIVE ESTÁ CARREGANDO",
            ["EXTREME x5 - MAXIMUM RISK"] = "EXTREMO x5 - RISCO MÁXIMO",
            ["OVERDRIVE! SCORE UP TO x8 - DAMAGE SHORTENS IT"] = "OVERDRIVE! PONTOS ATÉ x8 - DANO ENCURTA",
            ["OVERDRIVE ENDED"] = "OVERDRIVE ENCERRADO",
            ["CRIT"] = "CRÍTICO", ["ELITE"] = "ELITE",

            // ---- Mensagens de fase ----
            ["SECTOR CLEARED"] = "SETOR LIMPO", ["ALL BOSSES DESTROYED"] = "TODOS OS CHEFES DESTRUÍDOS",
            ["THE SWARM IS DEFEATED"] = "O ENXAME FOI DERROTADO", ["BOSS RUSH COMPLETE"] = "BOSS RUSH COMPLETO",
            ["WARNING: {0}"] = "ALERTA: {0}", ["PHASE {0}"] = "FASE {0}", ["CORE EXPOSED"] = "NÚCLEO EXPOSTO", ["PART DESTROYED"] = "PARTE DESTRUÍDA",
            ["AMBUSH!"] = "EMBOSCADA!", ["{0} INSTALLED"] = "{0} INSTALADO",
            ["SOLAR FLARE - CLEAR THE BAND"] = "TEMPESTADE SOLAR - SAIA DA FAIXA", ["METEOR SHOWER"] = "CHUVA DE METEOROS",
            ["NEBULA PULSE - LOW VISIBILITY"] = "PULSO DA NEBULOSA - BAIXA VISIBILIDADE",
            ["DAILY CHALLENGE  -  FASTER ENEMIES, FEWER DROPS"] = "DESAFIO DIÁRIO  -  INIMIGOS MAIS RÁPIDOS, MENOS ITENS",
            ["SURVIVAL  -  ENDLESS WAVES"] = "SOBREVIVÊNCIA  -  ONDAS INFINITAS",
            ["SECTOR 1: IRON BELT"] = "SETOR 1: CINTURÃO DE FERRO", ["SECTOR 2: SILENT COLONY"] = "SETOR 2: COLÔNIA SILENCIOSA",
            ["SECTOR 3: ORBITAL WALL"] = "SETOR 3: MURALHA ORBITAL", ["SECTOR 4: LIVING NEBULA"] = "SETOR 4: NEBULOSA VIVA",
            ["SECTOR 5: PIRATE CORRIDOR"] = "SETOR 5: CORREDOR PIRATA", ["SECTOR 6: AUTONOMOUS FACTORY"] = "SETOR 6: FÁBRICA AUTÔNOMA",
            ["SECTOR 7: AETHER RUINS"] = "SETOR 7: RUÍNAS DE AETHER", ["SECTOR 8: DIMENSIONAL RIFT"] = "SETOR 8: FENDA DIMENSIONAL",
            ["SECTOR 9: STELLAR CORE"] = "SETOR 9: NÚCLEO ESTELAR", ["SECTOR 10: OMEGA FORTRESS"] = "SETOR 10: FORTALEZA ÔMEGA",
            ["INTERCEPTORS INBOUND"] = "INTERCEPTADORES CHEGANDO", ["PATROL SQUADRON"] = "ESQUADRÃO DE PATRULHA", ["ROGUE DRONE PACK"] = "BANDO DE DRONES",
            ["SCAVENGERS!"] = "SAQUEADORES!", ["PIRATE AMBUSH!"] = "EMBOSCADA PIRATA!", ["PRODUCTION SURGE"] = "SURTO DE PRODUÇÃO",
            ["GUARDIAN CONSTRUCTS"] = "CONSTRUTOS GUARDIÕES", ["RIFT SURGE"] = "SURTO DA FENDA", ["BROOD SWARM"] = "ENXAME DA NINHADA",
            ["SPORE CLOUD"] = "NUVEM DE ESPOROS", ["OMEGA GUARD"] = "GUARDA ÔMEGA", ["THE OMEGA CORE AWAKENS"] = "O NÚCLEO ÔMEGA DESPERTA",
            ["WARNING: SENTINEL-X APPROACHING"] = "ALERTA: SENTINEL-X SE APROXIMA", ["WARNING: THE DESTROYER"] = "ALERTA: THE DESTROYER",
            ["WARNING: WIDOW"] = "ALERTA: WIDOW", ["WARNING: LEVIATHAN"] = "ALERTA: LEVIATHAN", ["WARNING: REAPER WING"] = "ALERTA: REAPER WING",
            ["WARNING: HIVE QUEEN"] = "ALERTA: HIVE QUEEN", ["WARNING: SENTINEL-X MK.II"] = "ALERTA: SENTINEL-X MK.II",
            ["WARNING: DESTROYER MK.II"] = "ALERTA: DESTROYER MK.II", ["WARNING: WIDOW PRIME"] = "ALERTA: WIDOW PRIME",
            ["WARNING: OMEGA CORE"] = "ALERTA: OMEGA CORE", ["WARNING: IRON WARDEN"] = "ALERTA: IRON WARDEN", ["WARNING: BASTION"] = "ALERTA: BASTION",
            ["WARNING: CORSAIR QUEEN"] = "ALERTA: CORSAIR QUEEN", ["WARNING: THE ASSEMBLER"] = "ALERTA: THE ASSEMBLER",
            ["WARNING: AETHER GUARDIAN"] = "ALERTA: AETHER GUARDIAN", ["WARNING: RIFT WALKER"] = "ALERTA: RIFT WALKER", ["WARNING: REAPER PRIME"] = "ALERTA: REAPER PRIME",

            // ---- Pausa / vitória / derrota ----
            ["PAUSED"] = "PAUSADO", ["CONTINUE"] = "CONTINUAR", ["RESTART STAGE"] = "REINICIAR FASE", ["MAIN MENU"] = "MENU PRINCIPAL",
            ["NEXT STAGE"] = "PRÓXIMA FASE", ["GAME OVER"] = "FIM DE JOGO", ["RETRY STAGE"] = "TENTAR DE NOVO", ["PLAY AGAIN"] = "JOGAR DE NOVO",
            ["RANK {0}"] = "RANK {0}", ["SCORE  {0}"] = "PONTOS  {0}", ["FINAL SCORE  {0}"] = "PONTUAÇÃO FINAL  {0}",
            ["KILLS {0}   GRAZE {1}   OBJECTIVE {2}\n"] = "ABATES {0}   RASPÕES {1}   OBJETIVO {2}\n",
            ["TIME {0}   NO DAMAGE {1}"] = "TEMPO {0}   SEM DANO {1}",
            ["BEST COMBO x{0}   BEST RISK x{1}   GRAZES {2}"] = "MELHOR COMBO x{0}   MELHOR RISCO x{1}   RASPÕES {2}",
            ["BOSSES DESTROYED  {0}"] = "CHEFES DESTRUÍDOS  {0}", ["ENEMIES DESTROYED  {0}"] = "INIMIGOS DESTRUÍDOS  {0}",
            ["HITS TAKEN  {0}   OVERDRIVES  {1}"] = "ACERTOS SOFRIDOS  {0}   OVERDRIVES  {1}", ["LIVES LEFT  {0}"] = "VIDAS RESTANTES  {0}",
            ["NEW HIGH SCORE!"] = "NOVO RECORDE!", ["RANKING #{0}"] = "RANKING #{0}",
            ["WAVES SURVIVED  {0}   ENEMIES  {1}"] = "ONDAS SOBREVIVIDAS  {0}   INIMIGOS  {1}", ["ENEMIES  {0}   BOSSES  {1}"] = "INIMIGOS  {0}   CHEFES  {1}",
            ["+{0} CREDITS{1}\n+{2} XP   +{3} COMPONENTS"] = "+{0} CRÉDITOS{1}\n+{2} XP   +{3} COMPONENTES",
            ["  (BASE {0} + RANK {1} + RISK {2} + FIRST CLEAR {3})"] = "  (BASE {0} + RANK {1} + RISCO {2} + PRIMEIRA VEZ {3})",
            ["  (BASE {0} + RANK {1} + RISK {2})"] = "  (BASE {0} + RANK {1} + RISCO {2})",

            // ---- Hangar ----
            ["{0} CREDITS   {1} COMPONENTS"] = "{0} CRÉDITOS   {1} COMPONENTES", ["EQUIPPED"] = "EQUIPADO", ["OWNED"] = "ADQUIRIDO", ["EQUIP"] = "EQUIPAR",
            ["{0} CR"] = "{0} CR", ["BUY  {0} CR"] = "COMPRAR  {0} CR", ["FINISH THE CAMPAIGN"] = "CONCLUA A CAMPANHA", ["Finish the campaign"] = "Conclua a campanha",
            ["HULL {0}   SHIELD {1}   SPEED {2}\n"] = "CASCO {0}   ESCUDO {1}   VELOC. {2}\n",
            ["DAMAGE x{0}   CRIT {1}% (x{2})\n"] = "DANO x{0}   CRÍTICO {1}% (x{2})\n",
            ["SHIELD REGEN {0}/s   ULTIMATE x{1}\n"] = "REGEN. ESCUDO {0}/s   ULTIMATE x{1}\n",
            ["{0}  HULL {1}  SHD {2}  SPD {3}"] = "{0}  CASCO {1}  ESC {2}  VEL {3}",
            ["LIFESTEAL {0}%/KILL"] = "ROUBO DE VIDA {0}%/ABATE", ["DRONE + MARK"] = "DRONE + MARCAÇÃO", ["MARK TARGETS"] = "MARCA ALVOS",
            ["PRECISION +{0}%/HIT"] = "PRECISÃO +{0}%/ACERTO", ["{0}   ULT: {1}   PASSIVE: {2}"] = "{0}   ULT: {1}   PASSIVA: {2}",
            ["   PIERCE {0}"] = "   PERFURA {0}", ["   HOMING"] = "   TELEGUIADO", ["   CHARGE {0}s"] = "   CARGA {0}s",
            ["DAMAGE {0}   RATE {1}/s   DPS {2}{3}\nMAX LEVEL {4}"] = "DANO {0}   CADÊNCIA {1}/s   DPS {2}{3}\nNÍVEL MÁX. {4}",
            ["FEDERATION"] = "FEDERAÇÃO", ["BIOMECH"] = "BIOMECÂNICA", ["CYBER CORP"] = "CYBER CORP",
            ["ORBITAL STRIKE"] = "ATAQUE ORBITAL", ["SPORE SWARM"] = "ENXAME DE ESPOROS", ["EMP BURST"] = "PULSO EMP",
            ["SF-01 Vanguard"] = "SF-01 Vanguard", ["SF-02 Falcon"] = "SF-02 Falcon", ["SF-03 Titan"] = "SF-03 Titan", ["SF-04 Phantom"] = "SF-04 Phantom",
            ["Balanced experimental interceptor of the Earth Defense Fleet."] = "Interceptador experimental equilibrado da Frota de Defesa da Terra.",
            ["Very fast strike craft. Thin hull, sharp reflexes."] = "Caça de ataque muito rápido. Casco fino, reflexos afiados.",
            ["Heavy assault frame. Slow, but shrugs off punishment."] = "Estrutura de assalto pesada. Lenta, mas aguenta muito castigo.",
            ["Corporate stealth interceptor. Precision systems: high critical chance, and Overdrive pushes criticals even further."] = "Interceptador furtivo corporativo. Sistemas de precisão: alta chance de crítico, e o Overdrive eleva os críticos ainda mais.",
            ["Experimental hybrid prototype. Enormous score potential and a devastating Ultimate, but thin armor, no shield regeneration and an Overdrive that only rewards relentless aggression."] = "Protótipo híbrido experimental. Potencial de pontuação enorme e uma Ultimate devastadora, mas blindagem fina, sem regeneração de escudo e um Overdrive que só recompensa agressão constante.",
            ["Biomechanical organism grafted to a hull. Its spores hunt on their own, every kill feeds the ship, and its swarm Ultimate keeps hunting after you fire it."] = "Organismo biomecânico enxertado num casco. Os esporos caçam sozinhos, cada abate alimenta a nave, e a Ultimate de enxame continua caçando depois de disparada.",
            ["Corporate command frame with a combat drone. Everything it hits is marked and takes extra damage; its EMP freezes the battlefield."] = "Estrutura de comando corporativa com drone de combate. Tudo que ela acerta fica marcado e recebe dano extra; o EMP congela o campo de batalha.",
            ["Reliable energy beam. Levels add parallel and angled shots."] = "Feixe de energia confiável. Níveis adicionam tiros paralelos e angulados.",
            ["Double Laser"] = "Laser Duplo", ["Twin beams. More shots per volley at every level."] = "Feixes gêmeos. Mais tiros por rajada a cada nível.",
            ["Slow, heavy bolts with high damage per hit."] = "Projéteis lentos e pesados com dano alto por acerto.",
            ["Spread Shot"] = "Tiro em Leque", ["Fan of shots. Great coverage, lower damage per bullet."] = "Leque de tiros. Ótima cobertura, menos dano por projétil.",
            ["Hyper-velocity slug that pierces through several enemies."] = "Projétil hipersônico que atravessa vários inimigos.",
            ["Missiles"] = "Mísseis", ["Homing warheads with splash damage. Never miss, but reload slowly."] = "Ogivas teleguiadas com dano em área. Nunca erram, mas recarregam devagar.",
            ["Energy Cannon"] = "Canhão de Energia", ["Hold to charge, release to unleash a massive orb."] = "Segure para carregar, solte para liberar um orbe gigante.",
            ["Spore Launcher"] = "Lançador de Esporos", ["Living spores that seek the nearest enemy. Weak alone, relentless in numbers."] = "Esporos vivos que buscam o inimigo mais próximo. Fracos sozinhos, implacáveis em grupo.",

            // ---- Melhorias ----
            ["MAX"] = "MÁX", ["{0} CR + {1} PARTS"] = "{0} CR + {1} PEÇAS", ["[{0}]   {1} per level"] = "[{0}]   {1} por nível",
            ["WEAPONS"] = "ARMAS", ["MOBILITY"] = "MOBILIDADE", ["SPECIAL"] = "ESPECIAL",
            ["Weapon Damage"] = "Dano da Arma", ["Fire Rate"] = "Cadência", ["Weapon Range"] = "Alcance", ["Shield Capacity"] = "Capacidade do Escudo",
            ["Shield Regeneration"] = "Regeneração do Escudo", ["Hull Integrity"] = "Integridade do Casco", ["Engine Speed"] = "Velocidade do Motor",
            ["Thrusters"] = "Propulsores", ["Ultimate Recharge"] = "Recarga da Ultimate", ["Ultimate Power"] = "Potência da Ultimate",
            ["+8% damage"] = "+8% de dano", ["+7% fire rate"] = "+7% de cadência", ["+10% range"] = "+10% de alcance", ["+12% shield"] = "+12% de escudo",
            ["+1 shield/s"] = "+1 escudo/s", ["+10% hull"] = "+10% de casco", ["+5% speed"] = "+5% de velocidade", ["+8% acceleration"] = "+8% de aceleração",
            ["+10% energy gain"] = "+10% de ganho de energia", ["+15% ultimate damage"] = "+15% de dano da ultimate",

            // ---- Módulos temporários ----
            ["CHOOSE A MODULE"] = "ESCOLHA UM MÓDULO", ["Current build: none"] = "Build atual: nenhuma", ["Current build: "] = "Build atual: ", ["SKIP"] = "PULAR",
            ["Overclock"] = "Overclock", ["Heavy Rounds"] = "Munição Pesada", ["Piercing Tips"] = "Pontas Perfurantes", ["Wide Spread"] = "Leque Amplo",
            ["Reactive Plating"] = "Blindagem Reativa", ["Nano Repair"] = "Nano Reparo", ["Afterburner"] = "Pós-combustor", ["Risk Tuner"] = "Sintonizador de Risco",
            ["Magnet"] = "Ímã", ["Lucky Core"] = "Núcleo da Sorte", ["Focus Lens"] = "Lente de Foco", ["Energy Cells"] = "Células de Energia",
            ["+12% fire rate"] = "+12% de cadência", ["+15% damage"] = "+15% de dano", ["Shots pierce one more enemy"] = "Tiros atravessam mais um inimigo",
            ["+1 angled shot per volley"] = "+1 tiro angulado por rajada", ["+25 max shield, restored now"] = "+25 de escudo máximo, restaurado agora",
            ["Kills restore 1% hull"] = "Abates restauram 1% do casco", ["+8% speed"] = "+8% de velocidade", ["+20% Overdrive gain"] = "+20% de ganho de Overdrive",
            ["Pickups fly to you"] = "Itens voam até você", ["+8% critical chance"] = "+8% de chance de crítico",
            ["+15% projectile speed and range"] = "+15% de velocidade e alcance dos tiros", ["+20% Ultimate charge"] = "+20% de carga da Ultimate",

            // ---- Ranking / conquistas ----
            ["No runs recorded yet."] = "Nenhuma partida registrada ainda.", ["ACHIEVEMENTS\n"] = "CONQUISTAS\n", ["ACHIEVEMENT: {0}"] = "CONQUISTA: {0}",
            ["STAGE {0}  {1}  {2}"] = "FASE {0}  {1}  {2}", ["WAVE {0}  {1}  {2}"] = "ONDA {0}  {1}  {2}",
            ["First Kill"] = "Primeiro Abate", ["Survivor"] = "Sobrevivente", ["Collector"] = "Colecionador", ["Boss Hunter"] = "Caçador de Chefes", ["Galactic Legend"] = "Lenda Galáctica",
            ["Destroy your first enemy."] = "Destrua seu primeiro inimigo.", ["Finish a stage without losing a life."] = "Termine uma fase sem perder vidas.",
            ["Max out every upgrade."] = "Maximize todas as melhorias.", ["Defeat every boss."] = "Derrote todos os chefes.",
            ["Complete 100%: stages, ships, weapons, upgrades."] = "Complete 100%: fases, naves, armas, melhorias.",

            // ---- Configurações ----
            ["MASTER VOLUME"] = "VOLUME GERAL", ["MUSIC"] = "MÚSICA", ["EFFECTS"] = "EFEITOS", ["TOUCH SENSITIVITY"] = "SENSIBILIDADE DO TOQUE",
            ["AUTO FIRE"] = "DISPARO AUTOMÁTICO", ["SCREEN SHAKE"] = "TREMOR DE TELA", ["REDUCED EFFECTS"] = "EFEITOS REDUZIDOS", ["SHOW HITBOX"] = "MOSTRAR HITBOX",
            ["GRAZE FEEDBACK"] = "AVISO DE RASPÃO", ["RESET PROGRESS"] = "REDEFINIR PROGRESSO", ["Progress reset."] = "Progresso redefinido.",
            ["LANGUAGE: {0}"] = "IDIOMA: {0}",

            // ---- Créditos ----
            ["STARFALL DEFENSE - MVP 1.0\n\nDesign, code and placeholder art: Starfall Team\nBuilt with Unity 6\n\nPlaceholder audio is synthesized at runtime.\nAll shapes are original procedural placeholders."] =
                "STARFALL DEFENSE\n\nDesign, código e arte: Starfall Team\nFeito com Unity 6\n\nÁudio provisório sintetizado em tempo real.",

            // ---- Power-ups ----
            ["Weapon Power"] = "Poder da Arma", ["WEAPON UP"] = "ARMA +", ["Shield Restore"] = "Recarga de Escudo", ["SHIELD +50"] = "ESCUDO +50",
            ["Damage Boost"] = "Impulso de Dano", ["DAMAGE x2"] = "DANO x2", ["Speed Boost"] = "Impulso de Velocidade", ["SPEED UP"] = "VELOCIDADE +",
            ["Ultimate Energy"] = "Energia da Ultimate", ["ENERGY +40"] = "ENERGIA +40", ["Invincibility"] = "Invencibilidade", ["INVINCIBLE"] = "INVENCÍVEL",

            // ---- Fases ----
            ["Iron Belt"] = "Cinturão de Ferro", ["Mining belt - first contact"] = "Cinturão de mineração - primeiro contato",
            ["Silent Colony"] = "Colônia Silenciosa", ["Dead colony in Earth orbit"] = "Colônia morta na órbita da Terra",
            ["Orbital Wall"] = "Muralha Orbital", ["Swarm blockade - turret grid"] = "Bloqueio do Enxame - grade de torres",
            ["Living Nebula"] = "Nebulosa Viva", ["Ionized cloud - something breathes inside"] = "Nuvem ionizada - algo respira lá dentro",
            ["Pirate Corridor"] = "Corredor Pirata", ["Smuggler lanes - raiders and mines"] = "Rotas de contrabando - saqueadores e minas",
            ["Autonomous Factory"] = "Fábrica Autônoma", ["Swarm forge - production never stops"] = "Forja do Enxame - a produção nunca para",
            ["Aether Ruins"] = "Ruínas de Aether", ["Precursor ruins - the Swarm digs for something"] = "Ruínas precursoras - o Enxame escava algo",
            ["Dimensional Rift"] = "Fenda Dimensional", ["Tear in space - enemies from nowhere"] = "Rasgo no espaço - inimigos do nada",
            ["Stellar Core"] = "Núcleo Estelar", ["Hive built around a dying star"] = "Colmeia construída em volta de uma estrela moribunda",
            ["Omega Fortress"] = "Fortaleza Ômega", ["Swarm command - the machine intelligence"] = "Comando do Enxame - a inteligência da máquina",
            ["Deep Space"] = "Espaço Profundo", ["Endless"] = "Infinito",
            ["The Swarm hit the mining belt first. Rocks everywhere, drones between them.\nDrag to move. Your weapon fires on its own.\nStay close to enemies to raise your RISK multiplier, and dodge close to bullets for GRAZE bonuses."] =
                "O Enxame atacou o cinturão de mineração primeiro. Rochas por toda parte, drones entre elas.\nArraste para mover. Sua arma dispara sozinha.\nFique perto dos inimigos para subir o multiplicador de RISCO e desvie rente aos tiros para bônus de RASPÃO.",
            ["No answer from the colony domes. The Destroyer, a Swarm scout carrier, is parked above them.\nSolar activity is high: leave the flare band when it lights up."] =
                "Nenhuma resposta dos domos da colônia. The Destroyer, um cargueiro batedor do Enxame, está parado sobre eles.\nA atividade solar está alta: saia da faixa quando ela acender.",
            ["Sensors fail in the nebula and the gas itself pulses. The Swarm breeds here.\nExpect every enemy type, elite escorts and the Leviathan coiling beneath the cloud."] =
                "Os sensores falham na nebulosa e o próprio gás pulsa. O Enxame se reproduz aqui.\nEspere todos os tipos de inimigo, escoltas elite e o Leviathan enrolado sob a nuvem.",
            ["Human pirates sold the corridor to the Swarm. Raiders come from the sides, mines drift everywhere,\nand their queen fights dirty."] =
                "Piratas humanos venderam o corredor ao Enxame. Saqueadores vêm pelos lados, minas flutuam por toda parte,\ne a rainha deles luta sujo.",
            ["A factory the size of a moon builds the Swarm army in real time.\nTurrets roll off the line and The Assembler keeps making more. Destroy its fabricators to stop the flow."] =
                "Uma fábrica do tamanho de uma lua constrói o exército do Enxame em tempo real.\nTorres saem da linha e The Assembler não para. Destrua as fabricadoras para cortar o fluxo.",
            ["Ruins of a civilization older than the Swarm. Its guardian still stands.\nThe Guardian's core is shielded by crystals: shatter them first, then strike the core."] =
                "Ruínas de uma civilização mais antiga que o Enxame. O guardião ainda está de pé.\nO núcleo do Guardião é protegido por cristais: quebre-os primeiro, depois ataque o núcleo.",
            ["The Swarm hive is wrapped around a star. Heat, swarms and their queen.\nFlares are constant here. Everything comes in numbers."] =
                "A colmeia do Enxame envolve uma estrela. Calor, enxames e a rainha.\nAs tempestades solares são constantes. Tudo vem em grande número.",
            ["- Clear the drone patrols\n- Survive the meteor shower\n- Destroy the Iron Warden"] = "- Elimine as patrulhas de drones\n- Sobreviva à chuva de meteoros\n- Destrua o Iron Warden",
            ["- Sweep the colony\n- Defeat the Widow\n- Destroy The Destroyer"] = "- Varra a colônia\n- Derrote a Widow\n- Destrua The Destroyer",
            ["- Silence the turret grid\n- Shoot down the Reaper Wing\n- Breach the Bastion"] = "- Silencie a grade de torres\n- Abata o Reaper Wing\n- Rompa o Bastion",
            ["- Break through the nebula\n- Defeat the Widow Prime\n- Destroy the Leviathan"] = "- Atravesse a nebulosa\n- Derrote a Widow Prime\n- Destrua o Leviathan",
            ["- Run the corridor\n- Defeat Sentinel-X Mk.II\n- Sink the Corsair Queen"] = "- Percorra o corredor\n- Derrote o Sentinel-X Mk.II\n- Afunde a Corsair Queen",
            ["- Shut down the assembly lines\n- Defeat the Reaper Prime\n- Destroy The Assembler"] = "- Desligue as linhas de montagem\n- Derrote o Reaper Prime\n- Destrua The Assembler",
            ["- Cross the ruins\n- Defeat the Widow Prime\n- Shatter the crystals and destroy the Aether Guardian"] = "- Cruze as ruínas\n- Derrote a Widow Prime\n- Quebre os cristais e destrua o Aether Guardian",
            ["- Hold the line at the rift\n- Defeat the Reaper Prime\n- Destroy the Rift Walker"] = "- Segure a linha na fenda\n- Derrote o Reaper Prime\n- Destrua o Rift Walker",
            ["- Reach the core\n- Defeat Sentinel-X Mk.II\n- Kill the Hive Queen"] = "- Alcance o núcleo\n- Derrote o Sentinel-X Mk.II\n- Mate a Hive Queen",
            ["- Storm the fortress\n- Destroy the Destroyer Mk.II\n- Destroy the Omega Core"] = "- Invada a fortaleza\n- Destrua o Destroyer Mk.II\n- Destrua o Omega Core",

            // ---- Inimigos e partes ----
            ["Drone"] = "Drone", ["Interceptor"] = "Interceptador", ["Bomber"] = "Bombardeiro", ["Kamikaze"] = "Kamikaze", ["ShieldDrone"] = "Drone Escudado",
            ["Turret"] = "Torre", ["SupplyDrone"] = "Drone de Suprimento", ["Asteroid"] = "Asteroide", ["Raider"] = "Saqueador", ["Pirate Captain"] = "Capitão Pirata",
            ["Elite Drone"] = "Drone Elite", ["Elite Interceptor"] = "Interceptador Elite", ["Elite Bomber"] = "Bombardeiro Elite", ["Elite Kamikaze"] = "Kamikaze Elite",
            ["Elite Shield Drone"] = "Drone Escudado Elite",
            ["Left Drill"] = "Broca Esquerda", ["Right Drill"] = "Broca Direita", ["Left Battery"] = "Bateria Esquerda", ["Right Battery"] = "Bateria Direita",
            ["Top Battery"] = "Bateria Central", ["Center Battery"] = "Bateria Central", ["Drone Fabricator"] = "Fabricadora de Drones", ["Turret Fabricator"] = "Fabricadora de Torres",
            ["Crystal"] = "Cristal", ["Left Crystal"] = "Cristal Esquerdo", ["Right Crystal"] = "Cristal Direito", ["Top Crystal"] = "Cristal Superior",
        };
    }
}
