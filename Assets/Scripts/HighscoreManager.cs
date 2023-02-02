using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using File = System.IO.File;

public static class HighscoreManager
{
    public static List<Score> Highscores = new();

    public static void Load()
    {
        string jsonfile;
        try
        {
            jsonfile = string.Join('\n', File.ReadAllLines("./scores.json"));
        }
        catch (FileNotFoundException)
        {
            jsonfile = JsonUtility.ToJson(new Scores(new Score[] {}));
            Debug.Log("Created new scores.json");
        }
        Debug.Log(Path.GetFullPath("./scores.json"));

        Highscores = JsonUtility.FromJson<Scores>(jsonfile).items.ToList();
    }

    public static void Save()
    {
        File.WriteAllText("./scores.json", JsonUtility.ToJson(new Scores(Highscores.ToArray()), true));
    }
}