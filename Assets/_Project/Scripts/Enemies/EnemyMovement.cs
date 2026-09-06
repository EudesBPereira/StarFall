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
                case MovementKind.Serpentine: return new SerpentineMovement();
                case MovementKind.Dash: return new DashMovement();
                case MovementKind.Hold: return new HoldMovement();
                case MovementKind.SideSweep: return new SideSweepMovement();
                case MovementKind.Blink: return new BlinkMovement();
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
            enemy.transform.position += Vector3.down * (enemy.Movement.Speed * enemy.SpeedMultiplier * dt);
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
            pos.y -= p.Speed * enemy.SpeedMultiplier * dt;
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
            pos += _direction * (p.Speed * enemy.SpeedMultiplier * dt);
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
            float speed = p.Speed * enemy.SpeedMultiplier;

            if (pos.y > holdY)
            {
                pos.y = Mathf.Max(holdY, pos.y - speed * dt);
            }
            else if (_holdTimer < p.HoldDuration)
            {
                _holdTimer += dt;
                pos.x += _direction * p.StrafeSpeed * enemy.SpeedMultiplier * dt;
                if (area != null)
                {
                    float pad = 0.6f;
                    if (pos.x < area.Left + pad) { pos.x = area.Left + pad; _direction = 1f; }
                    else if (pos.x > area.Right - pad) { pos.x = area.Right - pad; _direction = -1f; }
                }
            }
            else
            {
                pos.y -= speed * 1.5f * dt;
            }
            enemy.transform.position = pos;
        }
    }

    /// <summary>Turrets: descend to the hold line and stay (they leave only when destroyed or the wave times out).</summary>
    public sealed class HoldMovement : IMovementStrategy
    {
        public void Begin(Enemy enemy) { }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var area = enemy.Area;
            var pos = enemy.transform.position;
            float holdY = area != null ? area.Top - p.HoldHeight * area.Height : pos.y;
            if (pos.y > holdY) pos.y = Mathf.Max(holdY, pos.y - p.Speed * enemy.SpeedMultiplier * dt);
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
            _phase += p.Frequency * enemy.SpeedMultiplier * dt;
            var pos = enemy.transform.position;
            float amplitude = p.Amplitude;
            if (enemy.Area != null) amplitude = Mathf.Min(amplitude, enemy.Area.Width * 0.5f - 1f);
            pos.x = _originX + Mathf.Sin(_phase) * amplitude;
            enemy.transform.position = pos;
        }
    }

    /// <summary>Leviathan: figure-eight sweeps across the upper half of the screen.</summary>
    public sealed class SerpentineMovement : IMovementStrategy
    {
        private float _phase;
        private float _baseY;

        public void Begin(Enemy enemy)
        {
            _phase = 0f;
            _baseY = enemy.transform.position.y;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            _phase += p.Frequency * enemy.SpeedMultiplier * dt;
            var area = enemy.Area;
            float halfW = area != null ? area.Width * 0.5f - 1f : 4f;
            float ampX = Mathf.Min(p.Amplitude, halfW);
            float ampY = p.StrafeSpeed; // vertical amplitude for this kind
            float cx = area != null ? area.Center.x : 0f;
            var prev = (Vector2)enemy.transform.position;
            var pos = new Vector2(cx + Mathf.Sin(_phase) * ampX, _baseY + Mathf.Sin(_phase * 2f) * ampY);
            enemy.transform.position = pos;
            enemy.FaceDirection(pos - prev);
        }
    }

    /// <summary>Reaper Wing: dash to a random point in the upper area, pause, repeat.</summary>
    public sealed class DashMovement : IMovementStrategy
    {
        private Vector2 _target;
        private float _pause;
        private bool _dashing;

        public void Begin(Enemy enemy)
        {
            _pause = 0.4f;
            _dashing = false;
            _target = enemy.transform.position;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var area = enemy.Area;
            var pos = (Vector2)enemy.transform.position;
            if (!_dashing)
            {
                _pause -= dt;
                if (_pause > 0f) return;
                if (area != null)
                {
                    float minY = area.Top - p.HoldHeight * area.Height;
                    _target = new Vector2(area.LerpX(Random.value, 0.9f), Random.Range(minY - p.Amplitude, minY + p.Amplitude * 0.3f));
                }
                _dashing = true;
            }
            float speed = p.Speed * enemy.SpeedMultiplier;
            Vector2 next = Vector2.MoveTowards(pos, _target, speed * dt);
            enemy.FaceDirection((next - pos).sqrMagnitude > 0.0001f ? next - pos : Vector2.down);
            enemy.transform.position = next;
            if ((next - _target).sqrMagnitude < 0.01f)
            {
                _dashing = false;
                _pause = p.StrafeSpeed > 0f ? p.StrafeSpeed : 0.6f;
            }
        }
    }
}

namespace Starfall.Enemies
{
    /// <summary>Side formations: spawned at the left/right edge, crosses the screen horizontally with a wave.</summary>
    public sealed class SideSweepMovement : IMovementStrategy
    {
        private float _direction;
        private float _baseY;
        private float _phase;

        public void Begin(Enemy enemy)
        {
            var area = enemy.Area;
            _direction = area != null && enemy.transform.position.x > area.Center.x ? -1f : 1f;
            _baseY = enemy.transform.position.y;
            _phase = UnityEngine.Random.value * 6.28f;
            enemy.FaceDirection(new UnityEngine.Vector2(_direction, -0.2f));
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var pos = enemy.transform.position;
            pos.x += _direction * p.Speed * enemy.SpeedMultiplier * dt;
            pos.y = _baseY + UnityEngine.Mathf.Sin(enemy.Age * p.Frequency + _phase) * p.Amplitude * 0.5f - enemy.Age * 0.25f;
            enemy.transform.position = pos;
        }
    }

    /// <summary>Teleporting enemy: holds, blinks to a random point in the upper area, holds again.</summary>
    public sealed class BlinkMovement : IMovementStrategy
    {
        private float _timer;

        public void Begin(Enemy enemy)
        {
            _timer = enemy.Movement.HoldDuration > 0f ? enemy.Movement.HoldDuration : 2.5f;
        }

        public void Tick(Enemy enemy, float dt)
        {
            var p = enemy.Movement;
            var area = enemy.Area;
            _timer -= dt;
            // Drift slowly so the target is never perfectly static between blinks.
            var pos = enemy.transform.position;
            pos.x += UnityEngine.Mathf.Sin(enemy.Age * 1.3f) * p.StrafeSpeed * 0.3f * dt;
            enemy.transform.position = pos;
            if (_timer > 0f || area == null) return;
            _timer = p.HoldDuration > 0f ? p.HoldDuration : 2.5f;
            float minY = area.Top - p.HoldHeight * area.Height;
            var target = new UnityEngine.Vector3(area.LerpX(UnityEngine.Random.value, 1f), UnityEngine.Random.Range(minY - p.Amplitude, minY + p.Amplitude * 0.5f), 0f);
            var ctx = Core.GameplayContext.Current;
            if (ctx != null && ctx.Vfx != null)
            {
                ctx.Vfx.SpawnShockwave(enemy.transform.position, 1.6f, new UnityEngine.Color(0.8f, 0.5f, 1f));
                ctx.Vfx.SpawnShockwave(target, 1.6f, new UnityEngine.Color(0.8f, 0.5f, 1f));
            }
            enemy.transform.position = target;
        }
    }
}
