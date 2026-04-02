using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Blocks : MonoBehaviour
{
    public GameObject blockPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnScale = 0.8f;

    private List<GameObject> _currentBlocks = new List<GameObject>();

    void Start()
    {
        SpawnNewBlocks();
    }

    void Update()
    {
        // QUAN TRỌNG: Nếu game đang dừng (paused), không cho phép tương tác với khối
        if (Time.timeScale == 0) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            mousePos.z = 0;

            // Kiểm tra 3 slot chính
            for (int i = 0; i < _currentBlocks.Count; i++)
            {
                if (_currentBlocks[i] != null && _currentBlocks[i].activeSelf)
                {
                    Block script = _currentBlocks[i].GetComponent<Block>();
                    // Dùng Vector2 để bỏ qua trục Z, khoảng cách 2.0 đủ bao phủ khối
                    if (script != null && Vector2.Distance(mousePos, _currentBlocks[i].transform.position) < 2.5f)
                    {
                        script.StartDragging();
                        if (HoldManager.Instance != null) HoldManager.Instance.SetSelectedBlock(script);
                        return;
                    }
                }
            }

            // Kiểm tra ô HOLD
            if (HoldManager.Instance != null && HoldManager.Instance.GetHeldBlock() != null)
            {
                Block heldBlock = HoldManager.Instance.GetHeldBlock();
                if (Vector2.Distance(mousePos, heldBlock.transform.position) < 2.0f)
                {
                    heldBlock.StartDragging();
                    HoldManager.Instance.SetSelectedBlock(heldBlock);
                    HoldManager.Instance.ClearHeldBlockAfterDrag();
                    return;
                }
            }
        }
    }

    public void SpawnNewBlocks()
    {
        foreach (var b in _currentBlocks)
        {
            if (b != null) Destroy(b);
        }
        _currentBlocks.Clear();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 targetPos = spawnPoints[i].position;

            GameObject newBlockGO = Instantiate(blockPrefab, targetPos, Quaternion.identity, transform);

            Block blockScript = newBlockGO.GetComponent<Block>();
            if (blockScript != null)
            {
                int randomIndex = Random.Range(0, Poliominus.Shapes.Length);
                blockScript.shapeIndex = randomIndex;
                blockScript.slotIndex = i;
                blockScript.Initialize(Poliominus.Shapes[randomIndex]);

                blockScript.SetDefaultScale(spawnScale, targetPos);
            }

            _currentBlocks.Add(newBlockGO);
        }
    }

    public void SpawnSpecificBlock(int sIndex, int slotIdx)
    {
        if (slotIdx >= 0 && slotIdx < spawnPoints.Length)
        {
            if (_currentBlocks[slotIdx] != null) Destroy(_currentBlocks[slotIdx]);

            Vector3 targetPos = spawnPoints[slotIdx].position;
            GameObject newBlockGO = Instantiate(blockPrefab, targetPos, Quaternion.identity, transform);
            _currentBlocks[slotIdx] = newBlockGO;

            Block blockScript = newBlockGO.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.shapeIndex = sIndex;
                blockScript.slotIndex = slotIdx;
                blockScript.Initialize(Poliominus.Shapes[sIndex]);
                blockScript.SetDefaultScale(spawnScale, targetPos);
            }
        }
    }

    public void SpawnRandomAtSlot(int slotIdx)
    {
        if (slotIdx >= 0 && slotIdx < spawnPoints.Length)
        {
            if (_currentBlocks[slotIdx] != null) Destroy(_currentBlocks[slotIdx]);

            Vector3 targetPos = spawnPoints[slotIdx].position;
            GameObject newBlockGO = Instantiate(blockPrefab, targetPos, Quaternion.identity, transform);
            _currentBlocks[slotIdx] = newBlockGO;

            Block blockScript = newBlockGO.GetComponent<Block>();
            if (blockScript != null)
            {
                int randomIndex = Random.Range(0, Poliominus.Shapes.Length);
                blockScript.shapeIndex = randomIndex;
                blockScript.slotIndex = slotIdx;
                blockScript.Initialize(Poliominus.Shapes[randomIndex]);
                blockScript.SetDefaultScale(spawnScale, targetPos);
            }
        }
    }

    public void ClearSlot(int slotIdx)
    {
        if (slotIdx >= 0 && slotIdx < _currentBlocks.Count)
        {
            _currentBlocks[slotIdx] = null;
        }
    }

    public void CheckAndResetBlocks()
    {
        bool allUsed = true;

        foreach (var b in _currentBlocks)
        {
            if (b != null && b.activeSelf)
            {
                allUsed = false;
                break;
            }
        }

        if (allUsed)
        {
            SpawnNewBlocks();
        }

        RefreshAllBlocksTransparency(); // MỚI: Luôn làm mới độ mờ sau mỗi lượt
        CheckGameOver();
    }

    public void RefreshAllBlocksTransparency()
    {
        Board board = Object.FindFirstObjectByType<Board>();
        if (board == null) return;

        // Kiểm tra 3 slot chính
        foreach (var b in _currentBlocks)
        {
            if (b != null && b.activeSelf)
            {
                Block script = b.GetComponent<Block>();
                if (script != null)
                {
                    bool canFit = board.CanFit(script.ShapeData);
                    script.SetTransparency(!canFit);
                }
            }
        }

        // Kiểm tra nốt khối trong ô HOLD
        if (HoldManager.Instance != null && HoldManager.Instance.GetHeldBlock() != null)
        {
            Block heldBlock = HoldManager.Instance.GetHeldBlock();
            if (heldBlock != null)
            {
                bool canFit = board.CanFit(heldBlock.ShapeData);
                heldBlock.SetTransparency(!canFit);
            }
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

        // MỚI: Logic Cứu thua tinh tế
        if (!anyBlockCanFit)
        {
            if (HoldManager.Instance != null)
            {
                // Đếm số khối còn lại trong các slot
                int remainingBlocksCount = 0;
                foreach (var b in _currentBlocks) if (b != null && b.activeSelf) remainingBlocksCount++;

                // TRƯỜNG HỢP 1: Ô Hold đang trống
                if (HoldManager.Instance.GetHeldBlock() == null)
                {
                    // Chỉ cứu thua nếu trên bàn CÒN DUY NHẤT 1 KHỐI (vì cất khối này đi sẽ gọi wave mới)
                    // Nếu còn 2 hay 3 khối mà đều không vừa -> Thua luôn cho đỡ mất thời gian của người chơi
                    if (remainingBlocksCount == 1)
                    {
                        anyBlockCanFit = true;
                    }
                }
                // TRƯỜNG HỢP 2: Ô Hold đang có khối
                else
                {
                    // Kiểm tra xem khối trong Hold có đặt được vào bàn không
                    Block heldBlock = HoldManager.Instance.GetHeldBlock();
                    if (heldBlock != null && board.CanFit(heldBlock.ShapeData))
                    {
                        anyBlockCanFit = true;
                    }
                }
            }
        }

        if (!anyBlockCanFit)
        {
            if (GameplayUIManager.Instance != null)
            {
                int finalScore = 0;
                if (board != null)
                {
                    finalScore = board.GetCurrentScore();
                }
                
                GameplayUIManager.Instance.ShowGameOver(finalScore);
            }
        }
    }
}