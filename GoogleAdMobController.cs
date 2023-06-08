using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GoogleAdMobController : MonoBehaviour
{
    public string app_id = "ca-app-pub-3940256099942544~3347511713";
    public string adUnitId_Banner = "";
    public string adUnitId_INS = "";
    public string adUnitId_VIDEO = "";
    
    
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    //private RewardedInterstitialAd rewardedInterstitialAd;
    private float deltaTime;

    private UnityEvent OnAdLoadedEvent;
    private UnityEvent OnAdFailedToLoadEvent;
    private UnityEvent OnAdOpeningEvent;
    private UnityEvent OnAdFailedToShowEvent;
    private UnityEvent OnUserEarnedRewardEvent;
    private UnityEvent OnAdClosedEvent;

    private bool rewardvideoloaded;
    public delegate void OnVideoSuccessReward();
    public static OnVideoSuccessReward onVideoSuccessReward;


    public static GoogleAdMobController instance;

    //=== Test ID....
    /*
     * app id = ca-app-pub-3940256099942544~3347511713
     * 
     * banner = ca-app-pub-3940256099942544/6300978111
     * INS    = ca-app-pub-3940256099942544/1033173712
     * RWD    = ca-app-pub-3940256099942544/5224354917
     */
    //--------
   
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
   

    #region UNITY MONOBEHAVIOR METHODS

    public void start()
    {
        
        MobileAds.SetiOSAppPauseOnBackground(true);

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
            .SetTestDeviceIds(deviceIds).build();

        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);

        RequestAndLoadInterstitialAd();
        RequestAndLoadRewardedAd();
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
           // statusText.text = "Initialization complete";
            //RequestBannerAd();
        });
    }

    private void Update()
    {

    }

    #endregion

    #region HELPER METHODS

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();
    }

    #endregion

    #region BANNER ADS

    public void RequestBannerAd()
    {
        // Clean up banner before reusing
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        bannerView = new BannerView(adUnitId_Banner, AdSize.Banner, AdPosition.Bottom);

        // Add Event Handlers
//        bannerView.OnAdLoaded += (sender, args) => OnAdLoadedEvent.Invoke();
//        bannerView.OnAdFailedToLoad += (sender, args) => OnAdFailedToLoadEvent.Invoke();
//        bannerView.OnAdOpening += (sender, args) => OnAdOpeningEvent.Invoke();
//        bannerView.OnAdClosed += (sender, args) => OnAdClosedEvent.Invoke();

        // Load a banner ad
        bannerView.LoadAd(CreateAdRequest());
    }

    public void DestroyBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }
    }
    
    public void Show_Banner_ADS()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
        else
        {
            RequestBannerAd();
        }
    }
    
    public void HideBanner_ADS()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }
    

    #endregion

    #region INTERSTITIAL ADS

    public void RequestAndLoadInterstitialAd()
    {
        // Clean up interstitial before using it
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        interstitialAd = new InterstitialAd(adUnitId_INS);

        // Add Event Handlers
//        interstitialAd.OnAdLoaded += (sender, args) => OnAdLoadedEvent.Invoke();
//        interstitialAd.OnAdFailedToLoad += (sender, args) => OnAdFailedToLoadEvent.Invoke();
//        interstitialAd.OnAdOpening += (sender, args) => OnAdOpeningEvent.Invoke();
        interstitialAd.OnAdClosed += (sender, args) => Load_AD_IF_Not_Loaded(); //OnAdClosedEvent.Invoke();

        // Load an interstitial ad
        interstitialAd.LoadAd(CreateAdRequest());
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd.IsLoaded())
        {
            interstitialAd.Show();
           
        }
        else
        {
           // statusText.text = "Interstitial ad is not ready yet";
            RequestAndLoadInterstitialAd();
        }
    }
    
    //=========== Loading Dialog Display Then Display ads ======
   
    
        
   
    //=======================================
    

    public void DestroyInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }
    }
    #endregion

    #region REWARDED ADS

    public void RequestAndLoadRewardedAd()
    {
       
        // create new rewarded ad instance
        rewardedAd = new RewardedAd(adUnitId_VIDEO);

        // Add Event Handlers
        rewardedAd.OnAdLoaded += (sender, args) => Rewardads_load_done();
        rewardedAd.OnAdFailedToLoad += (sender, args) => Rewardads_Fail_load();
    //    rewardedAd.OnAdOpening += (sender, args) => OnAdOpeningEvent.Invoke();
    //    rewardedAd.OnAdFailedToShow += (sender, args) => OnAdFailedToShowEvent.Invoke();
        rewardedAd.OnAdClosed += (sender, args) => Load_AD_IF_Not_Loaded(); //OnAdClosedEvent.Invoke();
        rewardedAd.OnUserEarnedReward += (sender, args) => Rewardcomplet_Earn_Bonus(); //OnUserEarnedRewardEvent.Invoke();

        // Create empty ad request
        rewardedAd.LoadAd(CreateAdRequest());
    }
    
    void Rewardads_load_done()
    {
        rewardvideoloaded = true;
    }

    void Rewardads_Fail_load()
    {
        rewardvideoloaded = false;
    }

    public void ShowRewardedAd()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowAndroidToastMessage("Connect to internet");
        }
        else
        {
            if (rewardedAd != null && rewardvideoloaded)
            {
                rewardvideoloaded = false;
                rewardedAd.Show();
            }
            else
            {
                //  statusText.text = "Rewarded ad is not ready yet.";
                RequestAndLoadRewardedAd();
                
                ShowAndroidToastMessage("Video Ad Not Available");
            }
        }
    }
    
    
   
    #endregion
    
    
    public void Rewardcomplet_Earn_Bonus()
    {
        //=== Here add user reward function.... 
        if (onVideoSuccessReward != null)
        {
            onVideoSuccessReward();
        }
    }

    public void Load_AD_IF_Not_Loaded()
    {
        //===== Load ad check ..... 
        
        if (interstitialAd.IsLoaded())
        {
            
        }
        else
        {
            RequestAndLoadInterstitialAd();
        }
        
        
        if (rewardedAd != null && rewardvideoloaded)
        {
           
        }
        else
        {
            RequestAndLoadRewardedAd();
        }
    }
    public void Demo()
    {
        GoogleAdMobController.onVideoSuccessReward = null;
        GoogleAdMobController.onVideoSuccessReward = OnSuccess;
        GoogleAdMobController.instance.ShowRewardedAd();
    }
    void OnSuccess()
    {
        
    }
    
    public void ShowAndroidToastMessage(string message)
    {
#if !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
#endif
    }
    
}
