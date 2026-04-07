using UnityEngine;
using TMPro;

public class FloatingScore : MonoBehaviour
{
    private TextMeshPro _textMesh;
    private float _timer = 0f;
    private float _duration = 1.0f;
    private Vector3 _moveDir = new Vector3(0, 1, 0);
    private float _moveSpeed = 1.5f;

    void Awake()
    {
        _textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int score)
    {
        if (_textMesh != null)
        {
            _textMesh.text = "+" + score.ToString();
            _textMesh.sortingOrder = 1000; 
        }
        
        _moveDir.x = Random.Range(-0.5f, 0.5f);
        
        Destroy(gameObject, _duration);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        float ratio = _timer / _duration;

        // Move up
        transform.position += _moveDir * _moveSpeed * Time.deltaTime;

        // Fade out
        if (_textMesh != null)
        {
            Color c = _textMesh.color;
            c.a = 1f - ratio;
            _textMesh.color = c;
        }

        // Scale effect
        transform.localScale = Vector3.one * (1f + ratio * 0.5f);
    }
}
