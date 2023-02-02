using System;

[Serializable]
public class Score
{
    public string name;
    public int points ;

    public Score(string username, int score)
    {
        name = username;
        points = score;
    }
}

[Serializable]
public class Scores
{
    public Score[] items;

    public Scores(Score[] items)
    {
        this.items = items;
    }
}