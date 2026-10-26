using System;
using System.IO;

namespace FieldTable.Sim.Tests
{
    /// <summary>Finds the game's data files in the repo's Data folder.</summary>
    static class TestData
    {
        public static string Read(string relativePath)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "Data", relativePath);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = dir.Parent;
            }
            throw new FileNotFoundException("Couldn't find Data/" + relativePath + " above the test folder.");
        }

        static readonly Lazy<MapGrid> cinderFord = new Lazy<MapGrid>(() =>
        {
            var data = GameData.FromJson(Read("terrain.json"));
            return MapData.FromJson(Read("maps/cinder-ford.json")).Build(data.Terrains);
        });

        /// <summary>The Cinder Ford map, built once and shared, since tests only read it.</summary>
        public static MapGrid CinderFord() => cinderFord.Value;

        /// <summary>A grid square's position in meters, like the prototype's maps: 4.45 means 445 m.</summary>
        public static Vec2 At(float x, float y) => new Vec2(x * 100f, y * 100f);
    }
}
