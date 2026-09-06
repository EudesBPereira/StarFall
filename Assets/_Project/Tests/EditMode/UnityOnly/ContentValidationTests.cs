using System.Collections.Generic;
using NUnit.Framework;
using Starfall.Bosses;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.Waves;
using UnityEditor;
using UnityEngine;

namespace Starfall.Tests.EditMode.UnityOnly
{
    /// <summary>
    /// "Teste de impossibilidade de padrões" (plan §7.5 / §7.7) over the generated content: every stage has the
    /// required structure and every boss pattern stays inside dodgeable limits. Runs only inside the Unity Editor.
    /// </summary>
    public class ContentValidationTests
    {
        private const float MaxProjectileSpeed = 14f;
        private const float MinAttackInterval = 0.1f;
        private const float MinLaserTelegraph = 0.8f;
        private const int MaxRingCount = 20;

        private static GameConfig LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/ScriptableObjects/Config/GameConfig.asset");
            Assert.That(config, Is.Not.Null, "GameConfig.asset must be generated (Starfall > Run Full Bootstrap)");
            return config;
        }

        [Test]
        public void Campaign_HasTenStages_EachWithBossMiniBossWavesAndTuning()
        {
            var config = LoadConfig();
            Assert.That(config.StageCount, Is.EqualTo(10), "plan §3.5: ten stages");
            Assert.That(config.StageCount, Is.LessThanOrEqualTo(SaveData.StageSlots));
            for (int i = 0; i < config.StageCount; i++)
            {
                var s = config.Stages[i];
                Assert.That(s, Is.Not.Null, $"stage {i + 1}");
                int waves = 0, bosses = 0, minis = 0, hazards = 0, builds = 0, encounters = 0;
                foreach (var ev in s.Events)
                {
                    switch (ev.Type)
                    {
                        case StageEventType.Wave: waves++; Assert.That(ev.Wave, Is.Not.Null, $"{s.name}: wave event without wave"); break;
                        case StageEventType.Boss: bosses++; Assert.That(ev.Boss, Is.Not.Null); break;
                        case StageEventType.MiniBoss: minis++; Assert.That(ev.Boss, Is.Not.Null); break;
                        case StageEventType.Hazard: hazards++; break;
                        case StageEventType.BuildChoice: builds++; break;
                        case StageEventType.RandomEncounter: encounters++; Assert.That(ev.Wave, Is.Not.Null); Assert.That(ev.Chance, Is.InRange(0f, 1f)); break;
                    }
                }
                Assert.That(waves, Is.GreaterThanOrEqualTo(3), $"{s.name}: at least three exclusive formations");
                Assert.That(bosses, Is.EqualTo(1), $"{s.name}: exactly one boss");
                Assert.That(minis, Is.GreaterThanOrEqualTo(1), $"{s.name}: a mini-boss");
                Assert.That(hazards, Is.GreaterThanOrEqualTo(1), $"{s.name}: an environmental event");
                Assert.That(encounters, Is.GreaterThanOrEqualTo(1), $"{s.name}: a random encounter");
                Assert.That(builds, Is.GreaterThanOrEqualTo(1), $"{s.name}: a build choice");
                Assert.That(s.Events[s.Events.Length - 1].Type, Is.EqualTo(StageEventType.Boss), $"{s.name}: the boss ends the stage");
                Assert.That(s.ParTimeSeconds, Is.GreaterThan(60f));
                Assert.That(s.RankTargetScore, Is.GreaterThan(1000));
                Assert.That(s.BaseCredits, Is.GreaterThan(0));
                Assert.That(s.EnemyStatMultiplier, Is.InRange(0.5f, 2f));
                if (i > 0) Assert.That(s.RankTargetScore, Is.GreaterThan(config.Stages[i - 1].RankTargetScore), "rank targets grow with the stage");
            }
        }

