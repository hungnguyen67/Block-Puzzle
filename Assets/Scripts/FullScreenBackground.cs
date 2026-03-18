using UnityEngine;

public class FullScreenBackground : MonoBehaviour
{
    void Start()
    {
        ScaleToFitScreen();
    }

    void ScaleToFitScreen()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 1. Đưa background về gốc tòa độ và reset scale
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        transform.localScale = Vector3.one;

        // 2. Tính toán kích thước màn hình trong world space
        float screenHeight = Camera.main.orthographicSize * 2.0f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        // 3. Tính toán kích thước của Sprite hiện tại
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // 4. Tính toán tỷ lệ Scale cần thiết để phủ kín (Cover)
        float scaleX = screenWidth / spriteWidth;
        float scaleY = screenHeight / spriteHeight;

        // Chọn tỷ lệ lớn hơn để đảm bảo không bị khoảng trống (giống chế độ 'Cover')
        float finalScale = Mathf.Max(scaleX, scaleY);
        
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    // Tự động cập nhật nếu bạn thay đổi kích thước cửa sổ Game trong lúc phát triển
    #if UNITY_EDITOR
    void Update()
    {
        ScaleToFitScreen();
    }
    #endif
}
