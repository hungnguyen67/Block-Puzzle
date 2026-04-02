using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip placeBlockSound;
    public AudioClip clearLineSound;

    [Header("UI Buttons - Icons")]
    public Image musicBtnImage;
    public Image soundBtnImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private bool isMusicOn = true;
    private bool isSoundOn = true;

    private void Awake()
    {
        // Singleton pattern để dùng ở mọi Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tải cài đặt từ người chơi (mặc định là bật - 1)
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        UpdateMusicState();
        UpdateSoundUI();
    }

    // --- QUẢN LÝ NHẠC NỀN ---
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0); // SỬA: Phải là 1 : 0
        UpdateMusicState();
        PlaySFX(clickSound); // Kèm tiếng click khi bấm nút
    }

    private void UpdateMusicState()
    {
        if (isMusicOn)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }
        else
        {
            musicSource.Stop();
        }

        RefreshUI(); // Tự động cập nhật hình ảnh nút
    }

    // --- HỆ THỐNG TỰ ĐỘNG ĐĂNG KÝ (Sử dụng với AudioButtonHelper) ---
    public void RegisterMusicButton(Image newImg)
    {
        musicBtnImage = newImg;
        if (musicBtnImage != null)
            musicBtnImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
    }

    public void RegisterSoundButton(Image newImg)
    {
        soundBtnImage = newImg;
        if (soundBtnImage != null)
            soundBtnImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    public void RefreshUI()
    {
        if (musicBtnImage != null)
            musicBtnImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
            
        if (soundBtnImage != null)
            soundBtnImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    // MỚI: Hàm để gán trực tiếp cho mọi nút bấm bình thường trong game
    public void PlayClick()
    {
        PlaySFX(clickSound);
    }

    // --- QUẢN LÝ HIỆU ỨNG (SFX) ---
    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        UpdateSoundUI();
        PlaySFX(clickSound);
    }

    private void UpdateSoundUI()
    {
        if (soundBtnImage != null)
            soundBtnImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (isSoundOn && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