        [Test]
        public void TenDistinctMainBosses_WithPhasesAndAttacks()
        {
            var config = LoadConfig();
            Assert.That(config.BossCount, Is.EqualTo(10), "plan §3.5: ten bosses");
            var ids = new HashSet<BossId>();
            var sprites = new HashSet<Sprite>();
            foreach (var b in config.Bosses)
            {
                Assert.That(b, Is.Not.Null);
                Assert.That(b.IsMainBoss, Is.True, b.name);
                Assert.That(ids.Add(b.BossId), Is.True, $"{b.name}: duplicated BossId");
                Assert.That(sprites.Add(b.Sprite), Is.True, $"{b.name}: reuses another boss silhouette (recolours do not count as new bosses)");
                Assert.That(b.Phases.Length, Is.GreaterThanOrEqualTo(2), $"{b.name}: two or three mechanical phases");
                Assert.That(b.Phases[0].HealthThreshold, Is.EqualTo(1f).Within(0.001f), $"{b.name}: first phase at full health");
                Assert.That(b.Prefab, Is.Not.Null, $"{b.name}: prefab");
                bool anyTelegraphedLaser = true;
                foreach (var phase in b.Phases)
                {
                    Assert.That(phase.Attacks, Is.Not.Null.And.Not.Empty, $"{b.name}/{phase.Name}: attacks");
                    foreach (var atk in phase.Attacks)
                    {
                        var p = atk.Settings;
                        Assert.That(p.Interval, Is.GreaterThanOrEqualTo(MinAttackInterval), $"{b.name}/{phase.Name}: interval");
                        if (atk.Kind != BossAttackKind.Summon && atk.Kind != BossAttackKind.RiftSpawn && atk.Kind != BossAttackKind.FrontLaser)
                            Assert.That(p.ProjectileSpeed, Is.LessThanOrEqualTo(MaxProjectileSpeed), $"{b.name}/{phase.Name}: {atk.Kind} too fast to dodge");
                        if (atk.Kind == BossAttackKind.Ring) Assert.That(p.Count, Is.LessThanOrEqualTo(MaxRingCount), $"{b.name}: ring leaves no gap");
                        if (atk.Kind == BossAttackKind.FrontLaser) anyTelegraphedLaser &= atk.TelegraphSeconds >= MinLaserTelegraph;
                        if (atk.Kind == BossAttackKind.Summon || atk.Kind == BossAttackKind.RiftSpawn)
                        {
                            Assert.That(atk.SummonEnemy, Is.Not.Null, $"{b.name}: summon without enemy");
                            Assert.That(atk.SummonCap, Is.GreaterThan(0), $"{b.name}: summon without cap");
                        }
                    }
                }
                Assert.That(anyTelegraphedLaser, Is.True, $"{b.name}: every laser must be telegraphed");
                foreach (var part in b.Parts)
                {
                    Assert.That(part.Hull, Is.GreaterThan(0f));
                    Assert.That(part.Radius, Is.GreaterThan(0f));
                    if (part.DisablesAttack >= 0)
                        foreach (var phase in b.Phases)
                            Assert.That(part.DisablesAttack, Is.LessThan(phase.Attacks.Length), $"{b.name}: part disables a missing attack in {phase.Name}");
                }
            }
        }

        [Test]
        public void EveryEnemy_IsDodgeable_AndEveryWaveHasEnemies()
        {
            var guids = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/_Project/ScriptableObjects/Enemies" });
            Assert.That(guids.Length, Is.GreaterThan(10));
            foreach (var g in guids)
            {
                var e = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g));
                if (e.Attack != AttackKind.None)
                {
                    Assert.That(e.AttackSettings.ProjectileSpeed, Is.LessThanOrEqualTo(MaxProjectileSpeed), e.name);
                    Assert.That(e.AttackSettings.Interval, Is.GreaterThanOrEqualTo(MinAttackInterval), e.name);
                }
                Assert.That(e.MaxHull, Is.GreaterThan(0f), e.name);
                Assert.That(e.RiskWeight, Is.GreaterThanOrEqualTo(0f), e.name);
            }
            var waves = AssetDatabase.FindAssets("t:WaveDefinition", new[] { "Assets/_Project/ScriptableObjects/Waves" });
            foreach (var g in waves)
            {
                var w = AssetDatabase.LoadAssetAtPath<WaveDefinition>(AssetDatabase.GUIDToAssetPath(g));
                Assert.That(w.Entries, Is.Not.Empty, w.name);
                foreach (var entry in w.Entries)
                {
                    Assert.That(entry.Enemy, Is.Not.Null, w.name);
                    Assert.That(entry.Count, Is.GreaterThan(0), w.name);
                    if (entry.Pattern == SpawnPattern.LeftEdge || entry.Pattern == SpawnPattern.RightEdge || entry.Pattern == SpawnPattern.Pincer)
                        Assert.That(entry.Enemy.Movement, Is.EqualTo(MovementKind.SideSweep), $"{w.name}: edge spawns need a side-sweeping enemy");
                }
            }
        }

        [Test]
        public void Catalogs_MatchEnums()
        {
            var config = LoadConfig();
            Assert.That(config.Ships.Length, Is.EqualTo(ProgressionRules.ShipCount));
            Assert.That(config.Weapons.Length, Is.EqualTo(ProgressionRules.WeaponCount));
            for (int i = 0; i < config.Ships.Length; i++) Assert.That((int)config.Ships[i].Id, Is.EqualTo(i), "ships ordered by id");
            for (int i = 0; i < config.Weapons.Length; i++) Assert.That((int)config.Weapons[i].Id, Is.EqualTo(i), "weapons ordered by id");
            Assert.That(config.ProceduralEnemies.Length, Is.EqualTo(System.Enum.GetValues(typeof(ProceduralEnemy)).Length));
        }
    }
}
