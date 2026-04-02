using UnityEngine;
using UnityEngine.UI;

public class AudioButtonHelper : MonoBehaviour
{
    public bool isMusicButton = true; // Nếu là nút Music thì tick, nếu nút Sound thì bỏ tick
    private Image myImage;

    void Awake()
    {
        myImage = GetComponent<Image>();
    }

    void Start()
    {
        // Tự động gán lệnh Click cho nút bấm ngay khi bắt đầu
        Button myBtn = GetComponent<Button>();
        if (myBtn != null)
        {
            myBtn.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            if (isMusicButton)
                AudioManager.Instance.ToggleMusic();
            else
                AudioManager.Instance.ToggleSound();
        }
    }

    void OnEnable()
    {
        // Tự động "giơ tay" nói với AudioManager khi Scene vừa bật
        if (AudioManager.Instance != null && myImage != null)
        {
            if (isMusicButton)
                AudioManager.Instance.RegisterMusicButton(myImage);
            else
                AudioManager.Instance.RegisterSoundButton(myImage);
        }
    }
}
