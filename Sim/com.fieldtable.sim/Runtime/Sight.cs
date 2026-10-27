using System;

namespace FieldTable.Sim
{
    /// <summary>What a line of sight found.</summary>
    public readonly struct SightResult
    {
        public readonly bool Clear;

        /// <summary>How much the things in the way thin out the view, from 0 (nothing) to just under 1.</summary>
        public readonly float Obscured;

        /// <summary>True when smoke is in the way. Eyes and NODs can't see through it; a thermal can.</summary>
        public readonly bool Smoke;

        public SightResult(bool clear, float obscured, bool smoke = false)
        {
            Clear = clear;
            Obscured = obscured;
            Smoke = smoke;
        }

        /// <summary>True when this observer can see along the line: clear, and either no smoke or a thermal to look through it.</summary>
        public bool SeenBy(Unit observer) => Clear && (!Smoke || observer.Def.Thermal);

        public static readonly SightResult Blocked = new SightResult(false, 1f);
    }

    /// <summary>
    /// Line of sight over the map: hills block it, and buildings, trees, hedges, walls, and reeds
    /// block it below their height. Someone inside a building sees out its windows, and can be seen at them.
    /// </summary>
    public static class Sight
    {
        /// <summary>
        /// Traces a line from an eye at <paramref name="eyeHeight"/> above the ground at <paramref name="from"/>
        /// to a point <paramref name="targetHeight"/> above the ground at <paramref name="to"/>, checking every cell along the way.
        /// </summary>
        public static SightResult Trace(MapGrid map, Vec2 from, float eyeHeight, Vec2 to, float targetHeight)
        {
            if (!map.InBounds(from) || !map.InBounds(to)) return SightResult.Blocked;

            float z0 = map.GroundAt(from) + eyeHeight;
            float z1 = map.GroundAt(to) + targetHeight;
            float dist = Vec2.Distance(from, to);
            bool smoke = map.Smoke.Count > 0 && (map.InSmoke(from, z0) || map.InSmoke(to, z1));
            if (dist < map.CellSize) return new SightResult(true, 0f, smoke);

            int n = (int)Math.Ceiling(dist / map.CellSize);
            float step = dist / n;
            Vec2 delta = to - from;

            TerrainDef startTerrain = map.TerrainAt(from);
            TerrainDef endTerrain = map.TerrainAt(to);

            // Samples at the far end still inside the target's own building don't block: they're at a window.
            int trailStart = n;
            if (endTerrain.SeeOut)
            {
                while (trailStart - 1 >= 1 && ReferenceEquals(map.TerrainAt(from + delta * ((trailStart - 1) / (float)n)), endTerrain))
                    trailStart--;
            }

            bool leaving = startTerrain.SeeOut;
            float obscured = 0f;
            for (int i = 1; i < n; i++)
            {
                float t = i / (float)n;
                Vec2 p = from + delta * t;
                float z = z0 + (z1 - z0) * t;
                float g = map.GroundAt(p);
                if (g > z) return SightResult.Blocked;
                if (!smoke && map.Smoke.Count > 0 && map.InSmoke(p, z)) smoke = true;

                TerrainDef terrain = map.TerrainAt(p);
                if (leaving)
                {
                    if (ReferenceEquals(terrain, startTerrain)) continue;
                    leaving = false;
                }
                if (i >= trailStart) continue;

                if (terrain.Height > 0f && z < g + terrain.Height)
                {
                    // Solid things (walls, buildings) block outright, however thin the line clips them.
                    if (terrain.SightBlock >= 1f) return SightResult.Blocked;
                    obscured += terrain.SightBlock * step;
                    if (obscured >= 1f) return SightResult.Blocked;
                }
            }
            return new SightResult(true, obscured, smoke);
        }

        /// <summary>Line of sight between two units, eye to the top of the target, by their postures (or a drone's altitude).</summary>
        public static SightResult Between(MapGrid map, Unit observer, Unit target) =>
            Trace(map, observer.Position, observer.EyeHeight, target.Position, target.EyeHeight);
    }
}
