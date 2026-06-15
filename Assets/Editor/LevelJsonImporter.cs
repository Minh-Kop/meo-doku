using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

/// <summary>
///     Chuyển toàn bộ file JSON trong Assets/Levels/export/ thành PuzzleData .asset.
///     Menu → Tools → Bunnydoku → Import Levels From JSON
/// </summary>
public static class LevelJsonImporter
{
    private const string SourceFolder = "Assets/Levels/export";
    private const string OutputFolder = "Assets/Levels/Generated";

    [MenuItem("Tools/Bunnydoku/Import Levels From JSON")]
    public static void ImportAll()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets/Levels", "Generated");
        }

        var files = Directory.GetFiles(SourceFolder, "*.json");
        // Sắp xếp theo số: level_1, level_2, ... level_10 (không phải thứ tự chữ cái).
        Array.Sort(files, (a, b) => ExtractNumber(a).CompareTo(ExtractNumber(b)));

        var count = 0;
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<LevelJson>(json);

            var puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.gridSize = data.size;
            puzzle.regionMap = FlattenMap(data.map, data.size);
            puzzle.revealCells = ConvertRevealCells(data.reveal, data.size);
            puzzle.solutionCells = ConvertSolution(data.solution);
            puzzle.steps = data.steps;

            var name = Path.GetFileNameWithoutExtension(file);
            AssetDatabase.CreateAsset(puzzle, $"{OutputFolder}/{name}.asset");
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Đã import {count} level vào {OutputFolder}");
    }

    private static int[] FlattenMap(int[][] map, int n)
    {
        var flat = new int[n * n];
        for (var r = 0; r < n; r++)
        for (var c = 0; c < n; c++)
        {
            flat[r * n + c] = map[r][c];
        }

        return flat;
    }

    private static int[] ConvertRevealCells(int[][] input, int gridSize)
    {
        if (input == null)
        {
            return Array.Empty<int>();
        }

        var result = new int[input.Length];

        for (var i = 0; i < input.Length; i++)
        {
            result[i] = input[i][0] * gridSize + input[i][1]; // result[i] = row * gridSize + col
        }

        return result;
    }

    private static int[] ConvertSolution(int[][] input)
    {
        if (input == null)
        {
            return Array.Empty<int>();
        }

        var result = new int[input.Length];
        foreach (var p in input)
        {
            result[p[0]] = p[1]; // result[row] = col
        }

        return result;
    }

    private static int ExtractNumber(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path); // level_12
        var us = name.LastIndexOf('_');
        return us >= 0 && int.TryParse(name.Substring(us + 1), out var n) ? n : 0;
    }

    // Khớp với cấu trúc JSON của bạn.
    private class LevelJson
    {
        public int[][] map;
        public int[][] reveal;
        public int size;
        public int[][] solution;
        public int[] steps;
    }
}
