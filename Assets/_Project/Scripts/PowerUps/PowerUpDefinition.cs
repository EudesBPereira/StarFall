using Starfall.Logic;
using Starfall.Pooling;
using UnityEngine;

namespace Starfall.PowerUps
{
    /// <summary>Configurable power-up item. Rules and values in docs/BALANCING.md and docs/DECISIONS.md.</summary>
    [CreateAssetMenu(menuName = "Starfall/Power-Ups/Power-Up Definition", fileName = "PowerUp")]
    public sealed class PowerUpDefinition : ScriptableObject
    {
        public PowerUpKind Kind;
        public string DisplayName = "Power-Up";
        [Tooltip("Short text shown when collected.")]
        public string Label = "POWER";
        public Color Color = Color.white;
        [Tooltip("Seconds for timed effects. Ignored by instant effects.")]
        [Min(0f)] public float Duration = 8f;
        [Tooltip("Meaning depends on Kind: shield points, energy points, speed/damage multiplier.")]
        [Min(0f)] public float Magnitude = 1f;
        [Tooltip("Relative weight inside a drop table.")]
        [Min(0f)] public float Weight = 1f;
        public PooledObject PickupPrefab;
        [Min(0.1f)] public float FallSpeed = 1.6f;
        [Min(0f)] public float SwayAmplitude = 0.4f;
    }
}
