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
            foreach (var t in Terrains)
                if (t.MoveFactor < 0f || t.Cover < 0f || t.Cover > 1f)
                    throw new FormatException("Terrain " + t.Id + " has a move factor below 0 or cover outside 0 to 1.");
            foreach (var u in Units)
                if (u.WalkSpeed <= 0f || u.RunSpeed <= 0f || u.CrawlSpeed <= 0f || u.Ammo < 0)
                    throw new FormatException("Unit " + u.Id + " needs speeds above 0 and ammo of 0 or more.");
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
