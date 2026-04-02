using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class SingleAdController : MonoBehaviour
{
    // Test Ad Unit ID for Rewarded Interstitial (Android)
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5354046379"; 
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _adUnitId = "unused";
#endif

    private RewardedInterstitialAd _rewardedInterstitialAd;
    public static SingleAdController Instance { get; private set; }

    private Action _onRewardEarned; // To store the reward action until ad is closed

    private void Awake()
    {
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

    void Start()
    {
        MobileAds.Initialize(status => {
            LoadAd(); 
        });
    }

    public void LoadAd()
    {
        if (_adUnitId == "unused") return;

        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }

        var adRequest = new AdRequest();

        RewardedInterstitialAd.Load(_adUnitId, adRequest, (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                return;
            }
            _rewardedInterstitialAd = ad;
        });
    }

    public void ShowAdWithCallback(Action onReward)
    {
        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _onRewardEarned = null; // Clear previous

            // This only marks the reward as earned
            _rewardedInterstitialAd.Show(reward =>
            {
                _onRewardEarned = onReward;
            });

            // This triggers when the ad is DONE (user clicked X)
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += HandleAdClosed;
        }
        else
        {
            LoadAd();
        }
    }

    private void HandleAdClosed()
    {
        // Unregister the event
        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.OnAdFullScreenContentClosed -= HandleAdClosed;
        }

        // Only give reward if earned during show
        if (_onRewardEarned != null)
        {
            _onRewardEarned.Invoke();
            _onRewardEarned = null;
        }

        LoadAd(); 
    }
}
