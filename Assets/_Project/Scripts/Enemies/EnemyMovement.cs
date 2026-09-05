using UnityEngine;

namespace Starfall.Enemies
{
    /// <summary>Strategy for how an enemy moves. Instances are pooled with the enemy and reset on Begin.</summary>
    public interface IMovementStrategy
    {
        void Begin(Enemy enemy);
        void Tick(Enemy enemy, float deltaTime);
    }

    public static class MovementStrategies
    {
        public static IMovementStrategy Create(MovementKind kind)
        {
            switch (kind)
            {
                case MovementKind.Weave: return new WeaveMovement();
                case MovementKind.Chase: return new ChaseMovement();
                case MovementKind.HoverStrafe: return new HoverStrafeMovement();
                case MovementKind.LateralPatrol: return new LateralPatrolMovement();
                default: return new StraightDownMovement();
            }
        }
    }

    /// <summary>Drone / Bomber / asteroid: constant vertical speed.</summary>
    public sealed class StraightDownMovement : IMovementStrategy
    {
        public void Begin(Enemy enemy) { }

        public void Tick(Enemy enemy, float dt)
        {
            enemy.transform.position += Vector3.down * (enemy.Movement.Speed * dt);
        }
    }

    /// <summary>Interceptor: descends while weaving sideways.</summary>
    public sealed class WeaveMovement : IMovementStrategy
    {
        private float _originX;
        private float _phase;

        public void Begin(Enemy enemy)
        {
            _originX = enemy.transform.position.x;
            _phase = Random.value * Mathf.PI * 2f;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var pos = enemy.transform.position;
            pos.y -= p.Speed * dt;
            pos.x = _originX + Mathf.Sin(enemy.Age * p.Frequency + _phase) * p.Amplitude;
            if (enemy.Area != null)
                pos.x = Mathf.Clamp(pos.x, enemy.Area.Left + 0.4f, enemy.Area.Right - 0.4f);
            enemy.transform.position = pos;
        }
    }

    /// <summary>Kamikaze: steers towards the player, keeps going once it passes below the ship.</summary>
    public sealed class ChaseMovement : IMovementStrategy
    {
        private Vector2 _direction;

        public void Begin(Enemy enemy)
        {
            _direction = Vector2.down;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var pos = (Vector2)enemy.transform.position;
            var target = enemy.Target;
            if (target != null && target.position.y < pos.y - 0.1f)
            {
                Vector2 toTarget = ((Vector2)target.position - pos).normalized;
                float maxRad = p.TurnRate * Mathf.Deg2Rad * dt;
                _direction = Vector3.RotateTowards(_direction, toTarget, maxRad, 0f);
            }
            pos += _direction * (p.Speed * dt);
            enemy.transform.position = pos;
            enemy.FaceDirection(_direction);
        }
    }

    /// <summary>Shield enemy: descends to a hold line, strafes for a while, then leaves.</summary>
    public sealed class HoverStrafeMovement : IMovementStrategy
    {
        private float _holdTimer;
        private float _direction;

        public void Begin(Enemy enemy)
        {
            _holdTimer = 0f;
            _direction = Random.value < 0.5f ? -1f : 1f;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var area = enemy.Area;
            var pos = enemy.transform.position;
            float holdY = area != null ? area.Top - p.HoldHeight * area.Height : pos.y;

            if (pos.y > holdY)
            {
                pos.y = Mathf.Max(holdY, pos.y - p.Speed * dt);
            }
            else if (_holdTimer < p.HoldDuration)
            {
                _holdTimer += dt;
                pos.x += _direction * p.StrafeSpeed * dt;
                if (area != null)
                {
                    float pad = 0.6f;
                    if (pos.x < area.Left + pad) { pos.x = area.Left + pad; _direction = 1f; }
                    else if (pos.x > area.Right - pad) { pos.x = area.Right - pad; _direction = -1f; }
                }
            }
            else
            {
                pos.y -= p.Speed * 1.5f * dt;
            }
            enemy.transform.position = pos;
        }
    }

    /// <summary>Bosses: hold vertical position and sweep sideways.</summary>
    public sealed class LateralPatrolMovement : IMovementStrategy
    {
        private float _originX;
        private float _phase;

        public void Begin(Enemy enemy)
        {
            _originX = enemy.Area != null ? enemy.Area.Center.x : enemy.transform.position.x;
            _phase = 0f;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            _phase += p.Frequency * dt;
            var pos = enemy.transform.position;
            float amplitude = p.Amplitude;
            if (enemy.Area != null) amplitude = Mathf.Min(amplitude, enemy.Area.Width * 0.5f - 1f);
            pos.x = _originX + Mathf.Sin(_phase) * amplitude;
            enemy.transform.position = pos;
        }
    }
}
