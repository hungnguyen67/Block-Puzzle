using UnityEngine;
using System.Collections.Generic;

public class Blocks : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab; 
    [SerializeField] private Transform[] spawnPoints; 
    [SerializeField] private float spawnScale = 0.6f; // Kích thước nhỏ mặc định

    private List<GameObject> _currentBlocks = new List<GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Kiểm tra khoảng cách chuột tới 3 vị trí đặt khối
            Vector3[] fixedPositions = new Vector3[] {
                new Vector3(1.0f, 0.0f, 0),
                new Vector3(4.0f, 0.0f, 0),
                new Vector3(7.0f, 0.0f, 0)
            };

            for (int i = 0; i < _currentBlocks.Count; i++)
            {
                if (_currentBlocks[i] != null && _currentBlocks[i].activeSelf)
                {
                    float dist = Vector3.Distance(mousePos, fixedPositions[i]);
                    // Nếu bấm trong vùng 2.0 đơn vị quanh khối thì cho phép kéo
                    if (dist < 2.0f) 
                    {
                        _currentBlocks[i].GetComponent<Block>().StartDragging();
                        break;
                    }
                }
            }
        }
    }

    void Start() { SpawnNewBlocks(); }

    public void SpawnNewBlocks()
    {
        foreach (var b in _currentBlocks) if (b != null) Destroy(b);
        _currentBlocks.Clear();

        // VỊ TRÍ CỐ ĐỊNH CỤ THỂ CHO 3 KHỐI:
        // Bạn có thể chỉnh lại X (-2.6, 0, 2.6) và Y (-6.0) cho vừa với màn hình của mình
        Vector3[] fixedPositions = new Vector3[] {
            new Vector3(1.0f, 0.0f, 0), // Khối trái
            new Vector3(4.0f, 0.0f, 0),  // Khối giữa
            new Vector3(7.0f, 0.0f, 0)   // Khối phải
        };

        for (int i = 0; i < 3; i++)
        {
            Vector3 targetPos = fixedPositions[i];

            GameObject newBlockGO = Instantiate(blockPrefab, targetPos, Quaternion.identity, transform);
            
            Block blockScript = newBlockGO.GetComponent<Block>();
            if (blockScript != null)
            {
                int randomIndex = Random.Range(0, Poliominus.Shapes.Length);
                blockScript.Initialize(Poliominus.Shapes[randomIndex]);
                
                // Gán vị trí cố định và tỷ lệ thu nhỏ lúc nằm trong khay
                blockScript.SetDefaultScale(spawnScale, targetPos);
            }
            _currentBlocks.Add(newBlockGO);
        }
    }

    public void CheckAndResetBlocks()
    {
        bool allUsed = true;
        foreach (var b in _currentBlocks)
        {
            if (b != null && b.activeSelf) { allUsed = false; break; }
        }
        
        if (allUsed) 
        {
            SpawnNewBlocks();
            CheckGameOver();
        }
        else
        {
            // Kiểm tra xem còn nước đi nào không (Game Over check)
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        Board board = Object.FindFirstObjectByType<Board>();
        if (board == null) return;

        bool anyBlockCanFit = false;
        foreach (var blockGO in _currentBlocks)
        {
            if (blockGO != null && blockGO.activeSelf)
            {
                Block blockScript = blockGO.GetComponent<Block>();
                if (blockScript != null && board.CanFit(blockScript.ShapeData))
                {
                    anyBlockCanFit = true;
                    break;
                }
            }
        }

        if (!anyBlockCanFit)
        {
            Debug.LogError("GAME OVER! Không còn chỗ để đặt gạch.");
            // Ở đây bạn có thể hiện UI thông báo Game Over
            // Ví dụ: CanvasManager.Instance.ShowGameOver();
        }
    }
}