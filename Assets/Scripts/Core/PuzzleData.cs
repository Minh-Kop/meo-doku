using UnityEngine;

/// <summary>
///     ScriptableObject lưu dữ liệu một puzzle:
///     • gridSize  — kích thước lưới (ví dụ 5 = lưới 5×5)
///     • regionMap — mảng flat [gridSize*gridSize], giá trị = region ID (0..gridSize-1)
///     Cách tạo asset:
///     Project window → chuột phải → Create → Bunnydoku → Puzzle
/// </summary>
[CreateAssetMenu(menuName = "Bunnydoku/Puzzle", fileName = "NewPuzzle")]
public class PuzzleData : ScriptableObject
{
    [Tooltip("Kích thước lưới N×N (5 đến 12).")]
    public int gridSize;

    [Tooltip(
        "Flat array kích thước gridSize*gridSize. regionMap[row * gridSize + col] = regionId."
    )]
    public int[] regionMap;

    [Tooltip("Các ô được lộ sẵn (đáp án cho trước). Flat: revealCells[i*2]=row, +1=col.")]
    public int[] revealCells;

    [Tooltip("Đáp án đầy đủ. Flat: solutionCells[i*2]=row, +1=col.")]
    public int[] solutionCells;

    public int[] steps;
}
