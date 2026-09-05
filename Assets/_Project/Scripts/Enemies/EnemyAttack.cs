using UnityEngine;

namespace Starfall.Enemies
{
    /// <summary>Strategy for how an enemy attacks. Instances are pooled with the enemy and reset on Begin.</summary>
    public interface IAttackStrategy
    {
        void Begin(Enemy enemy);
        void Tick(Enemy enemy, float deltaTime);
    }

    public static class AttackStrategies
    {
        public static IAttackStrategy Create(AttackKind kind)
        {
            return kind == AttackKind.None ? null : new PatternAttack(kind);
        }
    }

    /// <summary>
    /// Timer-driven projectile attack. Handles forward shots, aimed shots, spreads, bursts, rings, streams and webs
    /// from a single parameter block so every regular enemy shares one implementation.
    /// </summary>
    public sealed class PatternAttack : IAttackStrategy
    {
        private readonly AttackKind _kind;
        private float _timer;
        private int _burstLeft;
        private float _burstTimer;
        private float _ringPhase;
        private float _sweep;

        public PatternAttack(AttackKind kind)
        {
            _kind = kind;
        }

        public void Begin(Enemy enemy)
        {
            var p = enemy.AttackSettings;
            _timer = p.InitialDelay > 0f ? p.InitialDelay : p.Interval * 0.5f;
            _burstLeft = 0;
            _burstTimer = 0f;
            _ringPhase = Random.Range(0f, 360f);
            _sweep = 0f;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.AttackSettings;
            if (p.OnlyWhenOnScreen && !enemy.IsOnScreen) return;

            if (_burstLeft > 0)
            {
                _burstTimer -= dt;
                if (_burstTimer <= 0f)
                {
                    FireOnce(enemy, p);
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

        private void FireOnce(Enemy enemy, in AttackParams p)
        {
            Vector2 origin = enemy.MuzzlePosition;
            switch (_kind)
            {
                case AttackKind.Ring:
                    enemy.FireRing(origin, p, Mathf.Max(3, p.Count), _ringPhase);
                    _ringPhase += 360f / Mathf.Max(3, p.Count) * 0.5f;
                    break;
                case AttackKind.Aimed:
                    enemy.FireSpread(origin, enemy.DirectionToTarget(origin), p, p.Count, p.SpreadAngle);
                    break;
                case AttackKind.Stream:
                {
                    _sweep += 0.9f;
                    float angle = Mathf.Sin(_sweep) * Mathf.Max(10f, p.SpreadAngle);
                    enemy.FireSpread(origin, Combat.ProjectileLauncher.Rotate(Vector2.down, angle), p, 1, 0f);
                    break;
                }
                case AttackKind.Web:
                    enemy.FireWeb(origin, enemy.DirectionToTarget(origin), p);
                    break;
                default:
                    enemy.FireSpread(origin, Vector2.down, p, p.Count, p.SpreadAngle);
                    break;
            }
        }
    }
}
