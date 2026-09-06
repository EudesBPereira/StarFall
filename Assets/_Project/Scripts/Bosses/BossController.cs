using System.Collections.Generic;
using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.Waves;
using UnityEngine;

namespace Starfall.Bosses
{
    /// <summary>
    /// Boss = enemy with an entrance, health-driven phases (with optional transformations), several concurrent
    /// attack patterns, an optional segmented body and optional destructible parts (weak points / disable attacks).
    /// Every boss and mini-boss is a <see cref="BossDefinition"/>.
    /// </summary>
    public sealed class BossController : Enemy
    {
        [SerializeField] internal LaserBeam frontLaser;
        [SerializeField] internal Transform segmentRoot;

        private BossDefinition _boss;
        private float _entranceTimer;
        private bool _entering;
        private float _entranceStartY;
        private float _entranceTargetY;
        private int _phaseIndex = -1;
        private float _transformTimer;
        private Color _phaseTint = Color.white;
        private IMovementStrategy _phaseMovement;
        private MovementKind _phaseMovementKind;
        private readonly List<AttackRunner> _runners = new List<AttackRunner>(4);
        private readonly List<SpriteRenderer> _segments = new List<SpriteRenderer>(12);
        private readonly List<Vector3> _trail = new List<Vector3>(256);
        private readonly List<BossPart> _parts = new List<BossPart>(6);
        private readonly HashSet<int> _disabledAttacks = new HashSet<int>();
        private bool _coreShielded;
        private float _statMultiplier = 1f;

        public BossDefinition Boss => _boss;
        public bool IsEntering => _entering;
        public int PhaseIndex => _phaseIndex;
        public bool CoreShielded => _coreShielded;
        public int PartsAlive
        {
            get { int n = 0; for (int i = 0; i < _parts.Count; i++) if (_parts[i] != null && _parts[i].IsAlive) n++; return n; }
        }

        /// <summary>Movement parameters come from the active phase instead of the base definition.</summary>
        public override MovementParams Movement => _phaseIndex >= 0 && _boss != null && _phaseIndex < _boss.Phases.Length
            ? _boss.Phases[_phaseIndex].MovementSettings
            : base.Movement;

        protected override Color CurrentTint => _phaseTint;

        public override void Initialize(EnemyDefinition definition, GameplayContext ctx, EnemySpawner spawner, float statMultiplier = 1f, float speedMultiplier = 1f)
        {
            base.Initialize(definition, ctx, spawner, statMultiplier, speedMultiplier);
            _boss = definition as BossDefinition;
            _statMultiplier = Mathf.Max(0.1f, statMultiplier);
            _phaseIndex = -1;
            _phaseTint = definition.Tint;
            _runners.Clear();
            _trail.Clear();
            _disabledAttacks.Clear();

            var area = Area;
            _entranceStartY = area != null ? area.Top + 3f : transform.position.y;
            _entranceTargetY = area != null ? area.Top - (_boss != null ? _boss.EntranceHeight : 0.2f) * area.Height : transform.position.y;
            var pos = transform.position;
            pos.x = area != null ? area.Center.x : pos.x;
            pos.y = _entranceStartY;
            transform.position = pos;

            _entering = true;
            _entranceTimer = 0f;
            _transformTimer = 0f;

            if (frontLaser != null)
            {
                float length = area != null ? area.Height + 4f : 24f;
                frontLaser.Configure(1f, length, new Color(1f, 0.35f, 0.55f, 0.95f));
            }
            BuildSegments();
            BuildParts();
            RefreshInvulnerability();

            GameSignals.RaiseBossSpawned(this);
            if (ctx != null && ctx.CameraShake != null) ctx.CameraShake.Shake(0.3f, 1.2f);
        }

        /// <summary>Core is untouchable while entering, transforming, or protected by a shielding part.</summary>
        private void RefreshInvulnerability()
        {
            Health.Invulnerable = _entering || _transformTimer > 0f || _coreShielded;
        }

        protected override void Update()
        {
            if (!IsActiveInstance) return;
            float dt = Time.deltaTime;
            TickStatus(dt);
            if (IsStunned && !_entering)
            {
                if (frontLaser != null) frontLaser.Hide();
                UpdateSegments(dt);
                return;
            }

            if (_entering)
            {
                _entranceTimer += dt;
                float duration = _boss != null ? _boss.EntranceDuration : 2.5f;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_entranceTimer / duration));
                var pos = transform.position;
                pos.y = Mathf.Lerp(_entranceStartY, _entranceTargetY, k);
                transform.position = pos;
                UpdateSegments(dt);
                if (k >= 1f)
                {
                    _entering = false;
                    RefreshInvulnerability();
                    EnterPhase(ResolvePhaseIndex());
                }
                return;
            }

