namespace AlgoVis.Server.Ratings;

/// <summary>
/// Формула уровней. XP для перехода на уровень N: 50 * (N-1) * N.
/// 1 → 0, 2 → 100, 3 → 300, 4 → 600, 5 → 1000, ...
/// </summary>
public static class LevelCalculator
{
    public static int LevelFromXp(long xp)
    {
        if (xp < 100) return 1;
        // Находим N: 50*(N-1)*N <= xp
        // N² - N - xp/50 ≤ 0
        var n = (int)Math.Floor((1 + Math.Sqrt(1 + 8.0 * xp / 100)) / 2);
        return Math.Max(1, n);
    }

    public static long XpForLevel(int level)
    {
        if (level <= 1) return 0;
        return 50L * (level - 1) * level;
    }
}
