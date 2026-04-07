using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Board : MonoBehaviour
{
    private const int Size = 8;

    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform cellsTransform; 
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Layout")]
    [SerializeField] private float yOffset = 0f;

    private readonly Cell[,] cells = new Cell[Size, Size];
    private int currentScore = 0;
    private int bestScore = 0;

    public int GetCurrentScore() => currentScore;

    void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateScoreUI();
        GenerateBoard();
    }

    void GenerateBoard()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c] = Instantiate(cellPrefab, cellsTransform);
                
                float posX = c - Size / 2f + 0.5f;
                float posY = r - Size / 2f + 0.5f + yOffset;
                cells[r, c].transform.localPosition = new Vector3(posX, posY, 0);
                
                cells[r, c].SetFilled(false);

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

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString();
        if (bestScoreText != null) bestScoreText.text = bestScore.ToString();
    }

    public void AddPlacementScore(int cellCount, Vector3 pos)
    {
        int score = cellCount * 10;
        AddScore(score, pos, false); // Không hiện text khi đặt khối
    }

    public void AddScore(int amount, Vector3 pos, bool spawnText = true)
    {
        currentScore += amount;
        UpdateScoreUI();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(amount, pos, spawnText);
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

        HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();

        foreach (int r in rowsToClear)
            for (int c = 0; c < Size; c++) cellsToClear.Add(new Vector2Int(r, c));

        foreach (int c in colsToClear)
            for (int r = 0; r < Size; r++) cellsToClear.Add(new Vector2Int(r, c));

        foreach (Vector2Int coord in cellsToClear)
        {
            cells[coord.x, coord.y].ClearWithAnimation();
        }

        int totalLines = rowsToClear.Count + colsToClear.Count;

        if (totalLines > 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.clearLineSound);

            // CÔNG THỨC ĐIỂM MỚI: Thăng tiến theo số hàng xóa được
            int scoreToAdd = 0;
            switch(totalLines)
            {
                case 1: scoreToAdd = 100; break;
                case 2: scoreToAdd = 300; break; 
                case 3: scoreToAdd = 600; break;
                case 4: scoreToAdd = 1000; break;
                default: scoreToAdd = totalLines * 300; break;
            }

            // Cộng điểm và hiện text ở trung tâm bàn cờ (hoặc vị trí dòng cuối cùng)
            AddScore(scoreToAdd, transform.position + new Vector3(0, yOffset, -1f));
        }
    }

    public void SetCellVisibility(int r, int c, bool visible)
    {
        if (r < 0 || r >= Size || c < 0 || c >= Size) return;

        Cell cell = cells[r, c];
        cell.gameObject.SetActive(visible); 
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
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                if (cells[i, j] != null) cells[i, j].SetHighlightPreview(false, false);

        if (previewCoords.Count == 0) return;

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

        foreach (Vector2Int coord in previewCoords)
        {
            if (cells[coord.x, coord.y] != null) cells[coord.x, coord.y].SetHighlightPreview(true, false);
        }

        foreach (int r in potentialRows)
            for (int c = 0; c < Size; c++) 
                if (cells[r, c] != null) cells[r, c].SetHighlightPreview(true, true);

        foreach (int c in potentialCols)
            for (int r = 0; r < Size; r++) 
                if (cells[r, c] != null) cells[r, c].SetHighlightPreview(true, true);
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
        
        if (!hasAtLeastOne) return true; 

        int shapeH = maxR - minR + 1;
        int shapeW = maxC - minC + 1;
        for (int r = 0; r <= Size - shapeH; r++)
        {
            for (int c = 0; c <= Size - shapeW; c++)
            {
                bool canPlaceAtPos = true;
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
                if (canPlaceAtPos) return true; 
            }
        }
        return false;
    }

    public void ClearMiddleRows()
    {
        // 8x8 middle rows = 3, 4
        for (int c = 0; c < Size; c++)
        {
            cells[3, c].ClearWithAnimation();
            cells[4, c].ClearWithAnimation();
        }
    }
}