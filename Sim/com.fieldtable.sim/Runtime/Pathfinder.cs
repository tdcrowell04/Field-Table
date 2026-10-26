using System;
using System.Collections.Generic;

namespace FieldTable.Sim
{
    /// <summary>
    /// Finds walking routes over the map on a 2-meter grid. Slow ground costs more, climbing costs more,
    /// and deep water is off limits unless the order says to swim.
    /// </summary>
    public sealed class Pathfinder
    {
        public const float NodeSize = 2f;

        /// <summary>Each meter climbed costs as much as this many meters on the flat.</summary>
        const float ClimbCost = 4f;

        readonly MapGrid map;
        readonly int w, h;
        readonly float[] speed;
        readonly float[] ground;
        readonly bool[] deep;

        // Search buffers, reused between searches.
        readonly float[] g;
        readonly int[] from;
        readonly int[] seen;
        int generation;

        public Pathfinder(MapGrid map)
        {
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            w = Math.Max(1, (int)(map.WidthMeters / NodeSize));
            h = Math.Max(1, (int)(map.HeightMeters / NodeSize));
            int n = w * h;
            speed = new float[n];
            ground = new float[n];
            deep = new bool[n];
            g = new float[n];
            from = new int[n];
            seen = new int[n];
            // A node is only as fast as its slowest meter, and wet if any meter of it is deep water,
            // so walking anywhere inside a dry node stays dry.
            int per = Math.Max(1, (int)Math.Round(NodeSize / map.CellSize));
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    float sp = float.MaxValue;
                    bool wet = false;
                    for (int cy = 0; cy < per; cy++)
                        for (int cx = 0; cx < per; cx++)
                        {
                            int mx = Math.Min(map.Width - 1, x * per + cx), my = Math.Min(map.Height - 1, y * per + cy);
                            TerrainDef t = map.TerrainAt(mx, my);
                            sp = Math.Min(sp, t.MoveFactor);
                            wet |= t.Deep;
                        }
                    speed[i] = sp;
                    deep[i] = wet;
                    ground[i] = map.GroundAt(Center(x, y));
                }
        }

        Vec2 Center(int x, int y) => new Vec2((x + 0.5f) * NodeSize, (y + 0.5f) * NodeSize);

        /// <summary>
        /// A route from one point to another as a list of waypoints, ending exactly at the destination.
        /// Null when there's no way there.
        /// </summary>
        public List<Vec2> Find(Vec2 start, Vec2 goal, bool allowDeep)
        {
            if (!map.InBounds(start) || !map.InBounds(goal)) return null;
            int s = Nearest(start, allowDeep), e = Nearest(goal, allowDeep);
            if (s < 0 || e < 0) return null;

            generation++;
            var open = new MinHeap();
            g[s] = 0f;
            from[s] = -1;
            seen[s] = generation;
            open.Push(s, Heuristic(s, e));
            int ex = e % w, ey = e / w;
            bool found = false;
            while (open.Count > 0)
            {
                int cur = open.Pop(out float f);
                if (cur == e) { found = true; break; }
                if (f - Heuristic(cur, e) > g[cur] + 0.001f) continue; // stale entry
                int cx = cur % w, cy = cur / w;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx, ny = cy + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        int ni = ny * w + nx;
                        // No cutting a corner past water or a wall.
                        if (dx != 0 && dy != 0 && (!Open(cy * w + nx, allowDeep) || !Open(ny * w + cx, allowDeep))) continue;
                        float step = Cost(cur, ni, allowDeep, dx != 0 && dy != 0);
                        if (step <= 0f) continue;
                        float ng = g[cur] + step;
                        if (seen[ni] == generation && ng >= g[ni]) continue;
                        seen[ni] = generation;
                        g[ni] = ng;
                        from[ni] = cur;
                        open.Push(ni, ng + Heuristic(ni, e));
                    }
            }
            if (!found) return null;

            var nodes = new List<int>();
            for (int at = e; at != -1; at = from[at]) nodes.Add(at);
            nodes.Reverse();

