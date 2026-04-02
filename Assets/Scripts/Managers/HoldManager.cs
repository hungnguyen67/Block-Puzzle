using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HoldManager : MonoBehaviour
{
    public static HoldManager Instance;

    [Header("UI")]
    public RectTransform holdPanel; 
    public RectTransform holdPreviewRoot;
    public Button holdButton; // Vẫn giữ để nếu user muốn dùng cả 2

    // Kiểm tra xem vị trí chuột/vị trí khối có nằm trong vùng HoldPanel không
    public bool IsOverHoldPanel(Vector2 screenPoint)
    {
        if (holdPanel == null) return false;
        
        // Tự động lấy Camera từ Canvas nếu có
        Canvas canvas = holdPanel.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        bool isOver = RectTransformUtility.RectangleContainsScreenPoint(holdPanel, screenPoint, cam);
        
        return isOver;
    }
    public Blocks blockSpawner; 

    private int heldShapeIndex = -1; 
    private GameObject heldPreviewObj; 

    private Block selectedBlock; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (holdButton != null)
            holdButton.onClick.AddListener(() => HoldSelectedBlock());
    }

    public void SetSelectedBlock(Block block)
    {
        selectedBlock = block;
        // Có thể thêm hiệu ứng highlight block đang chọn ở đây
    }

    public bool HoldSelectedBlock()
    {
        if (selectedBlock == null) return false;

        int currentShapeIndex = selectedBlock.shapeIndex;
        int currentSlotIndex = selectedBlock.slotIndex;

        if (heldShapeIndex == -1)
        {
            // Trường hợp 1: Hold đang trống
            heldShapeIndex = currentShapeIndex;
            UpdateHoldPreview();

            // MỚI: Xóa block khỏi slot quản lý thực sự (để trống slot đó)
            if (blockSpawner != null)
                blockSpawner.ClearSlot(currentSlotIndex);

            // Xóa block đang chọn trên tay
            Destroy(selectedBlock.gameObject);
        }
        else
        {
            // Trường hợp 2: Đổi block đang giữ với block đang chọn
            int temp = heldShapeIndex;
            heldShapeIndex = currentShapeIndex;
            UpdateHoldPreview();

            // QUAN TRỌNG: Xóa block đang chọn trên tay người chơi
            Destroy(selectedBlock.gameObject);

            // Spawn block từ Hold vào slot của block đang chọn
            if (blockSpawner != null)
                blockSpawner.SpawnSpecificBlock(temp, currentSlotIndex);
        }

        selectedBlock = null;
        if (holdButton != null) holdButton.interactable = true; // Luôn mở nút

        return true;
    }

    public void ClearHeldBlockAfterDrag()
    {
        heldShapeIndex = -1;
        heldPreviewObj = null;
        // KHÔNG gọi UpdateHoldPreview() vì nó sẽ xóa vật thể đang bay về
    }

    public void ReRegisterHeldBlock(Block block)
    {
        heldShapeIndex = block.shapeIndex;
        heldPreviewObj = block.gameObject;
    }

    public Block GetHeldBlock()
    {
        if (heldPreviewObj == null) return null;
        return heldPreviewObj.GetComponent<Block>();
    }

    public void ResetHoldTurn()
    {
        if (holdButton != null) holdButton.interactable = true;
    }

    private void UpdateHoldPreview()
    {
        if (heldPreviewObj != null)
            Destroy(heldPreviewObj);

        if (heldShapeIndex != -1 && blockSpawner != null)
        {
            // Tạo khối, đặt ngay vào trong thẻ con của Blocks để dễ quản lý (Hierarchy)
            GameObject freshBlock = Instantiate(blockSpawner.blockPrefab, holdPreviewRoot.position, Quaternion.identity, blockSpawner.transform);
            heldPreviewObj = freshBlock;
            
            // Đảm bảo khối nằm ở độ sâu hợp lý (Z âm nhẹ) để không bị che khuất
            freshBlock.transform.position = new Vector3(freshBlock.transform.position.x, freshBlock.transform.position.y, -2f);
            
            // Chỉnh scale cho khớp ô (Sửa scale thành 0.6f nếu thấy nhỏ quá)
            freshBlock.transform.localScale = Vector3.one * 0.6f;

            // QUAN TRỌNG: Khối trong Hold sẽ ở trạng thái sẵn sàng để kéo
            Block script = freshBlock.GetComponent<Block>();
            if (script != null)
            {
                script.shapeIndex = heldShapeIndex;
                script.slotIndex = -99; // Một mã để biết đây là khối ô Hold
                script.Initialize(Poliominus.Shapes[heldShapeIndex]);
                
                // MỚI: Thiết lập vị trí quay về mặc định là chính vị trí nó vừa xuất hiện
                script.SetDefaultScale(0.6f, freshBlock.transform.position);

                // MỚI: Ép Sorting Order cao để không bị che bởi Board/Background
                SpriteRenderer[] renderers = script.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in renderers) sr.sortingOrder = 50; 
            }
        }
    }
}
