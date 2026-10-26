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
    }
}
