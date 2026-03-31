using UnityEngine;
public static class Poliominus
{
    public static readonly int[][,] Shapes = new int[][,]
    {
        // 1. Single Dot (1x1)
        new int[,] {{1}},

        // 2. Lines (2 cells)
        new int[,] {{1, 1}},
        new int[,] {{1}, {1}},

        // 3. Lines (3 cells)
        new int[,] {{1, 1, 1}},
        new int[,] {{1}, {1}, {1}},

        // 4. Lines (4 cells)
        new int[,] {{1, 1, 1, 1}},
        new int[,] {{1}, {1}, {1}, {1}},

        // // 5. Lines (5 cells)
        // new int[,] {{1, 1, 1, 1, 1}},
        // new int[,] {{1}, {1}, {1}, {1}, {1}},

        // 6. Squares
        new int[,] {{1, 1}, {1, 1}},
        new int[,] {{1, 1, 1}, {1, 1, 1}, {1, 1, 1}},

        // 7. Small L / Corners (3 cells)
        new int[,] {{1, 1}, {1, 0}},
        new int[,] {{1, 1}, {0, 1}},
        new int[,] {{1, 0}, {1, 1}},
        new int[,] {{0, 1}, {1, 1}},

        // 8. Big L (3x3 corner - 5 cells)
        new int[,] {{1, 1, 1}, {1, 0, 0}, {1, 0, 0}},
        new int[,] {{1, 1, 1}, {0, 0, 1}, {0, 0, 1}},
        new int[,] {{1, 0, 0}, {1, 0, 0}, {1, 1, 1}},
        new int[,] {{0, 0, 1}, {0, 0, 1}, {1, 1, 1}},

        // 9. Medium L (3 cells in total but 2x2 shape)
        new int[,] {{1, 1, 1}, {1, 0, 0}},
        new int[,] {{1, 1, 1}, {0, 0, 1}},

        // 10. T-Shapes
        new int[,] {{1, 1, 1}, {0, 1, 0}},
        new int[,] {{0, 1, 0}, {1, 1, 1}},
        new int[,] {{1, 0}, {1, 1}, {1, 0}},
        new int[,] {{0, 1}, {1, 1}, {0, 1}},

        // 11. S and Z Shapes
        new int[,] {{0, 1, 1}, {1, 1, 0}},
        new int[,] {{1, 1, 0}, {0, 1, 1}},
        new int[,] {{1, 0}, {1, 1}, {0, 1}},
        new int[,] {{0, 1}, {1, 1}, {1, 0}}
    };
}