using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FieldTable.Sim
{
    /// <summary>
    /// The definitions a setting or mod brings: kinds of terrain and kinds of units.
    /// Loaded from JSON data files.
    /// </summary>
    public sealed class GameData
    {
        public List<TerrainDef> Terrains { get; set; } = new List<TerrainDef>();
        public List<UnitDef> Units { get; set; } = new List<UnitDef>();
        public List<WeaponDef> Weapons { get; set; } = new List<WeaponDef>();

        /// <summary>Puts several data files together, like terrain.json and units.json. Ids must stay unique.</summary>
        public static GameData Combine(params GameData[] parts)
        {
            var all = new GameData();
            foreach (var p in parts)
            {
                all.Terrains.AddRange(p.Terrains);
                all.Units.AddRange(p.Units);
                all.Weapons.AddRange(p.Weapons);
            }
            all.Check();
            return all;
        }

        public WeaponDef FindWeapon(string id)
        {
            foreach (var w in Weapons)
                if (w.Id == id) return w;
            throw new KeyNotFoundException("No weapon called " + id + " in the data.");
        }

        public static GameData FromJson(string json)
        {
            GameData data;
            try
            {
                data = JsonConvert.DeserializeObject<GameData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                });
            }
            catch (JsonException e)
            {
                throw new FormatException("The data file isn't valid: " + e.Message, e);
            }
            if (data == null) throw new FormatException("The data file is empty.");
            data.Check();
            return data;
        }

        public UnitDef FindUnit(string id)
        {
            foreach (var u in Units)
                if (u.Id == id) return u;
            throw new KeyNotFoundException("No unit called " + id + " in the data.");
        }

        void Check()
        {
            CheckIds("terrain", Terrains, t => t.Id);
            CheckIds("unit", Units, u => u.Id);
            CheckIds("weapon", Weapons, w => w.Id);
            foreach (var w in Weapons)
                if (w.Range <= 0f || w.Burst < 1 || w.Cycle <= 0f || w.Accuracy < 0f || w.Accuracy > 1f || w.Suppress < 0f || w.Blast < 0f || w.ApBlast < 0f || w.Lethality < 0f || w.Lethality > 1f)
                    throw new FormatException("Weapon " + w.Id + " needs a range, burst, and cycle above 0, and accuracy and lethality from 0 to 1.");
            foreach (var t in Terrains)
                if (t.MoveFactor < 0f || t.Cover < 0f || t.Cover > 1f || t.Height < 0f || t.SightBlock < 0f || t.Concealment < 0f || t.Concealment > 1f)
                    throw new FormatException("Terrain " + t.Id + " needs a move factor, height, and sight block of 0 or more, and cover and concealment from 0 to 1.");
            if (Weapons.Count > 0)
                foreach (var u in Units)
                {
                    if (u.Weapon != null && !Weapons.Exists(w => w.Id == u.Weapon)) throw new FormatException("Unit " + u.Id + " carries " + u.Weapon + ", which isn't in the weapon list.");
                    if (u.Launcher != null && !Weapons.Exists(w => w.Id == u.Launcher)) throw new FormatException("Unit " + u.Id + " carries " + u.Launcher + ", which isn't in the weapon list.");
                }
            foreach (var u in Units)
            {
                if (u.WalkSpeed <= 0f || u.RunSpeed <= 0f || u.CrawlSpeed <= 0f || u.Ammo < 0 || u.Load < 0f || u.Grenades < 0 || u.LauncherAmmo < 0 || u.LauncherAp < 0 || u.Smoke < 0 || u.Altitude < 0f)
                    throw new FormatException("Unit " + u.Id + " needs speeds above 0 and ammo, grenades, smoke, and load of 0 or more.");
                if (u.Drone != null && !Units.Exists(d => d.Id == u.Drone && d.Flying)) throw new FormatException("Unit " + u.Id + " flies " + u.Drone + ", which isn't a flying unit in the data.");
            }
        }

        static void CheckIds<T>(string kind, List<T> items, Func<T, string> id)
        {
            if (items == null) throw new FormatException("The " + kind + " list is missing.");
            var seen = new HashSet<string>();
            foreach (var item in items)
            {
                string i = id(item);
                if (string.IsNullOrWhiteSpace(i)) throw new FormatException("A " + kind + " is missing its id.");
                if (!seen.Add(i)) throw new FormatException("Two " + kind + "s share the id " + i + ".");
            }
        }
    }
}
