using System.Collections.Generic;
using UnityEngine;

public static class WordDatabase
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    static readonly string[] EasyWords =
    {
        "cat", "dog", "run", "sun", "sky", "sea", "red", "win",
        "neko", "inu", "mizu", "kaze", "yama", "sora", "hana"
    };

    static readonly string[] NormalWords =
    {
        "sword", "magic", "shield", "attack", "dragon", "battle",
        "samurai", "ninja", "katana", "sakura", "quest", "power",
        "combo", "bonus", "speed", "victory", "defend", "hero"
    };

    static readonly string[] HardWords =
    {
        "adventure", "challenge", "lightning", "avalanche", "champion",
        "shuriken", "kusarigama", "yamabushi", "kaminari", "bakuhatsu"
    };

    public static string GetRandomWord(Difficulty difficulty)
    {
        string[] pool = difficulty switch
        {
            Difficulty.Easy => EasyWords,
            Difficulty.Normal => NormalWords,
            Difficulty.Hard => HardWords,
            _ => EasyWords
        };

        return pool[Random.Range(0, pool.Length)];
    }

    public static Difficulty GetDifficultyForWave(int wave)
    {
        if (wave < 3) return Difficulty.Easy;
        if (wave < 7) return Difficulty.Normal;
        return Difficulty.Hard;
    }
}
