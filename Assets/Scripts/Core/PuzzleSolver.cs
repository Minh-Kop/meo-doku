using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    ///     Backtracking solver cho Bunnydoku.
    ///     Tìm cách đặt N thỏ thỏa mãn:
    ///     • Mỗi hàng có đúng 1 thỏ
    ///     • Mỗi cột có đúng 1 thỏ
    ///     • Mỗi region có đúng 1 thỏ
    ///     • Không có 2 thỏ nào chạm nhau (8 hướng)
    ///     Dùng để:
    ///     • Kiểm tra puzzle có giải được không (khi thiết kế level)
    ///     • Kiểm tra puzzle có ĐÚNG 1 đáp án (unique solution)
    ///     • Sinh đáp án để debug / auto-solve / auto-test
    ///     Đây là static utility — không cần gắn vào GameObject.
    /// </summary>
    public static class PuzzleSolver
    {
        /// <summary>
        ///     Tìm một đáp án hợp lệ.
        ///     Trả về mảng solution[row] = col (cột đặt thỏ ở hàng row),
        ///     hoặc null nếu puzzle không có đáp án nào.
        /// </summary>
        public static int[] FindSolution(PuzzleData puzzle)
        {
            var n = puzzle.gridSize;

            var solution = new int[n];
            var usedCols = new bool[n];
            // var usedRegionsMap = new NativeHashMap<int, bool>(n, Allocator.Temp);
            var usedRegionsMap = new Dictionary<int, bool>();

            for (var i = 0; i < n; i++)
            {
                solution[i] = -1;
            }

            var result = Solve(puzzle, 0, solution, usedCols, usedRegionsMap) ? solution : null;
            // usedRegionsMap.Dispose();
            return result;
        }

        /// <summary>
        ///     Đếm số đáp án hợp lệ, dừng sớm khi đạt <paramref name="cap" />.
        ///     cap = 2 (mặc định) → chỉ cần biết puzzle có UNIQUE solution (đếm = 1) hay không.
        /// </summary>
        public static int CountSolutions(PuzzleData puzzle, int cap = 2)
        {
            var n = puzzle.gridSize;

            var solution = new int[n];
            var usedCols = new bool[n];
            var usedRegionMap = new Dictionary<int, bool>();

            for (var i = 0; i < n; i++)
            {
                solution[i] = -1;
            }

            var count = 0;
            CountSolve(puzzle, 0, solution, usedCols, usedRegionMap, ref count, cap);
            return count;
        }

        // ── Private: tìm 1 đáp án ──────────────────────────────────────────────

        private static bool Solve(
            PuzzleData puzzle,
            int row,
            int[] solution,
            bool[] usedCols,
            Dictionary<int, bool> usedRegionMap
        )
        {
            var n = puzzle.gridSize;

            if (row == n)
            {
                return true; // đã đặt đủ N thỏ hợp lệ
            }

            for (var col = 0; col < n; col++)
            {
                if (usedCols[col])
                {
                    continue;
                }

                var region = puzzle.regionMap[row * n + col];
                if (usedRegionMap.ContainsKey(region) && usedRegionMap[region])
                {
                    continue;
                }

                // Chạm với thỏ ở hàng trước? (chỉ cần check hàng liền trước)
                if (row > 0 && Mathf.Abs(col - solution[row - 1]) <= 1)
                {
                    continue;
                }

                // ── Đặt thử ──
                solution[row] = col;
                usedCols[col] = true;
                usedRegionMap[region] = true;

                if (Solve(puzzle, row + 1, solution, usedCols, usedRegionMap))
                {
                    return true;
                }

                // ── Backtrack ──
                usedCols[col] = false;
                usedRegionMap[region] = false;
                solution[row] = -1;
            }

            return false; // không có col nào hợp lệ ở hàng này
        }

        // ── Private: đếm tổng số đáp án (có cap) ───────────────────────────────

        private static void CountSolve(
            PuzzleData puzzle,
            int row,
            int[] solution,
            bool[] usedCols,
            Dictionary<int, bool> usedRegionMap,
            ref int count,
            int cap
        )
        {
            if (count >= cap)
            {
                return;
            }

            var n = puzzle.gridSize;

            if (row == n)
            {
                count++;
                return;
            }

            for (var col = 0; col < n; col++)
            {
                if (usedCols[col])
                {
                    continue;
                }

                var region = puzzle.regionMap[row * n + col];
                if (usedRegionMap.ContainsKey(region) && usedRegionMap[region])
                {
                    continue;
                }

                if (row > 0 && Mathf.Abs(col - solution[row - 1]) <= 1)
                {
                    continue;
                }

                solution[row] = col;
                usedCols[col] = true;
                usedRegionMap[region] = true;

                CountSolve(puzzle, row + 1, solution, usedCols, usedRegionMap, ref count, cap);

                usedCols[col] = false;
                usedRegionMap[region] = false;
                solution[row] = -1;

                if (count >= cap)
                {
                    return; // dừng sớm ngay khi đủ cap
                }
            }
        }
    }
}
