using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldTable.Sim
{
    /// <summary>
    /// The bridge seizure start state (Cinder Ford): a fresh map with water, a bridge, and a village; the squad comes in
    /// along the road from one side, the enemy holds the far side, and civilians are in the village.
    /// </summary>
    public static class BridgeSeizure
    {
        public static Mission Create(GameData data, NamePools names, ulong seed)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (names == null) throw new ArgumentNullException(nameof(names));
            var request = new MapRequest { Seed = seed, Name = "Bridge seizure " + seed };
            request.Require.Add(MapFeatures.Bridges);
            request.Require.Add(MapFeatures.Villages);
            var mapData = MapGenerator.Generate(request, names);
            var map = mapData.Build(data.Terrains);
            var rng = new Dice(seed, 11);

            var sim = new Simulation(map, seed);
            sim.AddWeapons(data.Weapons);
            var mission = new Mission { Sim = sim, MapData = mapData };
            mission.Marines = sim.AddForce(new Force { Id = "marines", Name = "Raider 2", Faction = "marines" });
            mission.Enemy = sim.AddForce(new Force { Id = "enemy", Name = "Enemy", Faction = "insurgents" });
            mission.Civilians = sim.AddForce(new Force { Id = "civilians", Name = "Civilians", Faction = "civilians", Civilian = true });
            sim.PlayerForce = mission.Marines;
            sim.RecordFrames(mission.Marines);

            var village = map.Landmarks.First(l => l.Kind == "area" && (l.Note == "village" || l.Note == "town"));
            Vec2 villageCenter = village.Center;
            var bridges = map.Landmarks.Where(l => l.Kind == "line" && l.Label.EndsWith(" bridge", StringComparison.Ordinal)).ToList();
            var bridge = bridges.OrderBy(b => Vec2.Distance(b.Center, villageCenter)).First();
            mission.Objective = bridge;
            Vec2 b0 = bridge.Center;

            // Which way the water runs at the bridge, and which bank the village is on.
            var water = map.Landmarks.Where(l => l.Kind == "line" && (l.Label.EndsWith(" River", StringComparison.Ordinal) || l.Label.EndsWith(" Creek", StringComparison.Ordinal)))
                .OrderBy(l => DistanceTo(b0, l.Points)).First();
            Vec2 flow = Direction(water.Points, b0);
            Vec2 across = new Vec2(-flow.Y, flow.X);
            float enemySide = Side(villageCenter, b0, across);
            if (Math.Abs(enemySide) < 0.5f) enemySide = 1f;
            enemySide = Math.Sign(enemySide);

            // The road over the bridge, and the end of it on the near bank, where the squad comes in.
            var road = map.Landmarks.Where(l => l.Kind == "line" && l.Label.StartsWith("Route ", StringComparison.Ordinal)).OrderBy(l => DistanceTo(b0, l.Points)).First();
            var pts = road.Points.ToList();
            if (Side(pts[0], b0, across) * enemySide > 0) pts.Reverse();
            // Close the column up if the near bank is short, so nobody starts on the bridge.
            float room = LengthTo(pts, b0) - 40f;
            float startAt = Math.Min(30f, Math.Max(0f, room / 4f));
            float spacing = Math.Max(2f, Math.Min(8f, (room - startAt) / 13f));
            var column = AlongFrom(pts, startAt, spacing, 14).Select(p => Inside(map, p)).ToList();

            AddSquad(sim, mission.Marines, data, names, rng, column);
            AddEnemy(sim, mission.Enemy, data, map, rng, b0, across, enemySide);
            AddCivilians(sim, mission.Civilians, data, map, rng, village);

            string title = "Operation " + Pick(names.Routes, rng) + " Ford";
            mission.Title = title;
            string bridgeGrid = map.SquareOf(Inside(map, b0)).ToString();
            string startGrid = map.SquareOf(column[0]).ToString();
            mission.Brief.Add("Task: seize and hold " + bridge.Label + " (" + bridgeGrid + ") so the platoon can cross.");
            mission.Brief.Add("Intent: the bridge intact and usable, and nobody in " + village.Label + " hurt.");
            mission.Brief.Add("Enemy: unknown strength. Fighters reported near " + village.Label + " on the far bank of " + water.Label + ".");
            mission.Brief.Add("Civilians: in " + village.Label + ". Standing ROE: positive ID before engaging.");
            mission.Brief.Add("You: 13 Marines in three fire teams plus Doc, at " + startGrid + " on " + road.Label + ".");
            return mission;
        }

        static void AddSquad(Simulation sim, Force force, GameData data, NamePools names, Dice rng, List<Vec2> column)
        {
            var people = Shuffle(names.People.Count > 0 ? names.People : new List<string> { "Marine" }, rng);
            int next = 0;
            string Name() => next < people.Count ? people[next++] : "Marine " + (next++ + 1);

            var slots = new List<(string id, string def, string team, string[] calls)>
            {
                ("lead", "sl", "", new[] { "Lead", "SL", "squad leader" }),
                ("doc", "doc", "", new[] { "Doc", "corpsman" }),
            };
            foreach (var team in new[] { "Alpha", "Bravo", "Charlie" })
            {
                slots.Add((team.ToLowerInvariant() + "-tl", "tl", team, new[] { team + " TL", team + " team leader", team + " lead" }));
                slots.Add((team.ToLowerInvariant() + "-ar", "ar", team, new[] { team + " AR", team + " automatic rifleman", team + " gunner" }));
                slots.Add((team.ToLowerInvariant() + "-aar", "aar", team, new[] { team + " AAR", team + " assistant gunner" }));
                bool drone = team == "Charlie";
                slots.Add((team.ToLowerInvariant() + "-rf", drone ? "dr" : "rf", team, drone ? new[] { team + " rifleman", "drone operator" } : new[] { team + " rifleman" }));
            }
            // Column order: Alpha leads, then the squad leader and Doc, then Bravo and Charlie.
            var order = new[] { 2, 3, 4, 5, 0, 1, 6, 7, 8, 9, 10, 11, 12, 13 };
            for (int i = 0; i < order.Length; i++)
            {
                var s = slots[order[i]];
                var u = new Unit(s.id, Name(), data.FindUnit(s.def), force, column[Math.Min(i, column.Count - 1)]) { Team = s.team };
                u.Callsigns.AddRange(s.calls);
                u.Facing = i + 1 < column.Count ? Bearing(column[i + 1], column[i]) : 0f;
                sim.AddUnit(u);
            }
        }

        static void AddEnemy(Simulation sim, Force force, GameData data, MapGrid map, Dice rng, Vec2 bridge, Vec2 across, float side)
        {
            var taken = new List<Vec2>();
            void Place(string id, string name, string def, float near, float far, Posture posture)
            {
                Vec2 best = default;
                float bestScore = float.MinValue;
                for (int k = 0; k < 300; k++)
                {
                    float r = near + (far - near) * Next(rng);
                    double a = Next(rng) * Math.PI * 2;
                    var p = new Vec2(bridge.X + (float)Math.Cos(a) * r, bridge.Y + (float)Math.Sin(a) * r);
                    if (!map.InBounds(p) || Side(p, bridge, across) * side < 5f) continue;
                    var t = map.TerrainAt(p);
                    if (t.Deep || !t.Passable) continue;
                    if (taken.Any(q => Vec2.Distance(q, p) < 15f)) continue;
                    float score = t.Cover + (t.SeeOut ? 0.3f : 0f) + t.Concealment * 0.3f + Next(rng) * 0.2f;
                    if (Sight.Trace(map, p, Postures.Height(posture), bridge, 1f).Clear) score += 1f;
                    if (score > bestScore) { bestScore = score; best = p; }
                }
                if (bestScore == float.MinValue) best = Inside(map, bridge + across * (side * near));
                taken.Add(best);
                var u = new Unit(id, name, data.FindUnit(def), force, best) { Posture = posture };
                u.Facing = Bearing(best, bridge);
                sim.AddUnit(u);
            }
            Place("pkm", "PKM gunner", "en-pkm", 80f, 220f, Posture.Prone);
            Place("enemy-1", "Rifleman 1", "en-rf", 25f, 90f, Posture.Kneeling);
            Place("enemy-2", "Rifleman 2", "en-rf", 25f, 90f, Posture.Kneeling);
            Place("rpg", "RPG gunner", "en-rpg", 40f, 130f, Posture.Kneeling);
        }

        static void AddCivilians(Simulation sim, Force force, GameData data, MapGrid map, Dice rng, Landmark village)
        {
            var homes = map.Landmarks.Where(l => l.Kind == "building" && village.Contains(l.Center)).ToList();
            int count = Math.Min(homes.Count, 3 + (int)(Next(rng) * 3));
            homes = Shuffle(homes, rng);
            for (int i = 0; i < count; i++)
            {
                var u = new Unit("civilian-" + (i + 1), "Civilian " + (i + 1), data.FindUnit("civ"), force, homes[i].Center);
                sim.AddUnit(u);
            }
        }

        // ---------- helpers ----------

        static float Next(Dice d) => (d.NextUInt() >> 8) * (1f / 16777216f);

        static List<T> Shuffle<T>(IList<T> list, Dice rng)
        {
            var copy = list.ToList();
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = (int)(Next(rng) * (i + 1));
                var t = copy[i]; copy[i] = copy[j]; copy[j] = t;
            }
            return copy;
        }

        static string Pick(List<string> pool, Dice rng) => pool.Count == 0 ? "Cinder" : pool[(int)(Next(rng) * pool.Count) % pool.Count];

        static float Side(Vec2 p, Vec2 origin, Vec2 across) => (p.X - origin.X) * across.X + (p.Y - origin.Y) * across.Y;

        static Vec2 Inside(MapGrid map, Vec2 p) =>
            new Vec2(Math.Max(3f, Math.Min(map.WidthMeters - 3f, p.X)), Math.Max(3f, Math.Min(map.HeightMeters - 3f, p.Y)));

        static float Bearing(Vec2 a, Vec2 b) => (float)(Math.Atan2(b.X - a.X, -(b.Y - a.Y)) * 180.0 / Math.PI + 360.0) % 360f;

        static float DistanceTo(Vec2 p, IReadOnlyList<Vec2> line)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++) best = Math.Min(best, MapGrid.DistanceToSegment(p, line[i], line[i + 1]));
            return best;
        }

        /// <summary>The direction of the line's segment nearest a point.</summary>
        static Vec2 Direction(IReadOnlyList<Vec2> line, Vec2 p)
        {
            int best = 0;
            float bestD = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float d = MapGrid.DistanceToSegment(p, line[i], line[i + 1]);
                if (d < bestD) { bestD = d; best = i; }
            }
            Vec2 v = line[best + 1] - line[best];
            float len = v.Length;
            return len > 0f ? v * (1f / len) : new Vec2(0, 1);
        }

        /// <summary>How far along a line, from its first end, to the point on it nearest p.</summary>
        static float LengthTo(List<Vec2> pts, Vec2 p)
        {
            float walked = 0f, bestD = float.MaxValue, bestAt = 0f;
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                Vec2 a = pts[i], ab = pts[i + 1] - pts[i];
                float seg = ab.Length;
                float t = seg > 0f ? Math.Max(0f, Math.Min(1f, ((p.X - a.X) * ab.X + (p.Y - a.Y) * ab.Y) / (seg * seg))) : 0f;
                float d = Vec2.Distance(p, a + ab * t);
                if (d < bestD) { bestD = d; bestAt = walked + seg * t; }
                walked += seg;
            }
            return bestAt;
        }

        /// <summary>Points along a line, starting a little way in from its first end, spaced evenly.</summary>
        static List<Vec2> AlongFrom(List<Vec2> pts, float startAt, float spacing, int count)
        {
            var result = new List<Vec2>();
            float want = startAt;
            float walked = 0f;
            for (int i = 0; i + 1 < pts.Count && result.Count < count; i++)
            {
                float seg = Vec2.Distance(pts[i], pts[i + 1]);
                while (want <= walked + seg && result.Count < count)
                {
                    float t = seg > 0f ? (want - walked) / seg : 0f;
                    result.Add(pts[i] + (pts[i + 1] - pts[i]) * t);
                    want += spacing;
                }
                walked += seg;
            }
            while (result.Count < count) result.Add(result.Count > 0 ? result[result.Count - 1] : pts[0]);
            return result;
        }
    }
}
