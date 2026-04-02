using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightSprite;
    
    private SpriteRenderer spriteRenderer;
    public bool isFilled = false;

    public enum CellState { Normal, Hover, Highlight }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetState(CellState.Normal);
    }

    public void SetState(CellState state)
    {
        if (isFilled) return;

        switch (state)
        {
            case CellState.Normal:
                spriteRenderer.sprite = normalSprite;
                break;
            case CellState.Hover:
            case CellState.Highlight:
                spriteRenderer.sprite = highlightSprite;
                break;
        }
    }

    public void SetFilled(bool filled)
    {
        isFilled = filled;
        spriteRenderer.sprite = normalSprite;
        transform.localScale = Vector3.one;

        Color c = spriteRenderer.color;
        c.a = filled ? 1f : 0f;
        spriteRenderer.color = c;
    }

    public void SetHighlightPreview(bool active, bool isPotentialClear)
    {
        if (!active)
        {
            spriteRenderer.sprite = normalSprite;
            Color color = spriteRenderer.color;
            color.a = isFilled ? 1f : 0f;
            spriteRenderer.color = color;
            return;
        }

        if (isPotentialClear)
        {
            spriteRenderer.sprite = highlightSprite;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        else
        {
            spriteRenderer.sprite = normalSprite;
            Color color = spriteRenderer.color;
            color.a = 0.4f;
            spriteRenderer.color = color;
        }
    }

    public void ClearWithAnimation()
    {
        isFilled = false;
        if (highlightSprite != null) spriteRenderer.sprite = highlightSprite;
        StartCoroutine(AnimateClear());
    }

    private System.Collections.IEnumerator AnimateClear()
    {
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float s = 1f;
            if (t < 0.2f) s = Mathf.Lerp(1f, 1.3f, t / 0.2f);
            else s = Mathf.Lerp(1.3f, 0f, (t - 0.2f) / 0.8f);
            
            transform.localScale = Vector3.one * s;
            yield return null;
        }

        transform.localScale = Vector3.one;
        spriteRenderer.sprite = normalSprite;
        Color finalColor = spriteRenderer.color;
        finalColor.a = 0f; 
        spriteRenderer.color = finalColor;
    }
}