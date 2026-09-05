using Starfall.Logic;
using UnityEngine;

namespace Starfall.Combat
{
    /// <summary>Anything that can receive damage: player, enemies, bosses, obstacles.</summary>
    public interface IDamageable
    {
        Faction Faction { get; }
        bool IsAlive { get; }
        DamageResult ApplyDamage(in DamageInfo info, Vector2 hitPoint);
    }
}
