using UnityEngine;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    private const int Size = 8;

    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform cellsTransform; 
    
    private readonly Cell[,] cells = new Cell[Size, Size];

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c] = Instantiate(cellPrefab, cellsTransform);
                
                float posX = c + 0.5f; 
                float posY = r + 0.5f;
                cells[r, c].transform.localPosition = new Vector3(posX, posY, 0);
                
                cells[r, c].SetFilled(false);

                // Ẩn các ô trên bàn cờ lúc ban đầu
                SpriteRenderer sr = cells[r, c].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = 0f; 
                    sr.color = color;
                }
            }
        }
    }

    public void CheckAndClearLines()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int i = 0; i < Size; i++)
        {
            bool rFull = true, cFull = true;
            for (int j = 0; j < Size; j++)
            {
                if (!cells[i, j].isFilled) rFull = false;
                if (!cells[j, i].isFilled) cFull = false;
            }
            if (rFull) rowsToClear.Add(i);
            if (cFull) colsToClear.Add(i);
        }

        // Tạo tập hợp các ô cần xóa để tránh lặp lại (Fix lỗi syntax)
        HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();

        foreach (int r in rowsToClear)
            for (int c = 0; c < Size; c++) cellsToClear.Add(new Vector2Int(r, c));

        foreach (int c in colsToClear)
            for (int r = 0; r < Size; r++) cellsToClear.Add(new Vector2Int(r, c));

        // Thực hiện xóa mượt mà từng ô duy nhất
        foreach (Vector2Int coord in cellsToClear)
        {
            cells[coord.x, coord.y].ClearWithAnimation();
        }

        // Tính điểm dựa trên số hàng/cột xóa được
        int combo = rowsToClear.Count + colsToClear.Count;
        
        int totalLines = rowsToClear.Count + colsToClear.Count;

        if (totalLines > 0)
        {
            // 2. Công thức tính điểm: 100 điểm mỗi hàng/cột (bạn có thể thay đổi tùy ý)
            int scoreToAdd = totalLines * 100;

            // 3. Gửi điểm sang ScoreManager
            // Ở đây mình dùng FindFirstObjectByType để tìm Script ScoreManager trong Scene
            ScoreManager scoreMgr = Object.FindFirstObjectByType<ScoreManager>();
            
            if (scoreMgr != null)
            {
                scoreMgr.AddScore(scoreToAdd);
                Debug.Log($"Đã xóa {totalLines} dòng. Cộng {scoreToAdd} điểm!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ScoreManager trong Scene để cộng điểm!");
            }
        }
    }

    public void SetCellVisibility(int r, int c, bool visible)
    {
        if (r < 0 || r >= Size || c < 0 || c >= Size) return;

        Cell cell = cells[r, c];
        cell.gameObject.SetActive(visible); // Đảm bảo node đang active để sprite hiện ra
        cell.SetFilled(visible);
        
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = visible ? 1f : 0f; 
            sr.color = color;
        }
    }

    public void ShowPotentialClears(List<Vector2Int> previewCoords)
    {
        // 1. Reset lưới về trạng thái ban đầu của từng ô (ẩn các bóng cũ)
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                cells[i, j].SetHighlightPreview(false, false);

        if (previewCoords.Count == 0) return;

        // 2. Dự báo hàng/cột nào sẽ đầy nếu đặt khối gạch này xuống
        HashSet<int> potentialRows = new HashSet<int>();
        HashSet<int> potentialCols = new HashSet<int>();

        for (int i = 0; i < Size; i++)
        {
            bool rFull = true, cFull = true;
            for (int j = 0; j < Size; j++)
            {
                bool willBeFilledR = cells[i, j].isFilled || previewCoords.Contains(new Vector2Int(i, j));
                bool willBeFilledC = cells[j, i].isFilled || previewCoords.Contains(new Vector2Int(j, i));

                if (!willBeFilledR) rFull = false;
                if (!willBeFilledC) cFull = false;
            }
            if (rFull) potentialRows.Add(i);
            if (cFull) potentialCols.Add(i);
        }

        // 3. HIỆN BÓNG CỦA KHỐI GẠCH (Mờ 40%)
        foreach (Vector2Int coord in previewCoords)
        {
            cells[coord.x, coord.y].SetHighlightPreview(true, false);
        }

        // 4. BƯỚC QUYẾT ĐỊNH (NHƯ ẢNH 2): Nếu là hàng/cột sắp đầy, Sáng rực 100% Alpha và dùng Highlight sprite
        foreach (int r in potentialRows)
            for (int c = 0; c < Size; c++) cells[r, c].SetHighlightPreview(true, true);

        foreach (int c in potentialCols)
            for (int r = 0; r < Size; r++) cells[r, c].SetHighlightPreview(true, true);
    }

    public Cell GetCellAt(int r, int c) {
        if (r >= 0 && r < Size && c >= 0 && c < Size) return cells[r, c];
        return null;
    }

    public bool CanFit(int[,] shape)
    {
        if (shape == null) return false;
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);

        // BƯỚC 1: Tìm bounding box (Khung bao thực sự) của khối gạch
        int minR = int.MaxValue, maxR = int.MinValue;
        int minC = int.MaxValue, maxC = int.MinValue;
        bool hasAtLeastOne = false;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (shape[r, c] == 1) {
                    minR = Mathf.Min(minR, r); maxR = Mathf.Max(maxR, r);
                    minC = Mathf.Min(minC, c); maxC = Mathf.Max(maxC, c);
                    hasAtLeastOne = true;
                }
        
        if (!hasAtLeastOne) return true; // Khối trống thì coi như vừa mọi chỗ

        int shapeH = maxR - minR + 1;
        int shapeW = maxC - minC + 1;

        // BƯỚC 2: Quét toàn bộ bàn cờ (8x8)
        for (int r = 0; r <= Size - shapeH; r++)
        {
            for (int c = 0; c <= Size - shapeW; c++)
            {
                bool canPlaceAtPos = true;
                // Kiểm tra xem vị trí (r, c) có bị ô nào cản không
                for (int sr = 0; sr < shapeH; sr++)
                {
                    for (int sc = 0; sc < shapeW; sc++)
                    {
                        if (shape[minR + sr, minC + sc] == 1)
                        {
                            if (cells[r + sr, c + sc].isFilled)
                            {
                                canPlaceAtPos = false;
                                break;
                            }
                        }
                    }
                    if (!canPlaceAtPos) break;
                }
                if (canPlaceAtPos) return true; // Tìm thấy ít nhất 1 chỗ trống
            }
        }
        return false;
    }
}