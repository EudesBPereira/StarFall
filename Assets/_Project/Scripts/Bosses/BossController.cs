using System.Collections.Generic;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Bosses
{
    /// <summary>
    /// Boss = enemy with an entrance, health-driven phases and several concurrent attack patterns.
    /// Sentinel-X and The Destroyer are two <see cref="BossDefinition"/> assets driving this one class.
    /// </summary>
    public sealed class BossController : Enemy
    {
        [SerializeField] internal LaserBeam frontLaser;

        private BossDefinition _boss;
        private float _entranceTimer;
        private bool _entering;
        private float _entranceStartY;
        private float _entranceTargetY;
        private int _phaseIndex = -1;
        private IMovementStrategy _phaseMovement;
        private MovementKind _phaseMovementKind;
        private readonly List<AttackRunner> _runners = new List<AttackRunner>(4);

        public BossDefinition Boss => _boss;
        public bool IsEntering => _entering;
        public int PhaseIndex => _phaseIndex;
        /// <summary>Movement parameters come from the active phase instead of the base definition.</summary>
        public override MovementParams Movement => _phaseIndex >= 0 && _boss != null && _phaseIndex < _boss.Phases.Length
            ? _boss.Phases[_phaseIndex].MovementSettings
            : base.Movement;

        public override void Initialize(EnemyDefinition definition, GameplayContext ctx, EnemySpawner spawner)
        {
            base.Initialize(definition, ctx, spawner);
            _boss = definition as BossDefinition;
            _phaseIndex = -1;
            _runners.Clear();

            var area = Area;
            _entranceStartY = area != null ? area.Top + 3f : transform.position.y;
            _entranceTargetY = area != null ? area.Top - (_boss != null ? _boss.EntranceHeight : 0.2f) * area.Height : transform.position.y;
            var pos = transform.position;
            pos.x = area != null ? area.Center.x : pos.x;
            pos.y = _entranceStartY;
            transform.position = pos;

            _entering = true;
            _entranceTimer = 0f;
            Health.Invulnerable = true;

            if (frontLaser != null)
            {
                float length = area != null ? area.Height + 4f : 24f;
                frontLaser.Configure(1f, length, new Color(1f, 0.35f, 0.55f, 0.95f));
            }

            GameSignals.RaiseBossSpawned(this);
            if (ctx != null && ctx.CameraShake != null) ctx.CameraShake.Shake(0.3f, 1.2f);
        }

        protected override void Update()
        {
            if (!IsActiveInstance) return;
            float dt = Time.deltaTime;

            if (_entering)
            {
                _entranceTimer += dt;
                float duration = _boss != null ? _boss.EntranceDuration : 2.5f;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_entranceTimer / duration));
                var pos = transform.position;
                pos.y = Mathf.Lerp(_entranceStartY, _entranceTargetY, k);
                transform.position = pos;
                if (k >= 1f)
                {
                    _entering = false;
                    Health.Invulnerable = false;
                    EnterPhase(ResolvePhaseIndex());
                }
                return;
            }

            int desired = ResolvePhaseIndex();
            if (desired != _phaseIndex) EnterPhase(desired);

            _phaseMovement?.Tick(this, dt);
            for (int i = 0; i < _runners.Count; i++) _runners[i].Tick(this, dt);
        }

        private int ResolvePhaseIndex()
        {
            if (_boss == null || _boss.Phases == null || _boss.Phases.Length == 0) return -1;
            float hp = Health.MaxHull > 0f ? Health.Hull / Health.MaxHull : 0f;
            int best = 0;
            for (int i = 0; i < _boss.Phases.Length; i++)
                if (hp <= _boss.Phases[i].HealthThreshold + 0.0001f) best = i;
            return best;
        }

        private void EnterPhase(int index)
        {
            _phaseIndex = index;
            _runners.Clear();
            if (frontLaser != null) frontLaser.Hide();
            if (index < 0) return;

            var phase = _boss.Phases[index];
            if (_phaseMovement == null || _phaseMovementKind != phase.Movement)
            {
                _phaseMovementKind = phase.Movement;
                _phaseMovement = MovementStrategies.Create(phase.Movement);
            }
            _phaseMovement.Begin(this);

            if (phase.Attacks != null)
                for (int i = 0; i < phase.Attacks.Length; i++)
                    _runners.Add(new AttackRunner(phase.Attacks[i], i));

            if (index > 0)
            {
                Flash(Color.white);
                GameSignals.RaiseStageMessage(string.IsNullOrEmpty(phase.Name) ? $"PHASE {index + 1}" : phase.Name, 1.2f);
                if (Context != null && Context.CameraShake != null) Context.CameraShake.Shake(0.25f, 0.4f);
            }
        }

        protected override void HandleDeath(DamageSource source)
        {
            if (frontLaser != null) frontLaser.Hide();
            Projectile.ClearEnemyProjectiles();
            if (Context != null)
            {
                if (Context.Vfx != null)
                    Context.Vfx.SpawnExplosionChain(transform.position, _boss != null ? _boss.DeathExplosions : 8,
                        transform.localScale.x * 0.8f, _boss != null ? _boss.DeathSequenceSeconds : 1.5f, Definition.ExplosionColor);
                if (Context.CameraShake != null) Context.CameraShake.Shake(0.6f, 1.2f);
            }
            GameSignals.RaiseBossDefeated(this);
            base.HandleDeath(source);
        }

        // ---- Attack runners ----------------------------------------------------------------------------

        /// <summary>Per-attack timer state. Bosses run several of these at once.</summary>
        private sealed class AttackRunner
        {
            private readonly BossAttack _attack;
            private float _timer;
            private int _burstLeft;
            private float _burstTimer;
            private float _ringPhase;
            private int _side;
            private float _laserTimer;
            private int _laserState; // 0 idle, 1 telegraph, 2 firing

            public AttackRunner(BossAttack attack, int index)
            {
                _attack = attack;
                var p = attack.Settings;
                _timer = p.InitialDelay > 0f ? p.InitialDelay : p.Interval * (0.4f + 0.3f * index);
                _ringPhase = 0f;
            }

            public void Tick(BossController boss, float dt)
            {
                var p = _attack.Settings;
                if (_attack.Kind == BossAttackKind.FrontLaser)
                {
                    TickLaser(boss, dt);
                    return;
                }

                if (_burstLeft > 0)
                {
                    _burstTimer -= dt;
                    if (_burstTimer <= 0f)
                    {
                        FireOnce(boss, p);
                        _burstLeft--;
                        _burstTimer = p.BurstInterval;
                    }
                    return;
                }

                _timer -= dt;
                if (_timer > 0f) return;
                _timer = p.Interval;
                _burstLeft = Mathf.Max(1, p.BurstCount);
                _burstTimer = 0f;
            }

            private void FireOnce(BossController boss, in AttackParams p)
            {
                Vector2 center = boss.MuzzlePosition;
                switch (_attack.Kind)
                {
                    case BossAttackKind.Ring:
                        boss.FireRing(center, p, Mathf.Max(4, p.Count), _ringPhase);
                        _ringPhase += 360f / Mathf.Max(4, p.Count) * 0.37f;
                        break;
                    case BossAttackKind.Aimed:
                        boss.FireSpread(center, boss.DirectionToTarget(center), p, p.Count, p.SpreadAngle);
                        break;
                    case BossAttackKind.Forward:
                        boss.FireSpread(center, Vector2.down, p, p.Count, p.SpreadAngle);
                        break;
                    case BossAttackKind.SideCannons:
                    {
                        _side = 1 - _side;
                        float offset = _attack.MuzzleOffset > 0f ? _attack.MuzzleOffset : 1.2f;
                        Vector2 muzzle = (Vector2)boss.transform.position + new Vector2(_side == 0 ? -offset : offset, -0.2f);
                        boss.FireSpread(muzzle, boss.DirectionToTarget(muzzle), p, p.Count, p.SpreadAngle);
                        break;
                    }
                    case BossAttackKind.Missiles:
                    {
                        float offset = _attack.MuzzleOffset > 0f ? _attack.MuzzleOffset : 1.0f;
                        var spec = boss.BuildSpec(p);
                        spec.Homing = true;
                        spec.TurnRateDegrees = _attack.HomingTurnRate > 0f ? _attack.HomingTurnRate : 70f;
                        spec.HomingTarget = boss.Target;
                        spec.Type = DamageType.Explosive;
                        Vector2 left = (Vector2)boss.transform.position + new Vector2(-offset, 0f);
                        Vector2 right = (Vector2)boss.transform.position + new Vector2(offset, 0f);
                        boss.FireProjectile(left, new Vector2(-0.6f, -1f), spec);
                        boss.FireProjectile(right, new Vector2(0.6f, -1f), spec);
                        AudioManager.PlaySfx(SfxId.EnemyShot, 0.7f);
                        break;
                    }
                }
            }

            private void TickLaser(BossController boss, float dt)
            {
                var laser = boss.frontLaser;
                if (laser == null) return;
                var p = _attack.Settings;

                switch (_laserState)
                {
                    case 0:
                        _timer -= dt;
                        if (_timer <= 0f)
                        {
                            _laserState = 1;
                            _laserTimer = 0f;
                            AudioManager.PlaySfx(SfxId.LaserCharge);
                        }
                        break;
                    case 1:
                    {
                        float telegraph = Mathf.Max(0.3f, _attack.TelegraphSeconds);
                        _laserTimer += dt;
                        laser.ShowTelegraph(Mathf.Clamp01(_laserTimer / telegraph));
                        if (_laserTimer >= telegraph)
                        {
                            _laserState = 2;
                            _laserTimer = 0f;
                            laser.Fire(_attack.BeamDamagePerSecond > 0f ? _attack.BeamDamagePerSecond : 40f);
                            AudioManager.PlaySfx(SfxId.Ultimate, 0.5f);
                            if (boss.Context != null && boss.Context.CameraShake != null) boss.Context.CameraShake.Shake(0.15f, 0.3f);
                        }
                        break;
                    }
                    case 2:
                        _laserTimer += dt;
                        if (_laserTimer >= Mathf.Max(0.2f, _attack.BeamSeconds))
                        {
                            laser.Hide();
                            _laserState = 0;
                            _timer = p.Interval;
                        }
                        break;
                }
            }
        }
    }
}
