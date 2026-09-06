using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Logic;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>
    /// Cyber faction assistant (plan §6): orbits the ship and fires at the nearest enemy on its own.
    /// Marks what it hits so the ship's main weapon deals bonus damage.
    /// </summary>
    public sealed class CompanionDrone : MonoBehaviour
    {
        [SerializeField] internal SpriteRenderer body;
        [SerializeField] internal float orbitRadius = 0.9f;
        [SerializeField] internal float orbitSpeed = 2.2f;

        private PlayerShip _ship;
        private float _damage = 4f;
        private float _interval = 0.5f;
        private float _timer;
        private float _angle;
        private bool _active;

        public void Configure(PlayerShip ship, float damage, float interval, Color tint)
        {
            _ship = ship;
            _damage = damage;
            _interval = interval;
            if (body != null) body.color = tint;
        }

        public void SetActive(bool active)
        {
            _active = active;
            if (body != null) body.enabled = active;
            _timer = _interval * 0.5f;
        }

        private void Update()
        {
            if (!_active || _ship == null || !_ship.IsAlive || !_ship.ControlEnabled) return;
            float dt = Time.deltaTime;
            _angle += orbitSpeed * dt;
            transform.position = _ship.transform.position + new Vector3(Mathf.Cos(_angle) * orbitRadius, 0.35f + Mathf.Sin(_angle) * orbitRadius * 0.5f, 0f);

            _timer -= dt;
            if (_timer > 0f) return;
            var ctx = GameplayContext.Current;
            if (ctx == null || ctx.Enemies == null) return;
            var target = ctx.Enemies.Nearest(transform.position, true);
            if (target == null) return;
            _timer = _interval / (_ship.Risk != null && _ship.Risk.Overdrive.IsActive ? 1.5f : 1f);

            var spec = new ProjectileSpec
            {
                Speed = 20f,
                Damage = _damage * (_ship.Loadout.DamageMultiplier),
                Lifetime = 1.6f,
                Scale = 0.55f,
                Color = new Color(0.14f, 0.84f, 1f),
                Faction = Faction.Player,
                Source = DamageSource.Player,
                Type = DamageType.Energy,
                MarkSeconds = FactionRules.MarkSeconds,
            };
            Vector2 dir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
            var prefab = _ship.Loadout.Weapon != null ? _ship.Loadout.Weapon.ProjectilePrefab : null;
            if (prefab == null) return;
            ProjectileLauncher.Fire(ctx.Pools, prefab, spec, transform.position, dir);
            AudioManager.PlaySfx(SfxId.Laser, 0.25f);
        }
    }
}
