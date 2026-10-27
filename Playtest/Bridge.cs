using System.Buffers;
using System.Text;
using System.Text.Json;
using FieldTable.Sim;
using Microsoft.JSInterop;

namespace FieldTable.Playtest
{
    /// <summary>
    /// What the playtest page calls: start a mission, check and give orders, run a turn.
    /// Everything comes back as JSON, and only what the Marines know: an enemy they can't see shows
    /// where it was last seen, or not at all.
    /// </summary>
    public static class Bridge
    {
        static GameData data;
        static Mission mission;

        /// <summary>Secondary objectives the Marines have seen broken. Until the mission ends, the page only shows what they know.</summary>
        static readonly HashSet<string> seenBroken = new HashSet<string>();

        static Mission Current => mission ?? throw new InvalidOperationException("Start a mission first.");
        static readonly Dictionary<string, int> terrainIndex = new Dictionary<string, int>();

        /// <summary>Builds a fresh bridge seizure from the game's data files and a seed. Returns the map and the start.</summary>
        [JSInvokable]
        public static string NewMission(string terrainJson, string unitsJson, string namesJson, int seed)
        {
            data = GameData.Combine(GameData.FromJson(terrainJson), GameData.FromJson(unitsJson));
            terrainIndex.Clear();
            seenBroken.Clear();
            for (int i = 0; i < data.Terrains.Count; i++) terrainIndex[data.Terrains[i].Id] = i;
            mission = BridgeSeizure.Create(data, NamePools.FromJson(namesJson), (ulong)(uint)seed);
            mission.Sim.Light = LightLevel.Day;

            return Json(w =>
            {
                w.WriteString("title", mission.Title);
                w.WriteNumber("seed", seed);
                w.WriteStartArray("brief");
                foreach (var line in mission.Brief) w.WriteStringValue(line);
                w.WriteEndArray();
                var b = mission.Objective;
                w.WriteStartObject("objective");
                w.WriteString("label", b.Label);
                w.WriteNumber("x", R(b.Center.X));
                w.WriteNumber("y", R(b.Center.Y));
                w.WriteString("grid", mission.Sim.Map.SquareOf(b.Center).ToString());
                w.WriteEndObject();
                WriteMap(w);
                WriteRoster(w);
                WriteState(w);
            });
        }

        /// <summary>Where things stand between turns, as the Marines know it.</summary>
        [JSInvokable]
        public static string Snapshot() => Json(WriteState);

        /// <summary>
        /// Checks a list of orders without giving them: [{who, verb, where, target, speed, posture, swim, kind}].
        /// Each comes back with who it reaches, whether they can do it, why not, warnings, and the route.
        /// </summary>
        [JSInvokable]
        public static string CheckOrders(string ordersJson) => Json(w => WriteChecks(w, ordersJson, false));

        /// <summary>Gives a list of orders, the same shape as CheckOrders. Teams spread out.</summary>
        [JSInvokable]
        public static string GiveOrders(string ordersJson) => Json(w => WriteChecks(w, ordersJson, true));

