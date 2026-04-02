using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private SpriteRenderer backgroundRenderer; 

    private Vector3 _startPos;
    private Board _board;
    private bool _isDragging = false;
    private List<Vector2Int> _previewCoords = new List<Vector2Int>();

    private float _blockDragOffset = 2.0f; 
    private float scaleOnGrab = 1f; 
    private float worldCellSize = 1f; 
    public int[,] ShapeData { get; private set; } 

    private float _defaultScale = 1f;
    private Vector3 _originalSpawnPos;
    
    public int shapeIndex; // Lưu index của hình khối
    public int slotIndex;  // Lưu index vị trí spawn (0, 1, 2)
    void Start()
    {
        _board = Object.FindFirstObjectByType<Board>();

        if (backgroundRenderer == null)
        {
            GameObject bg = GameObject.Find("background");
            if (bg != null)
            {
                backgroundRenderer = bg.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void SetDefaultScale(float scale, Vector3 pos)
    {
        _defaultScale = scale;
        _originalSpawnPos = pos;
        _startPos = pos;
        transform.position = pos;
        transform.localScale = Vector3.one * _defaultScale;
    }

    public void Initialize(int[,] shapeData)
    {
        this.ShapeData = shapeData;
        foreach (Transform child in transform) Destroy(child.gameObject);
        foreach (BoxCollider2D col in GetComponents<BoxCollider2D>()) Destroy(col);

        int rows = shapeData.GetLength(0);
        int cols = shapeData.GetLength(1);
        int minRow = int.MaxValue, maxRow = int.MinValue;
        int minCol = int.MaxValue, maxCol = int.MinValue;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (shapeData[r, c] == 1)
                {
                    if (r < minRow) minRow = r;
                    if (r > maxRow) maxRow = r;
                    if (c < minCol) minCol = c;
                    if (c > maxCol) maxCol = c;
                }
            }
        }

        float centerCol = (minCol + maxCol) / 2f;
        float centerRow = (minRow + maxRow) / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (shapeData[r, c] == 1)
                {
                    Vector3 pos = new Vector3(c - centerCol, r - centerRow, 0);
                    GameObject cell = Instantiate(cellPrefab, transform);
                    cell.transform.localPosition = pos;

                    BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
                    boxCol.offset = pos;
                    boxCol.size = new Vector2(0.9f, 0.9f);
                }
            }
        }
    }

    public void StartDragging()
    {
        _isDragging = true;
        
        // Đưa block ra phía trước cùng khi bắt đầu kéo
        SetSortingOrder(100);

        if (_board != null)
        {
            Cell c0 = _board.GetCellAt(0, 0);
            Cell c1 = _board.GetCellAt(0, 1);
            if (c0 != null && c1 != null)
            {
                worldCellSize = Vector3.Distance(c0.transform.position, c1.transform.position);
                scaleOnGrab = worldCellSize;
            }
        }

        transform.localScale = Vector3.one * scaleOnGrab; 
    }

    void Update()
    {
        // MỚI: Nếu game đang dừng (paused), không cập nhật di chuyển khối nữa
        if (Time.timeScale == 0) return;

        if (_isDragging && Pointer.current != null)
        {
            // Lấy vị trí chuột trong không gian thế giới
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            mousePos.z = -2f; // Ép Z để luôn nhìn thấy trước Board

            // Tính toán vị trí mục tiêu (chuột nằm ở dưới, khối nằm ở trên)
            Vector3 targetPos = mousePos + Vector3.up * _blockDragOffset;

            // Chỉ giới hạn nếu khối đang đi sâu vào bàn chơi
            if (backgroundRenderer != null && targetPos.y > backgroundRenderer.bounds.min.y)
            {
                targetPos = ClampBlockInsideBackground(targetPos);
            }

            transform.position = targetPos;
            UpdatePreview();

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _isDragging = false;
                ClearPreview();
                SetSortingOrder(0);

                if (HoldManager.Instance != null && HoldManager.Instance.IsOverHoldPanel(Pointer.current.position.ReadValue()))
                {
                    if (HoldManager.Instance.HoldSelectedBlock())
                    {
                        // Sau khi cất vào Hold, check xem có hết khối chưa để sinh wave mới
                        Blocks manager = Object.FindFirstObjectByType<Blocks>();
                        if (manager != null) manager.CheckAndResetBlocks();
                    }
                    else
                    {
                        // Nếu đã dùng lượt hold rồi, bắt khối bay về chỗ cũ
                        transform.position = _originalSpawnPos;
                        transform.localScale = Vector3.one * _defaultScale;
                        
                        if (slotIndex == -99)
                        {
                            HoldManager.Instance.ReRegisterHeldBlock(this);
                        }
                    }
                }
                else if (TryPlace())
                {
                    gameObject.SetActive(false);
                    if (_board != null) _board.CheckAndClearLines();

                    if (HoldManager.Instance != null) HoldManager.Instance.ResetHoldTurn();

                    Blocks manager = Object.FindFirstObjectByType<Blocks>();
                    if (manager != null) manager.CheckAndResetBlocks();
                }
                else
                {
                    // Nếu đặt hụt, cho bay về vị trí cũ
                    transform.position = _originalSpawnPos;
                    transform.localScale = Vector3.one * _defaultScale;

                    // MỚI: Nếu là khối ô HOLD, đăng ký lại với HoldManager để lần sau kéo tiếp được
                    if (slotIndex == -99 && HoldManager.Instance != null)
                    {
                        HoldManager.Instance.ReRegisterHeldBlock(this);
                    }
                }
            }
        }
    }

    private void SetSortingOrder(int order)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingOrder = order;
        }
    }

    public void SetTransparency(bool isFaded)
    {
        float alpha = isFaded ? 0.35f : 1.0f;
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }


    private Vector3 ClampBlockInsideBackground(Vector3 targetPos)
    {
        if (backgroundRenderer == null)
            return targetPos;

        // Bounds của background
        Bounds bgBounds = backgroundRenderer.bounds;

        // Tính bounds hiện tại của toàn bộ block nếu đặt ở targetPos
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        Vector3 offset = targetPos - transform.position;

        foreach (Transform child in transform)
        {
            Vector3 futurePos = child.position + offset;

            if (futurePos.x < minX) minX = futurePos.x;
            if (futurePos.x > maxX) maxX = futurePos.x;
            if (futurePos.y < minY) minY = futurePos.y;
            if (futurePos.y > maxY) maxY = futurePos.y;
        }

    // Tạo độ đệm để block không dính sát mép gỗ
    float padding = 0.8f;

    // Nếu block tràn trái
    if (minX < bgBounds.min.x + padding)
    {
        targetPos.x += (bgBounds.min.x + padding - minX);
    }

        // Nếu block tràn phải
        if (maxX > bgBounds.max.x - padding)
        {
            targetPos.x -= (maxX - (bgBounds.max.x - padding));
        }

        // Nếu block tràn dưới
        if (minY < bgBounds.min.y + padding)
        {
            targetPos.y += (bgBounds.min.y + padding - minY);
        }

        // Nếu block tràn trên
        if (maxY > bgBounds.max.y - padding)
        {
            targetPos.y -= (maxY - (bgBounds.max.y - padding));
        }

        return targetPos;
    }

    private bool GetGridPos(Transform child, out int r, out int c)
    {
        r = -1; c = -1;
        if (_board == null) return false;

        Cell originCell = _board.GetCellAt(0, 0);
        if (originCell == null) return false;

        // Tính khoảng cách so với ô gốc (0,0)
        Vector3 worldDiff = child.position - originCell.transform.position;

        // Làm tròn lấy số nguyên chính xác (hít vào giữa ô)
        c = Mathf.RoundToInt(worldDiff.x / worldCellSize);
        r = Mathf.RoundToInt(worldDiff.y / worldCellSize);

        return true;
    }

    private void UpdatePreview()
    {
        if (_board == null) return;

        List<Vector2Int> newCoords = new List<Vector2Int>();
        bool isValid = true;

        foreach (Transform child in transform)
        {
            GetGridPos(child, out int r, out int c);
            Cell cellOnBoard = _board.GetCellAt(r, c);

            if (cellOnBoard == null || cellOnBoard.isFilled)
            {
                isValid = false;
                break;
            }
            newCoords.Add(new Vector2Int(r, c));
        }

        if (isValid)
        {
            _previewCoords = newCoords;
            _board.ShowPotentialClears(_previewCoords);
        }
        else
        {
            ClearPreview();
        }
    }

    private void ClearPreview()
    {
        if (_board != null)
        {
            _board.ShowPotentialClears(new List<Vector2Int>());
        }
        _previewCoords.Clear();
    }

    private bool TryPlace()
    {
        if (_board == null) return false;
        List<Vector2Int> targetCoords = new List<Vector2Int>();

        foreach (Transform child in transform)
        {
            GetGridPos(child, out int r, out int c);
            Cell cellOnBoard = _board.GetCellAt(r, c);
            if (cellOnBoard == null || cellOnBoard.isFilled) return false;
            targetCoords.Add(new Vector2Int(r, c));
        }

        foreach (Vector2Int coord in targetCoords)
        {
            _board.SetCellVisibility(coord.x, coord.y, true);
        }
        return true;
    }
}