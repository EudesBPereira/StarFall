using Starfall.Core;
using UnityEngine;

namespace Starfall.Waves
{
    /// <summary>Converts spawn patterns into world positions just above the visible area.</summary>
    public static class SpawnPositionResolver
    {
        private const float SidePadding = 0.7f;

        public static Vector2 Resolve(PlayArea area, SpawnPattern pattern, float patternValue, int index, int count, float spawnHeightOffset = 1.2f)
        {
            float y = area.Top + spawnHeightOffset;
            float x;
            switch (pattern)
            {
                case SpawnPattern.TopCenter:
                    x = area.Center.x;
                    break;
                case SpawnPattern.TopLeft:
                    x = area.LerpX(0.2f, SidePadding);
                    break;
                case SpawnPattern.TopRight:
                    x = area.LerpX(0.8f, SidePadding);
                    break;
                case SpawnPattern.TopLine:
                    x = count <= 1 ? area.Center.x : area.LerpX((float)index / (count - 1), SidePadding);
                    break;
                case SpawnPattern.TopVee:
                {
                    int ring = (index + 1) / 2;
                    float side = index == 0 ? 0f : (index % 2 == 1 ? -1f : 1f);
                    float spread = Mathf.Min(area.Width * 0.5f - SidePadding, ring * 1.1f);
                    x = area.Center.x + side * spread;
                    y += ring * 0.9f;
                    break;
                }
                case SpawnPattern.TopFixed:
                    x = area.LerpX(patternValue, SidePadding);
                    break;
                case SpawnPattern.TopAlternate:
                    x = area.LerpX(index % 2 == 0 ? 0.25f : 0.75f, SidePadding);
                    break;
                default:
                    x = area.LerpX(Random.value, SidePadding);
                    break;
            }
            return new Vector2(x, y);
        }
    }
}
