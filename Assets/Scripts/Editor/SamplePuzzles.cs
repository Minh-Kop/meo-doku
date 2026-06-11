using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
///     Công cụ tạo nhanh các PuzzleData mẫu để test trong Editor.
///     Cách dùng:
///     Menu → Tools → Bunnydoku → Create Sample Puzzles
///     → Tự động tạo 3 file .asset trong Assets/Puzzles/
///     Đây không phải runtime script — chỉ dùng trong Editor.
/// </summary>
public static class SamplePuzzles
{
#if UNITY_EDITOR
    [MenuItem("Tools/Bunnydoku/Create Sample Puzzles")]
    public static void CreateAll()
    {
        CreatePuzzle5x5();
        CreatePuzzle6x6();
        CreatePuzzle7x7();
        AssetDatabase.SaveAssets();
        Debug.Log("✅ Đã tạo 3 puzzle mẫu trong Assets/Puzzles/");
    }

    // ── 5×5 ────────────────────────────────────────────────────────────────
    // Regions:
    //   0 0 1 1 1
    //   0 0 2 1 3
    //   0 2 2 3 3
    //   2 2 4 4 3
    //   4 4 4 4 3
    //
    // Solution (1 thỏ mỗi vùng, không chạm nhau):
    //   row=0,col=1 (region 0)
    //   row=1,col=3 (region 1)
    //   row=3,col=2 (region 4) → tuỳ bạn verify
    //   ...
    private static void CreatePuzzle5x5()
    {
        var p = ScriptableObject.CreateInstance<PuzzleData>();
        p.gridSize = 5;
        p.regionMap = new[]
        {
            0,
            0,
            1,
            1,
            1,
            0,
            0,
            2,
            1,
            3,
            0,
            2,
            2,
            3,
            3,
            2,
            2,
            4,
            4,
            3,
            4,
            4,
            4,
            4,
            3,
        };

        EnsureFolder("Assets/Puzzles");
        AssetDatabase.CreateAsset(p, "Assets/Puzzles/Puzzle_5x5_01.asset");
    }

    // ── 6×6 ────────────────────────────────────────────────────────────────
    private static void CreatePuzzle6x6()
    {
        var p = ScriptableObject.CreateInstance<PuzzleData>();
        p.gridSize = 6;
        p.regionMap = new[]
        {
            0,
            0,
            0,
            1,
            1,
            1,
            0,
            2,
            2,
            2,
            1,
            1,
            0,
            2,
            3,
            3,
            4,
            1,
            5,
            2,
            3,
            4,
            4,
            4,
            5,
            5,
            3,
            3,
            4,
            4,
            5,
            5,
            5,
            3,
            4,
            4,
        };

        EnsureFolder("Assets/Puzzles");
        AssetDatabase.CreateAsset(p, "Assets/Puzzles/Puzzle_6x6_01.asset");
    }

    // ── 7×7 ────────────────────────────────────────────────────────────────
    private static void CreatePuzzle7x7()
    {
        var p = ScriptableObject.CreateInstance<PuzzleData>();
        p.gridSize = 7;
        p.regionMap = new[]
        {
            0,
            0,
            0,
            1,
            1,
            2,
            2,
            0,
            0,
            1,
            1,
            2,
            2,
            2,
            0,
            3,
            3,
            1,
            2,
            4,
            4,
            3,
            3,
            3,
            5,
            4,
            4,
            4,
            3,
            6,
            5,
            5,
            5,
            4,
            4,
            6,
            6,
            6,
            5,
            5,
            5,
            4,
            6,
            6,
            6,
            6,
            5,
            5,
            5,
        };

        EnsureFolder("Assets/Puzzles");
        AssetDatabase.CreateAsset(p, "Assets/Puzzles/Puzzle_7x7_01.asset");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
#endif
}
