using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///     Kiểm tra luật chơi sau mỗi lần người chơi đặt / gỡ thỏ.
///     Luật Bunnydoku:
///     1. Mỗi hàng   có đúng 1 thỏ
///     2. Mỗi cột    có đúng 1 thỏ
///     3. Mỗi region có đúng 1 thỏ
///     4. Không có 2 thỏ nào chạm nhau (kể cả đường chéo — 8 ô xung quanh)
///     Singleton — truy cập qua SolverValidator.Instance.
/// </summary>
public class SolverValidator : MonoBehaviour
{
    public static SolverValidator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Validate toàn bộ board, highlight ô sai, và gọi GameManager khi cần.
    /// </summary>
    public void Validate(Cell[,] cells)
    {
        if (cells == null)
        {
            return;
        }

        var n = cells.GetLength(0);

        // Reset tất cả error trước
        foreach (var c in cells)
        {
            c.SetError(false);
        }

        // Thu thập các ô đang có thỏ
        var bunnies = new List<Cell>();
        foreach (var c in cells)
        {
            if (c.CurrentState == Cell.State.Bunny)
            {
                bunnies.Add(c);
            }
        }

        var anyNewMistake = false;

        foreach (var b in bunnies)
        {
            var error = false;

            // Luật 1: hàng
            if (bunnies.Count(x => x.Row == b.Row) > 1)
            {
                error = true;
            }

            // Luật 2: cột
            if (bunnies.Count(x => x.Col == b.Col) > 1)
            {
                error = true;
            }

            // Luật 3: region
            if (bunnies.Count(x => x.RegionId == b.RegionId) > 1)
            {
                error = true;
            }

            // Luật 4: adjacency (8 hướng)
            foreach (var other in bunnies)
            {
                if (other == b)
                {
                    continue;
                }

                if (Mathf.Abs(other.Row - b.Row) <= 1 && Mathf.Abs(other.Col - b.Col) <= 1)
                {
                    error = true;
                    break;
                }
            }

            if (error)
            {
                b.SetError(true);
                anyNewMistake = true;
            }
        }

        // Thông báo GameManager nếu có lỗi
        if (anyNewMistake)
        {
            GameManager.Instance?.OnMistake();
        }

        // Win condition: đủ N thỏ và không có lỗi nào
        if (bunnies.Count == n && bunnies.All(b => !b.IsError))
        {
            GameManager.Instance?.OnWin();
        }
    }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Tìm một ô có thể xác định chắc chắn bằng logic suy luận.
    ///     Trả về null nếu không tìm được.
    ///     Gọi từ GameManager.UseHint().
    /// </summary>
    public Cell FindHintCell(Cell[,] cells)
    {
        if (cells == null)
        {
            return null;
        }

        var n = cells.GetLength(0);

        // Tập hợp các hàng, cột, region đã có thỏ
        var filledRows = new HashSet<int>();
        var filledCols = new HashSet<int>();
        var filledRegions = new HashSet<int>();

        foreach (var c in cells)
        {
            if (c.CurrentState == Cell.State.Bunny)
            {
                filledRows.Add(c.Row);
                filledCols.Add(c.Col);
                filledRegions.Add(c.RegionId);
            }
        }

        // Với mỗi region chưa có thỏ, đếm số ô còn hợp lệ
        for (var regionId = 0; regionId < n; regionId++)
        {
            if (filledRegions.Contains(regionId))
            {
                continue;
            }

            var candidates = new List<Cell>();

            foreach (var c in cells)
            {
                if (c.RegionId != regionId)
                {
                    continue;
                }

                if (c.CurrentState == Cell.State.Bunny)
                {
                    continue;
                }

                if (filledRows.Contains(c.Row))
                {
                    continue;
                }

                if (filledCols.Contains(c.Col))
                {
                    continue;
                }

                if (IsAdjacentToBunny(c, cells))
                {
                    continue;
                }

                candidates.Add(c);
            }

            // Nếu chỉ còn đúng 1 ô hợp lệ trong region → đây là hint chắc chắn
            if (candidates.Count == 1)
            {
                return candidates[0];
            }
        }

        return null; // Không tìm được ô nào chắc chắn
    }

    // ── Private ────────────────────────────────────────────────────────────

    private bool IsAdjacentToBunny(Cell cell, Cell[,] cells)
    {
        foreach (var other in cells)
        {
            if (other.CurrentState != Cell.State.Bunny)
            {
                continue;
            }

            if (Mathf.Abs(other.Row - cell.Row) <= 1 && Mathf.Abs(other.Col - cell.Col) <= 1)
            {
                return true;
            }
        }

        return false;
    }
}