            // Straighten the route: from each point, walk straight to the farthest later point
            // the unit can reach without crossing anything slower or wetter than the route itself.
            var path = new List<Vec2>();
            int anchor = 0;
            Vec2 from0 = start;
            while (anchor < nodes.Count - 1)
            {
                int best = anchor + 1;
                float slowest = speed[nodes[anchor]];
                int misses = 0;
                for (int j = anchor + 1; j < nodes.Count && misses < 24; j++)
                {
                    slowest = Math.Min(slowest, speed[nodes[j]]);
                    if (StraightWalk(from0, Center(nodes[j] % w, nodes[j] / w), slowest, allowDeep)) { best = j; misses = 0; }
                    else misses++;
                }
                Vec2 p = Center(nodes[best] % w, nodes[best] / w);
                if (best < nodes.Count - 1) path.Add(p);
                from0 = p;
                anchor = best;
            }
            path.Add(goal);
            return path;
        }

        bool Open(int i, bool allowDeep) => speed[i] > 0f && (allowDeep || !deep[i]);

        /// <summary>True when every point on a straight line, and a little to either side, is as quick as the route and dry enough.</summary>
        bool StraightWalk(Vec2 a, Vec2 b, float slowest, bool allowDeep)
        {
            Vec2 d = b - a;
            float len = d.Length;
            if (len < 0.01f) return true;
            Vec2 side = new Vec2(-d.Y / len, d.X / len) * 0.6f;
            int n = (int)Math.Ceiling(len / 0.5f);
            for (int k = 0; k <= n; k++)
            {
                Vec2 p = a + d * (k / (float)n);
                foreach (var q in new[] { p, p + side, p - side })
                {
                    if (!map.InBounds(q)) return false;
                    var t = map.TerrainAt(q);
                    if (t.MoveFactor < slowest * 0.99f || t.MoveFactor <= 0f) return false;
                    if (t.Deep && !allowDeep) return false;
                }
            }
            return true;
        }

        /// <summary>Total length of a route in meters, counting from a start point.</summary>
        public static float Length(Vec2 start, IList<Vec2> path)
        {
            float total = 0f;
            Vec2 at = start;
            foreach (var p in path) { total += Vec2.Distance(at, p); at = p; }
            return total;
        }

        float Cost(int a, int b, bool allowDeep, bool diagonal)
        {
            float sp = speed[b];
            if (sp <= 0f) return -1f;
            if (deep[b] && !allowDeep) return -1f;
            float dist = diagonal ? NodeSize * 1.41421356f : NodeSize;
            float climb = Math.Max(0f, ground[b] - ground[a]);
            return dist / sp + climb * ClimbCost;
        }

        float Heuristic(int a, int b)
        {
            int dx = Math.Abs(a % w - b % w), dy = Math.Abs(a / w - b / w);
            return NodeSize * (Math.Max(dx, dy) + 0.41421356f * Math.Min(dx, dy));
        }

        /// <summary>The node at a point, or the closest usable one within 20 meters.</summary>
        int Nearest(Vec2 p, bool allowDeep)
        {
            int px = Math.Min(w - 1, Math.Max(0, (int)(p.X / NodeSize)));
            int py = Math.Min(h - 1, Math.Max(0, (int)(p.Y / NodeSize)));
            for (int r = 0; r <= 10; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                        int x = px + dx, y = py + dy;
                        if (x < 0 || y < 0 || x >= w || y >= h) continue;
                        int i = y * w + x;
                        if (speed[i] > 0f && (allowDeep || !deep[i])) return i;
                    }
            return -1;
        }

        /// <summary>A small binary heap of node indexes by priority, since .NET Standard 2.1 has none.</summary>
        sealed class MinHeap
        {
            readonly List<int> items = new List<int>();
            readonly List<float> keys = new List<float>();
            public int Count => items.Count;

            public void Push(int item, float key)
            {
                items.Add(item);
                keys.Add(key);
                int i = items.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (keys[parent] <= keys[i]) break;
                    Swap(i, parent);
                    i = parent;
                }
            }

            public int Pop(out float key)
            {
                int top = items[0];
                key = keys[0];
                int last = items.Count - 1;
                items[0] = items[last];
                keys[0] = keys[last];
                items.RemoveAt(last);
                keys.RemoveAt(last);
                int i = 0;
                while (true)
                {
                    int l = 2 * i + 1, r = l + 1, m = i;
                    if (l < items.Count && keys[l] < keys[m]) m = l;
                    if (r < items.Count && keys[r] < keys[m]) m = r;
                    if (m == i) break;
                    Swap(i, m);
                    i = m;
                }
                return top;
            }

            void Swap(int a, int b)
            {
                int t = items[a]; items[a] = items[b]; items[b] = t;
                float k = keys[a]; keys[a] = keys[b]; keys[b] = k;
            }
        }
    }
}
