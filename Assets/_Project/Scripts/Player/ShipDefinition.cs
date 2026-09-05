using Starfall.Combat;
using UnityEngine;

namespace Starfall.Player
{
    /// <summary>Playable ship data (MVP: SF-01 Vanguard). Values documented in docs/BALANCING.md.</summary>
    [CreateAssetMenu(menuName = "Starfall/Ships/Ship Definition", fileName = "Ship")]
    public sealed class ShipDefinition : ScriptableObject
    {
        public string DisplayName = "SF-01 Vanguard";
        [TextArea] public string Description = "Experimental interceptor of the Earth Defense Fleet.";

        [Header("Durability")]
        [Min(1f)] public float MaxHull = 100f;
        [Min(0f)] public float MaxShield = 50f;

        [Header("Movement")]
        [Min(0.1f)] public float MoveSpeed = 9f;
        [Min(0.1f)] public float Acceleration = 60f;
        [Min(0.1f)] public float Deceleration = 80f;
        [Tooltip("Bank angle (degrees) at full horizontal speed.")]
        [Min(0f)] public float BankAngle = 18f;

        [Header("Weapon")]
        public WeaponDefinition Weapon;

        [Header("Visuals")]
        public Sprite Sprite;
        public Color Tint = Color.white;
        [Min(0.05f)] public float HitboxRadius = 0.28f;
    }
}
