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
        if (combo > 0) Debug.Log("ĂN COMBO " + combo + "! Cộng điểm: " + (combo * 100));
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
}