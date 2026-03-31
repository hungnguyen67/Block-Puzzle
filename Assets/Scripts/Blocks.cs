using UnityEngine;
using System.Collections.Generic;

public class Blocks : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnScale = 0.8f;

    private List<GameObject> _currentBlocks = new List<GameObject>();

    void Start()
    {
        SpawnNewBlocks();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            for (int i = 0; i < _currentBlocks.Count; i++)
            {
                if (_currentBlocks[i] != null && _currentBlocks[i].activeSelf)
                {
                    Vector3 targetPos = spawnPoints[i].position;

                    float dist = Vector3.Distance(mousePos, targetPos);

                    if (dist < 2.0f)
                    {
                        _currentBlocks[i].GetComponent<Block>().StartDragging();
                        break;
                    }
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
                blockScript.Initialize(Poliominus.Shapes[randomIndex]);

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

        CheckGameOver();
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