using UnityEngine;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    private Vector3 _startPos;
    private Board _board;
    private bool _isDragging = false;
    private List<Vector2Int> _previewCoords = new List<Vector2Int>();

    private float offsetUp = 4.0f; // Đẩy khối lên cao hơn chuột (Sửa trực tiếp ở đây sẽ có tác dụng ngay)
    private float scaleOnGrab = 1f; // Tỷ lệ khi cầm gạch (để 1 là chuẩn nhất)

    private float _defaultScale = 1f;
    private Vector3 _originalSpawnPos; // Lưu mốc gốc cực kỳ quan trọng

    void Start()
    {
        _board = Object.FindFirstObjectByType<Board>();
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
        foreach (Transform child in transform) Destroy(child.gameObject);
        foreach (BoxCollider2D col in GetComponents<BoxCollider2D>()) Destroy(col);

        int rows = shapeData.GetLength(0);
        int cols = shapeData.GetLength(1);

        // BƯỚC 1: TÌM KHUNG BAO (BOUNDING BOX) CỦA CÁC Ô CHỨA SỐ 1
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

        // BƯỚC 2: TÍNH TÂM THỰC SỰ CỦA KHỐI (Bỏ qua các hàng/cột số 0 thừa)
        float centerCol = (minCol + maxCol) / 2f;
        
        // Căn dọc: GIỮA TUYỆT ĐỐI (Dùng trung điểm để khối luôn nằm tâm điểm spawn)
        float centerRow = (minRow + maxRow) / 2f;

        // BƯỚC 3: SINH RA Ô VÀ CĂN GIỮA TUYỆT ĐỐI
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (shapeData[r, c] == 1)
                {
                    // Công thức mới ép khối gạch LUÔN mọc ra từ chính giữa tâm của nó
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
        transform.localScale = Vector3.one * scaleOnGrab; 
    }

    void Update()
    {
        if (_isDragging)
        {
            // DI CHUYỂN
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos + Vector3.up * offsetUp;
            UpdatePreview();

            // THẢ CHUỘT (Global check)
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                ClearPreview();

                if (TryPlace())
                {
                    gameObject.SetActive(false);
                    if (_board != null) _board.CheckAndClearLines();

                    Blocks manager = Object.FindFirstObjectByType<Blocks>();
                    if (manager != null) manager.CheckAndResetBlocks();
                }
                else
                {
                    // Nếu đặt hụt, trả về đúng gốc và thu nhỏ
                    transform.position = _originalSpawnPos;
                    transform.localScale = Vector3.one * _defaultScale;
                }
            }
        }
    }

private bool GetGridPos(Transform child, out int r, out int c)
    {
        r = -1; c = -1;
        if (_board == null) return false;

        Cell originCell = _board.GetCellAt(0, 0);
        if (originCell == null) return false;

        // 1. Chỉ đo khoảng cách từ tâm viên gạch tới mốc (0,0) của bàn cờ
        Vector3 parentPos = transform.position;
        Vector3 parentDiff = parentPos - originCell.transform.position;

        // 2. BÍ QUYẾT TẠI ĐÂY: Dùng child.localPosition (Tọa độ thiết kế ban đầu)
        // Thay vì dùng tọa độ thế giới đang bị co giãn, ta cộng thẳng tọa độ gốc của ô con.
        // Điều này ép các ô con LUÔN cách nhau đúng 1.0 đơn vị toán học.
        Vector3 logicalChildPos = parentDiff + child.localPosition;

        // 3. Khớp vào lưới chuẩn xác
        c = Mathf.FloorToInt(logicalChildPos.x + 0.5f);
        r = Mathf.FloorToInt(logicalChildPos.y + 0.5f);

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
            // GỌI HIỆU ỨNG MỚI: Tự động highlight cả hàng/cột sắp đầy "như ảnh"
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