        /// <summary>Plays one minute and returns it for playback: frames, shots, blasts, radio, rolls, events, and the outcome.</summary>
        [JSInvokable]
        public static string RunTurn() => Json(w =>
        {
            var sim = Current.Sim;
            if (mission.Outcome != null) throw new InvalidOperationException("The mission is over: " + mission.OutcomeText);
            int turn = sim.Clock.Turn;
            int from = sim.Log.Events.Count;
            int startStep = sim.Clock.Step;
            sim.RunTurn();
            mission.CheckOutcome();
            var events = sim.Log.Events.Skip(from).ToList();
            var frames = sim.Frames;
            var marines = mission.Marines;
            var names = new HashSet<string>(sim.Units.Where(u => ReferenceEquals(u.Force, marines)).Select(u => u.Name));

            // Whether the Marines could see a unit at a given step, from the nearest frame.
            bool SeenAt(Unit u, int step)
            {
                if (ReferenceEquals(u.Force, marines)) return true;
                if (frames.Count == 0) return false;
                int i = frames.Count - 1;
                for (int k = 0; k < frames.Count; k++) if (frames[k].Step >= step) { i = k; break; }
                int idx = IndexOf(u);
                return idx >= 0 && frames[i].Units[idx].Seen;
            }

            {
                w.WriteNumber("turn", turn);
                w.WriteNumber("startStep", startStep);
                w.WriteNumber("stepsPerSecond", SimClock.StepsPerSecond);

                // Frames: per unit [x*10, y*10, facing, flags] or null when the Marines can't see it.
                // flags: posture (0 stand, 1 kneel, 2 prone) + 4 * condition (0 ok, 1 wounded, 2 down, 3 dead)
                // + 16 moving + 32 firing + 64 detained. A fifth entry is an emote, like "controller" for flying the drone.
                w.WriteStartArray("frames");
                foreach (var f in frames)
                {
                    w.WriteStartObject();
                    w.WriteNumber("s", f.Step);
                    w.WriteStartArray("u");
                    foreach (var uf in f.Units)
                    {
                        if (!uf.Seen) { w.WriteNullValue(); continue; }
                        w.WriteStartArray();
                        w.WriteNumberValue((int)Math.Round(uf.X * 10));
                        w.WriteNumberValue((int)Math.Round(uf.Y * 10));
                        w.WriteNumberValue((int)Math.Round(uf.Facing));
                        w.WriteNumberValue((int)uf.Posture + 4 * (int)uf.Condition + (uf.Moving ? 16 : 0) + (uf.Firing ? 32 : 0) + (uf.Detained ? 64 : 0));
                        if (uf.Emote != null) w.WriteStringValue(uf.Emote);
                        w.WriteEndArray();
                    }
                    w.WriteEndArray();
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("shots");
                foreach (var e in events.OfType<ShotFired>())
                {
                    if (!SeenAt(e.Shooter, e.Step) && !ReferenceEquals(e.Shooter.Force, marines)) continue;
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteNumber("u", IndexOf(e.Shooter));
                    w.WriteNumber("x0", R(e.From.X)); w.WriteNumber("y0", R(e.From.Y));
                    w.WriteNumber("x1", R(e.To.X)); w.WriteNumber("y1", R(e.To.Y));
                    w.WriteString("w", e.Weapon);
                    w.WriteBoolean("hit", e.Hit);
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("blasts");
                foreach (var e in events.OfType<BlastAt>())
                {
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteNumber("x", R(e.At.X)); w.WriteNumber("y", R(e.At.Y));
                    w.WriteNumber("r", e.Radius);
                    w.WriteString("w", e.Weapon.Id + (e.Ap ? "-ap" : ""));
                    w.WriteString("name", e.Weapon.Name);
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("smoke");
                foreach (var e in events.OfType<SmokeAt>()) WriteCloud(w, e.Cloud, e.Step);
                w.WriteEndArray();

                // A blown bridge: the ground that changed, as x, y, terrain, x, y, terrain...
                w.WriteStartArray("blown");
                foreach (var e in events.OfType<BridgeBlown>())
                {
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    WriteCells(w, e.Cells);
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("breaches");
                foreach (var e in events.OfType<BreachMade>())
                {
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteNumber("x", R(e.At.X)); w.WriteNumber("y", R(e.At.Y));
                    w.WriteNumber("r", e.Radius);
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("casevac");
                foreach (var e in events.OfType<CasevacMoved>())
                {
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteBoolean("arrived", e.Arrived);
                    w.WriteBoolean("helicopter", e.Helicopter);
                    w.WriteNumber("x", R(e.Lz.X)); w.WriteNumber("y", R(e.Lz.Y));
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("radio");
                foreach (var e in events.OfType<RadioCall>())
                {
                    if (e.From != null && !ReferenceEquals(e.From.Force, marines)) continue;
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteNumber("u", e.From == null ? -1 : IndexOf(e.From));
                    w.WriteString("text", e.Text);
                    // Whose words they really are, for the AI to give them their own voice.
                    if (e.Speaker != null) w.WriteString("p", e.Speaker);
                    // A call about a place gets a marker on the map: where, and what kind of news.
                    if (e.At is Vec2 at)
                    {
                        w.WriteNumber("x", R(at.X)); w.WriteNumber("y", R(at.Y));
                        w.WriteString("k", e.Kind ?? "info");
                    }
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                // Rolls that touch a Marine: their own, and the ones made against them.
                w.WriteStartArray("rolls");
                foreach (var e in events.OfType<DiceRolled>())
                {
                    bool ours = names.Contains(e.Actor) || names.Any(n => e.Reason.Contains(" " + n + " ") || e.Reason.EndsWith(" " + n, StringComparison.Ordinal));
                    if (!ours) continue;
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteString("who", e.Actor);
                    w.WriteString("why", e.Reason);
                    w.WriteNumber("d", e.Sides);
                    w.WriteNumber("r", e.Result);
                    w.WriteBoolean("big", e.BigMoment);
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("events");
                foreach (var e in events)
                {
                    Unit about = null;
                    string kind = null;
                    switch (e)
                    {
                        case UnitWounded x: about = x.Unit; kind = "wounded"; break;
                        case UnitDown x: about = x.Unit; kind = "down"; break;
                        case UnitDied x: about = x.Unit; kind = "died"; break;
                        case UnitTreated x: about = x.Patient; kind = "treated"; break;
                        case UnitInTrouble x: about = x.Unit; kind = "trouble"; break;
                        case ContactSpotted x when ReferenceEquals(x.Force, marines): about = x.Target; kind = "spotted"; break;
                        case ContactLost x when ReferenceEquals(x.Force, marines): about = x.Target; kind = "lost"; break;
                        case UnitDetained x: about = x.Unit; kind = "detained"; break;
                        case UnitSurrendered x: about = x.Unit; kind = "surrendered"; break;
                        case UnitEvacuated x: about = x.Unit; kind = "evacuated"; break;
                    }
                    if (kind == null) continue;
                    if (kind != "spotted" && kind != "lost" && !SeenAt(about, e.Step)) continue;
                    if ((kind == "wounded" || kind == "down" || kind == "died") && ReferenceEquals(about.Force, mission.Civilians)) seenBroken.Add("civilians");
                    w.WriteStartObject();
                    w.WriteNumber("s", e.Step);
                    w.WriteString("kind", kind);
                    w.WriteNumber("u", IndexOf(about));
                    w.WriteString("text", e.Describe());
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                WriteState(w);
            }
        });

        // ---------- the map ----------

        /// <summary>
        /// The ground at 1-meter cells (terrain index runs, base64), heights every 2 meters (decimeters, base64 int16),
        /// the terrain table, line features for drawing roads and water crisply, buildings, and named places.
        /// </summary>
        static void WriteMap(Utf8JsonWriter w)
        {
            var map = mission.Sim.Map;
            w.WriteStartObject("map");
            w.WriteString("name", map.Name);
            w.WriteNumber("w", map.Width);
            w.WriteNumber("h", map.Height);
            w.WriteNumber("cell", map.CellSize);
            w.WriteNumber("square", map.SquareSize);
            w.WriteNumber("cols", map.Columns);
            w.WriteNumber("rows", map.Rows);

            w.WriteStartArray("terrains");
            foreach (var t in data.Terrains)
            {
                w.WriteStartObject();
                w.WriteString("id", t.Id);
                w.WriteString("name", t.Name);
                w.WriteNumber("height", t.Height);
                w.WriteBoolean("deep", t.Deep);
                w.WriteEndObject();
            }
            w.WriteEndArray();

            // Terrain per cell, row by row, as runs: [terrain index, how many] pairs, up to 255 per run.
            var runs = new List<byte>();
            int run = 0, last = -1;
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    int t = terrainIndex[map.TerrainAt(x, y).Id];
                    if (t == last && run < 255) { run++; continue; }
                    if (run > 0) { runs.Add((byte)last); runs.Add((byte)run); }
                    last = t;
                    run = 1;
                }
            runs.Add((byte)last); runs.Add((byte)run);
            w.WriteString("cells", Convert.ToBase64String(runs.ToArray()));

            const int step = 2;
            int hw = map.Width / step, hh = map.Height / step;
            var heights = new byte[hw * hh * 2];
            for (int y = 0; y < hh; y++)
                for (int x = 0; x < hw; x++)
                {
                    short d = (short)Math.Round(map.GroundAt(x * step + step / 2, y * step + step / 2) * 10f);
                    int i = (y * hw + x) * 2;
                    heights[i] = (byte)(d & 0xff);
                    heights[i + 1] = (byte)((d >> 8) & 0xff);
                }
            w.WriteNumber("hstep", step);
            w.WriteNumber("hw", hw);
            w.WriteNumber("hh", hh);
            w.WriteString("heights", Convert.ToBase64String(heights));

            // Lines in the order they were painted, in meters, so roads, water, walls, and hedges draw crisply.
            float sq = mission.MapData.SquareSize;
            w.WriteStartArray("lines");
            foreach (var f in mission.MapData.Features)
            {
                if (f.Kind != "line" || f.Points == null) continue;
                w.WriteStartObject();
                w.WriteString("t", f.Terrain);
                w.WriteNumber("w", f.Width);
                w.WriteStartArray("p");
                foreach (var p in f.Points) { w.WriteNumberValue(R(p[0] * sq)); w.WriteNumberValue(R(p[1] * sq)); }
                w.WriteEndArray();
                w.WriteEndObject();
            }
            w.WriteEndArray();

            float buildingHeight = data.Terrains.FirstOrDefault(t => t.Id == "building")?.Height ?? 6f;
            w.WriteStartArray("buildings");
            foreach (var l in map.Landmarks.Where(l => l.Kind == "building"))
            {
                w.WriteStartObject();
                w.WriteString("label", l.Label);
                w.WriteString("n", l.Label.StartsWith("building ", StringComparison.OrdinalIgnoreCase) ? l.Label.Substring(9) : l.Label);
                w.WriteNumber("x", R(l.Min.X)); w.WriteNumber("y", R(l.Min.Y));
                w.WriteNumber("w", R(l.Max.X - l.Min.X)); w.WriteNumber("h", R(l.Max.Y - l.Min.Y));
                w.WriteNumber("z", buildingHeight);
                w.WriteEndObject();
            }
            w.WriteEndArray();

            w.WriteStartArray("places");
            foreach (var l in map.Landmarks.Where(l => l.Kind != "building"))
            {
                w.WriteStartObject();
                w.WriteString("label", l.Label);
                w.WriteString("kind", l.Kind);
                if (l.Note != null) w.WriteString("note", l.Note);
                w.WriteNumber("x", R(l.Center.X)); w.WriteNumber("y", R(l.Center.Y));
                w.WriteString("grid", map.SquareOf(Inside(map, l.Center)).ToString());
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }

        // ---------- units ----------

        /// <summary>Every unit's fixed facts, in the order frames list them. Enemies show only what a Marine could tell by looking.</summary>
        static void WriteRoster(Utf8JsonWriter w)
        {
            w.WriteStartArray("roster");
            foreach (var u in mission.Sim.Units)
            {
                bool ours = ReferenceEquals(u.Force, mission.Marines);
                w.WriteStartObject();
                w.WriteString("id", u.Id);
                w.WriteString("name", u.Name);
                w.WriteString("side", ours ? "us" : u.Force.Civilian ? "civ" : "op");
                w.WriteString("figure", u.Def.Figure ?? "rf");
                w.WriteString("kind", u.Def.Name);
                if (ours)
                {
                    w.WriteString("team", u.Team);
                    w.WriteStartArray("calls");
                    foreach (var c in u.Callsigns) w.WriteStringValue(c);
                    w.WriteEndArray();
                }
                w.WriteEndObject();
            }
            w.WriteEndArray();
        }

        /// <summary>The clock, the outcome, every Marine in full, and the contacts they know about.</summary>
        static void WriteState(Utf8JsonWriter w)
        {
            var sim = Current.Sim;
            var knowledge = sim.Fog.For(mission.Marines);
            w.WriteStartObject("state");
            w.WriteNumber("turn", sim.Clock.Turn);
            w.WriteNumber("step", sim.Clock.Step);
            w.WriteNumber("turnLimit", Mission.TurnLimit);
            WriteObjectives(w);
            w.WriteNumber("friendlyFire", mission.FriendlyFireHits);
            if (mission.Outcome != null)
            {
                w.WriteString("outcome", mission.Outcome);
                w.WriteString("outcomeText", mission.OutcomeText);
                w.WriteString("rating", mission.Rating);
                // At the end, what the enemy was really up to.
                if (mission.Enemy.Plan != null) w.WriteString("enemyGoal", mission.Enemy.Plan.Goal);
            }
            w.WriteStartArray("units");
            foreach (var u in sim.Units)
            {
                bool ours = ReferenceEquals(u.Force, mission.Marines);
                Contact c = ours ? null : knowledge.About(u.Id);
                if (!ours && c == null) { w.WriteNullValue(); continue; }
                if (ours && u.Gone)
                {
                    // Off the table: evacuated, or the drone still in its case.
                    w.WriteStartObject();
                    w.WriteBoolean("gone", true);
                    if (u.Evacuated) w.WriteBoolean("evacuated", true);
                    w.WriteString("condition", u.Condition.ToString().ToLowerInvariant());
                    w.WriteEndObject();
                    continue;
                }
                bool visible = ours || c.Visible;
                Vec2 at = visible ? u.Position : c.LastKnownPosition;
                w.WriteStartObject();
                w.WriteNumber("x", R(at.X));
                w.WriteNumber("y", R(at.Y));
                w.WriteString("grid", sim.Map.SquareOf(Inside(sim.Map, at)).ToString());
                w.WriteBoolean("seen", visible);
                if (!visible)
                {
                    w.WriteNumber("lastSeen", c.LastSeenStep);
                }
                else
                {
                    w.WriteNumber("facing", (int)Math.Round(u.Facing));
                    w.WriteString("posture", u.Posture.ToString().ToLowerInvariant());
                    w.WriteString("condition", u.Condition.ToString().ToLowerInvariant());
                    if (u.Detained) w.WriteBoolean("detained", true);
                    if (u.Emote != null) w.WriteString("emote", u.Emote);
                }
                if (ours)
                {
                    w.WriteNumber("ammo", u.Ammo);
                    w.WriteNumber("launcher", u.LauncherAmmo);
                    w.WriteNumber("launcherAp", u.LauncherAp);
                    w.WriteNumber("grenades", u.Grenades);
                    w.WriteNumber("smokes", u.Smokes);
                    if (u.ArmHit) w.WriteBoolean("armHit", true);
                    if (u.LegHit) w.WriteBoolean("legHit", true);
                    if (u.CarriedBy != null) w.WriteNumber("carriedBy", IndexOf(u.CarriedBy));
                    w.WriteNumber("load", u.Load);
                    w.WriteNumber("suppression", R(u.Suppression));
                    if (u.Stabilized) w.WriteBoolean("stable", true);
                    if (u.Order != null)
                    {
                        w.WriteStartObject("order");
                        w.WriteString("verb", u.Order.Verb);
                        if (u.Order.Where != null) w.WriteString("where", u.Order.Where);
                        if (u.Order.Target != null) w.WriteString("target", u.Order.Target);
                        w.WriteEndObject();
                    }
                }
                w.WriteEndObject();
            }
            w.WriteEndArray();

            // Ground a blown bridge changed, smoke hanging now, every breach so far (the ground the page drew has changed
            // there), and CASEVAC.
            w.WriteStartObject("blown");
            WriteCells(w, sim.Log.Events.OfType<BridgeBlown>().SelectMany(e => e.Cells).ToList());
            w.WriteEndObject();
            w.WriteStartArray("smoke");
            foreach (var cloud in sim.Map.Smoke) WriteCloud(w, cloud, -1);
            w.WriteEndArray();
            w.WriteStartArray("breaches");
            foreach (var e in sim.Log.Events.OfType<BreachMade>())
            {
                w.WriteStartObject();
                w.WriteNumber("x", R(e.At.X)); w.WriteNumber("y", R(e.At.Y));
                w.WriteNumber("r", e.Radius);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            if (sim.Evac != null)
            {
                w.WriteStartObject("casevac");
                w.WriteNumber("x", R(sim.Evac.Lz.X)); w.WriteNumber("y", R(sim.Evac.Lz.Y));
                w.WriteString("grid", sim.Map.SquareOf(sim.Evac.Lz).ToString());
                w.WriteBoolean("helicopter", sim.Evac.Helicopter);
                w.WriteBoolean("landed", sim.Evac.Landed);
                w.WriteEndObject();
            }
            w.WriteEndObject();
        }

        static void WriteCloud(Utf8JsonWriter w, SmokeCloud c, int step)
        {
            w.WriteStartObject();
            if (step >= 0) w.WriteNumber("s", step);
            w.WriteNumber("x", R(c.Center.X)); w.WriteNumber("y", R(c.Center.Y));
            w.WriteNumber("r", c.Radius);
            w.WriteNumber("until", c.UntilStep);
            w.WriteEndObject();
        }

        /// <summary>
        /// The main objective and the secondary ones, each with where it stands. While the mission runs, a secondary
        /// objective shows as broken only once the Marines know it (they saw the civilian get hurt); at the end, the truth.
        /// </summary>
        static void WriteObjectives(Utf8JsonWriter w)
        {
            bool over = mission.Outcome != null;
            w.WriteStartObject("objectives");
            w.WriteString("main", mission.MainObjective);
            w.WriteString("mainStatus", over ? (mission.Outcome == "success" ? "seized" : "failed") : mission.MarineOnObjective ? "onit" : "notyet");
            w.WriteString("roe", mission.Roe);
            w.WriteNumber("onObjective", Mission.OnObjective);
            w.WriteNumber("holdRadius", Mission.HoldRadius);
            w.WriteNumber("combatIneffective", Mission.CombatIneffective);
            w.WriteStartArray("secondary");
            foreach (var o in mission.Secondary)
            {
                bool broken = o.IsBroken(mission);
                bool known = over || o.Id != "civilians" || seenBroken.Contains(o.Id);
                w.WriteStartObject();
                w.WriteString("id", o.Id);
                w.WriteString("text", o.Text);
                w.WriteString("status", broken && known ? "failed" : over ? "met" : "ok");
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }

        // ---------- orders ----------

        static void WriteChecks(Utf8JsonWriter w, string ordersJson, bool give)
        {
            var sim = Current.Sim;
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(ordersJson) ? "[]" : ordersJson);
            w.WriteStartArray("checks");
            int n = 0;
            foreach (var o in doc.RootElement.EnumerateArray())
            {
                string who = Str(o, "who");
                var order = new Order
                {
                    Verb = Str(o, "verb"),
                    Where = Str(o, "where"),
                    Target = Str(o, "target"),
                    Speed = Str(o, "speed"),
                    Posture = Str(o, "posture"),
                    Kind = Str(o, "kind"),
                    Swim = o.TryGetProperty("swim", out var s) && s.ValueKind == JsonValueKind.True,
                };
                List<OrderCheck> checks;
                if (give) checks = sim.GiveOrder(mission.Marines, who, order);
                else
                {
                    var list = sim.UnitsCalled(mission.Marines, who);
                    checks = list.Count == 0
                        ? new List<OrderCheck> { sim.GiveOrder(mission.Marines, who, order)[0] }
                        : list.Select(u => sim.Check(u, order)).ToList();
                }
                foreach (var c in checks)
                {
                    w.WriteStartObject();
                    w.WriteNumber("order", n);
                    w.WriteString("who", who);
                    w.WriteNumber("u", c.Unit == null ? -1 : IndexOf(c.Unit));
                    w.WriteBoolean("ok", c.Ok);
                    if (c.Problem != null) w.WriteString("problem", c.Problem);
                    w.WriteStartArray("warnings");
                    foreach (var x in c.Warnings) w.WriteStringValue(x);
                    w.WriteEndArray();
                    if (c.Path != null)
                    {
                        w.WriteStartArray("path");
                        foreach (var p in c.Path) { w.WriteNumberValue(R(p.X)); w.WriteNumberValue(R(p.Y)); }
                        w.WriteEndArray();
                        w.WriteNumber("meters", R(Pathfinder.Length(c.Unit.Position, c.Path)));
                    }
                    w.WriteEndObject();
                }
                n++;
            }
            w.WriteEndArray();
            if (give) WriteState(w);
        }

        // ---------- helpers ----------

        static int IndexOf(Unit u)
        {
            var list = mission.Sim.Units;
            for (int i = 0; i < list.Count; i++) if (ReferenceEquals(list[i], u)) return i;
            return -1;
        }

        static string Str(JsonElement o, string name) =>
            o.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()) ? v.GetString().Trim() : null;

        static double R(float v) => Math.Round(v, 1);

        static void WriteCells(Utf8JsonWriter w, IReadOnlyList<(int x, int y, string terrain)> cells)
        {
            w.WriteStartArray("cells");
            foreach (var (x, y, t) in cells) { w.WriteNumberValue(x); w.WriteNumberValue(y); w.WriteStringValue(t); }
            w.WriteEndArray();
        }

        static Vec2 Inside(MapGrid map, Vec2 p) =>
            new Vec2(Math.Max(0f, Math.Min(map.WidthMeters - 0.01f, p.X)), Math.Max(0f, Math.Min(map.HeightMeters - 0.01f, p.Y)));

        static string Json(Action<Utf8JsonWriter> body)
        {
            try
            {
                var buffer = new ArrayBufferWriter<byte>(1 << 16);
                using (var w = new Utf8JsonWriter(buffer))
                {
                    w.WriteStartObject();
                    body(w);
                    w.WriteEndObject();
                }
                return Encoding.UTF8.GetString(buffer.WrittenSpan);
            }
            catch (Exception e)
            {
                return "{\"error\":" + JsonSerializer.Serialize(e.Message) + "}";
            }
        }
    }
}
