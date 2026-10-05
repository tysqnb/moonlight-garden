using UnityEngine;

/// <summary>
/// Keeps the lantern progress while the reader moves between screens.
/// Six lanterns for six pages, in reading order (home, four scenes, credits).
/// </summary>
public static class ProgressManager
{
    // How many lanterns are lit. Page 1 (home) starts with one lantern lit.
    public static int Lit = 1;

    public static void Visit(int pageIndex)
    {
        Lit = Mathf.Max(Lit, Mathf.Clamp(pageIndex + 1, 1, 6));
    }

    public static void Light(int count)
    {
        Lit = Mathf.Max(Lit, Mathf.Clamp(count, 1, 6));
    }

    public static void ResetProgress()
    {
        Lit = 1;
    }
}