            int desired = ResolvePhaseIndex();
            if (desired != _phaseIndex) EnterPhase(desired);

            if (_transformTimer > 0f)
            {
                _transformTimer -= dt;
                if (_transformTimer <= 0f) RefreshInvulnerability();
                float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 30f);
                if (body != null) body.transform.localScale = Vector3.one * pulse;
                UpdateSegments(dt);
                return;
            }

            _phaseMovement?.Tick(this, dt);
            for (int i = 0; i < _runners.Count; i++)
                if (!_disabledAttacks.Contains(_runners[i].Index)) _runners[i].Tick(this, dt);
            UpdateSegments(dt);
            if (_coreShielded && shieldVisual != null && !shieldVisual.enabled)
            {
                shieldVisual.enabled = true;
                shieldVisual.color = new Color(0.8f, 0.5f, 1f, 0.6f);
            }
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
            bool first = _phaseIndex < 0;
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

            // Transformation: new look + short invulnerable "morph" window.
            bool transforms = phase.Sprite != null || phase.Scale > 0f || phase.Tint.a > 0f;
            if (transforms)
            {
                _phaseTint = phase.Tint.a > 0f ? phase.Tint : _boss.Tint;
                ApplyVisual(phase.Sprite, _phaseTint, phase.Scale);
                if (body != null) body.transform.localScale = Vector3.one;
            }

            if (!first)
            {
                Flash(Color.white);
                Projectile.ClearEnemyProjectiles();
                GameSignals.RaiseStageMessage(string.IsNullOrEmpty(phase.Name) ? $"PHASE {index + 1}" : phase.Name, 1.4f);
                GameSignals.RaiseBossPhaseChanged(this, index);
                AudioManager.PlaySfx(SfxId.BossWarning, 0.7f);
                if (Context != null)
                {
                    if (Context.CameraShake != null) Context.CameraShake.Shake(0.3f, 0.5f);
                    if (Context.Vfx != null) Context.Vfx.SpawnShockwave(transform.position, 6f, _phaseTint);
                }
                if (transforms && phase.TransformSeconds > 0f)
                {
                    _transformTimer = phase.TransformSeconds;
                    RefreshInvulnerability();
                }
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
            for (int i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] == null) continue;
                if (Context != null && Context.Vfx != null) Context.Vfx.SpawnExplosion(_segments[i].transform.position, 1f, Definition.ExplosionColor);
                _segments[i].enabled = false;
            }
            for (int i = 0; i < _parts.Count; i++) if (_parts[i] != null) _parts[i].Hide();
            GameSignals.RaiseBossDefeated(this);
            base.HandleDeath(source);
        }

        // ---- Destructible parts ------------------------------------------------------------------------

        private void BuildParts()
        {
            for (int i = 0; i < _parts.Count; i++) if (_parts[i] != null) _parts[i].Hide();
            _coreShielded = false;
            if (_boss == null || _boss.Parts == null || _boss.Parts.Length == 0) return;

            while (_parts.Count < _boss.Parts.Length)
            {
                var go = new GameObject("Part" + _parts.Count) { layer = GameLayers.Enemy };
                go.transform.SetParent(transform, false);
                var health = go.AddComponent<Health>();
                health.faction = Faction.Enemy;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                var spriteGo = new GameObject("Body");
                spriteGo.transform.SetParent(go.transform, false);
                var sr = spriteGo.AddComponent<SpriteRenderer>();
                var part = go.AddComponent<BossPart>();
                part.body = sr;
                part.hitbox = col;
                part.Destroyed += OnPartDestroyed;
                _parts.Add(part);
            }
            for (int i = 0; i < _boss.Parts.Length; i++)
            {
                var spec = _boss.Parts[i];
                if (spec.Sprite == null) spec.Sprite = Definition.Sprite;
                _parts[i].Setup(i, spec, _statMultiplier);
                if (spec.ShieldsCore) _coreShielded = true;
            }
            if (shieldVisual != null && _coreShielded)
            {
                shieldVisual.enabled = true;
                shieldVisual.color = new Color(0.8f, 0.5f, 1f, 0.6f);
            }
        }

        private void OnPartDestroyed(BossPart part)
        {
            if (!IsActiveInstance || part == null) return;
            var spec = part.Spec;
            if (Context != null && Context.Vfx != null)
            {
                Context.Vfx.SpawnExplosion(part.transform.position, 1.2f, Definition.ExplosionColor);
                Context.Vfx.SpawnFloatingText(part.transform.position + Vector3.up * 0.5f, string.IsNullOrEmpty(spec.Name) ? "PART DESTROYED" : spec.Name.ToUpperInvariant(), new Color(1f, 0.85f, 0.3f), 0.9f);
            }
            if (spec.DisablesAttack >= 0) _disabledAttacks.Add(spec.DisablesAttack);
            if (spec.DisablesAttack >= 0 && frontLaser != null)
            {
                // A disabled laser attack must not keep a lit beam around.
                for (int i = 0; i < _runners.Count; i++)
                    if (_runners[i].Index == spec.DisablesAttack && _runners[i].IsLaser) frontLaser.Hide();
            }
            GameSignals.RaiseBossPartDestroyed(part.transform.position, spec.Score);
            AudioManager.PlaySfx(SfxId.ExplosionSmall, 0.9f);

            if (spec.ShieldsCore)
            {
                bool any = false;
                for (int i = 0; i < _parts.Count; i++)
                    if (_parts[i] != null && _parts[i].IsAlive && _parts[i].Spec.ShieldsCore) any = true;
                if (!any && _coreShielded)
                {
                    _coreShielded = false;
                    RefreshInvulnerability();
                    if (shieldVisual != null) shieldVisual.enabled = false;
                    GameSignals.RaiseStageMessage("CORE EXPOSED", 1.4f);
                    if (Context != null && Context.Vfx != null) Context.Vfx.SpawnShieldBreak(transform.position, new Color(0.8f, 0.5f, 1f));
                }
            }
        }

        // ---- Segmented body (Leviathan) ----------------------------------------------------------------

        private void BuildSegments()
        {
            for (int i = 0; i < _segments.Count; i++) if (_segments[i] != null) _segments[i].enabled = false;
            if (_boss == null || _boss.SegmentCount <= 0) return;
            if (segmentRoot == null)
            {
                var root = new GameObject("Segments");
                root.transform.SetParent(transform.parent, false);
                segmentRoot = root.transform;
            }
            while (_segments.Count < _boss.SegmentCount)
            {
                var go = new GameObject("Segment" + _segments.Count);
                go.transform.SetParent(segmentRoot, false);
                var sr = go.AddComponent<SpriteRenderer>();
                _segments.Add(sr);
            }
            for (int i = 0; i < _boss.SegmentCount; i++)
            {
                var sr = _segments[i];
                sr.enabled = true;
                sr.sprite = _boss.SegmentSprite != null ? _boss.SegmentSprite : _boss.Sprite;
                float t = (float)i / Mathf.Max(1, _boss.SegmentCount - 1);
                sr.color = Color.Lerp(_boss.Tint, _boss.Tint * 0.55f, t);
                sr.sortingOrder = SortingOrders.Enemy - 1 - i;
                sr.transform.localScale = Vector3.one * (_boss.SegmentScale * _boss.Scale * Mathf.Lerp(1f, 0.6f, t));
                sr.transform.position = transform.position;
            }
        }

        private void UpdateSegments(float dt)
        {
            if (_boss == null || _boss.SegmentCount <= 0) return;
            var head = transform.position;
            if (_trail.Count == 0 || (_trail[_trail.Count - 1] - head).sqrMagnitude > 0.0025f)
            {
                _trail.Add(head);
                if (_trail.Count > 240) _trail.RemoveAt(0);
            }
            float spacing = _boss.SegmentSpacing * _boss.Scale;
            for (int s = 0; s < _boss.SegmentCount && s < _segments.Count; s++)
            {
                float wanted = spacing * (s + 1);
                Vector3 p = PointAlongTrail(wanted, out Vector3 dir);
                var t = _segments[s].transform;
                t.position = p;
                if (dir.sqrMagnitude > 0.0001f) t.rotation = Quaternion.FromToRotation(Vector3.down, -dir);
            }
        }

        private Vector3 PointAlongTrail(float distance, out Vector3 direction)
        {
            direction = Vector3.down;
            if (_trail.Count == 0) return transform.position;
            float acc = 0f;
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                Vector3 a = _trail[i];
                Vector3 b = _trail[i - 1];
                float seg = Vector3.Distance(a, b);
                if (acc + seg >= distance)
                {
                    float k = seg > 0f ? (distance - acc) / seg : 0f;
                    direction = (a - b).normalized;
                    return Vector3.Lerp(a, b, k);
                }
                acc += seg;
            }
            Vector3 last = _trail[0];
            return last + Vector3.up * (distance - acc);
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
            private float _sweep;

            public int Index { get; }
            public bool IsLaser => _attack.Kind == BossAttackKind.FrontLaser;

            public AttackRunner(BossAttack attack, int index)
            {
                _attack = attack;
                Index = index;
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
                    case BossAttackKind.Stream:
                    {
                        _sweep += 0.7f;
                        float angle = Mathf.Sin(_sweep) * Mathf.Max(10f, p.SpreadAngle);
                        boss.FireSpread(center, ProjectileLauncher.Rotate(Vector2.down, angle), p, 1, 0f);
                        break;
                    }
                    case BossAttackKind.Web:
                        boss.FireWeb(center, boss.DirectionToTarget(center), p);
                        break;
                    case BossAttackKind.Summon:
                    {
                        if (_attack.SummonEnemy == null || boss.Context == null) break;
                        int alive = boss.Context.Enemies.BlockingCount - 1;
                        if (_attack.SummonCap > 0 && alive >= _attack.SummonCap) break;
                        boss.Summon(_attack.SummonEnemy, Mathf.Max(1, p.Count));
                        break;
                    }
                    case BossAttackKind.RiftSpawn:
                    {
                        if (_attack.SummonEnemy == null || boss.Context == null) break;
                        int alive = boss.Context.Enemies.BlockingCount - 1;
                        if (_attack.SummonCap > 0 && alive >= _attack.SummonCap) break;
                        boss.OpenRifts(_attack.SummonEnemy, Mathf.Max(1, p.Count));
                        break;
                    }
                    case BossAttackKind.Mines:
                    {
                        var spec = boss.BuildSpec(p);
                        spec.Type = DamageType.Explosive;
                        spec.SplashRadius = p.SplashRadius > 0f ? p.SplashRadius : 1.3f;
                        spec.DetonateOnExpire = true;
                        spec.StopAfterSeconds = 0.9f;
                        spec.Scale = Mathf.Max(spec.Scale, 1.4f);
                        spec.Color = new Color(1f, 0.55f, 0.2f);
                        for (int i = 0; i < Mathf.Max(1, p.Count); i++)
                        {
                            Vector2 dir = ProjectileLauncher.Rotate(Vector2.down, Random.Range(-60f, 60f));
                            boss.FireProjectile(center, dir, spec);
                        }
                        AudioManager.PlaySfx(SfxId.EnemyShot, 0.6f);
                        break;
                    }
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
                        AudioManager.PlaySfx(SfxId.Missile, 0.7f);
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

        /// <summary>Rift Walker: telegraphed portals at random on-screen points that spawn minions.</summary>
        public void OpenRifts(EnemyDefinition minion, int count)
        {
            if (Context == null) return;
            StartCoroutine(RiftRoutine(minion, count));
        }

        private System.Collections.IEnumerator RiftRoutine(EnemyDefinition minion, int count)
        {
            var area = Area;
            var points = new List<Vector2>(count);
            for (int i = 0; i < count; i++)
            {
                var p = new Vector2(area.LerpX(Random.value, 1f), area.LerpY(Random.Range(0.45f, 0.85f)));
                points.Add(p);
                if (Context.Vfx != null) Context.Vfx.SpawnShockwave(p, 2.2f, new Color(0.8f, 0.5f, 1f));
            }
            AudioManager.PlaySfx(SfxId.Summon, 0.7f);
            yield return new WaitForSeconds(1f);
            if (!IsActiveInstance) yield break;
            for (int i = 0; i < points.Count; i++)
            {
                if (Context.Vfx != null) Context.Vfx.SpawnPickupBurst(points[i], new Color(0.8f, 0.5f, 1f));
                Context.Spawner.Spawn(minion, points[i]);
            }
        }
    }
}
