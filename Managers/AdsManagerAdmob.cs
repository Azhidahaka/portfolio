using System;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;

public class AdsManagerAdmob : AdsManager {
    public static AdsManagerAdmob instance;

    private RewardedAd rewardedAd;
    private bool complete = true;

    private bool success = false;

    private Callback callback;
    private bool popup = false;

    //전면광고
    private InterstitialAd interstitial;
    private Callback interstitialCallback;

    private bool closed = true;

    private bool init = false;
    private bool reset = false;

    //배너
    private BannerView banner;
    private bool bannerLoaded = false;

    public void Awake() {
        instance = this;
    }

    public void Start() {
        Init();
    }

    private void Update() {
        if (reset) {
            reset = false;
            Init();
            return;
        }

        if (init == false)
            return;

        init = false;
        Dispatcher.Instance.InvokePending();
    }

    private void Init() {
        // iOS에서만 실행하도록 한다. 
#if UNITY_IOS && !UNITY_EDITOR 
        AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true); 
#endif

        init = false;
        MobileAds.Initialize(initStatus => {
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map) {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState) {
                    case AdapterState.NotReady:
                        print("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        print("Adapter: " + className + " is initialized.");
                        break;
                }
            }

            Dispatcher.AddAction(() => {
                Debug.Log("Complete");
                //RequestBanner();
                RequestRewardedAd();
                RequestInterstitial();
            });
            init = true;
        });
    }

    public void DetermineRequestBanner() {
        if (UserDataModel.instance.IsAdsRemoved())
            return;

        RequestBanner();
    }

    private void RequestBanner() {
        string adUnitId;
#if UNITY_EDITOR || ( UNITY_ANDROID && DEVELOPMENT_BUILD )
        adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IOS && DEVELOPMENT_BUILD
        adUnitId = "ca-app-pub-3940256099942544/2934735716";
#elif UNITY_ANDROID
        adUnitId = "ca-app-pub-2489295471754858/2057167952";    
#elif UNITY_IOS
        adUnitId = "ca-app-pub-2489295471754858/2264201455";
#else
        adUnitId = "unexpected_platform";
#endif

        if (banner != null)
            banner.Destroy();

        //AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

        banner = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        banner.OnAdLoaded += HandleBannerLoaded;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        banner.LoadAd(request);
    }

    public void HandleBannerLoaded(object sender, EventArgs args) {
        bannerLoaded = true;

        Debug.Log("HandleAdLoaded event received");
        Debug.Log(string.Format("Banner Height: {0}, width: {1}",
                            banner.GetHeightInPixels(),
                            banner.GetWidthInPixels()));
    }

    private void RequestInterstitial() {
        string adUnitId;
#if UNITY_EDITOR || ( UNITY_ANDROID && DEVELOPMENT_BUILD )
        adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS && DEVELOPMENT_BUILD
        adUnitId = "ca-app-pub-3940256099942544/4411468910";
#elif UNITY_ANDROID
        adUnitId = "ca-app-pub-2489295471754858/3462567207";    
#elif UNITY_IOS
        adUnitId = "ca-app-pub-2489295471754858/5804841273";
#else
        adUnitId = "unexpected_platform";
#endif

        interstitial = new InterstitialAd(adUnitId);
        // 전면광고
        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public void HandleOnAdLoaded(object sender, EventArgs args) {
        Debug.Log("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        Debug.Log("interstitial Failed : "
                            + args.LoadAdError.GetMessage());
    }

    public void HandleOnAdOpened(object sender, EventArgs args) {
        Debug.Log("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args) {
        Debug.Log("HandleAdClosed event received");
        RequestInterstitial();

        closed = true;
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args) {
        Debug.Log("HandleAdLeavingApplication event received");
    }

    public override void ShowInterstitial(Callback callback) {
        interstitialCallback = callback;

        StartCoroutine(CheckInterstitial());
    }

    private IEnumerator CheckInterstitial() {
        closed = false;

        if (interstitial.IsLoaded() == false) {
            RequestInterstitial();
            interstitialCallback();
            interstitialCallback = null;
        }
        else {
            interstitial.Show();
            yield return new WaitUntil(() => closed);
            //UserDataModel.instance.PlayedCount = 0;
            interstitialCallback();
            interstitialCallback = null;
        }
    }

    // 보상형 광고
    private void RequestRewardedAd() {
        string adUnitId;
#if UNITY_EDITOR || ( UNITY_ANDROID && DEVELOPMENT_BUILD )
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS && DEVELOPMENT_BUILD
        adUnitId = "ca-app-pub-3940256099942544/1712485313";
#elif UNITY_ANDROID
        adUnitId = "ca-app-pub-2489295471754858/9644832176";    
#elif UNITY_IOS
        adUnitId = "ca-app-pub-2489295471754858/4491759606";
#else
        adUnitId = "unexpected_platform";
#endif

        rewardedAd = new RewardedAd(adUnitId);

        // Called when an ad request has successfully loaded.
        rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);
    }

    private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs e) {
        print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + e.LoadAdError.GetMessage());
        complete = true;
        try {
            if (e.LoadAdError.GetCode() == 3 || e.LoadAdError.GetMessage().Contains("config"))
                reset = false;
            else
                reset = true;
        }
        catch(Exception exception) {
            Debug.Log($"HandleRewardedAdFailedToLoad::{exception.Message}");
            reset = false;
        }
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args) {
        print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args) {
        print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args) {
        print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.AdError.GetMessage());
        complete = true;
        try {
            if (args.AdError.GetCode() == 3 || args.AdError.GetMessage().Contains("config"))
                reset = false;
            else
                reset = true;
        }
        catch(Exception exception) {
            Debug.Log($"HandleRewardedAdFailedToLoad::{exception.Message}");
            reset = false;
        }
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args) {
        print("HandleRewardedAdClosed event received");
        RequestRewardedAd();
        complete = true;
    }

    public void HandleUserEarnedReward(object sender, Reward args) {
        string type = args.Type;
        double amount = args.Amount;
        print(
            "HandleUserEarnedReward event received for "
                        + amount.ToString() + " " + type);

        success = true;
    }

    public override void ShowRewardedAd(bool popup, Callback callback) {
        this.popup = popup;

        if (complete == false)
            return;

        this.callback = callback;
        StartCoroutine(CheckAds());
    }

    private IEnumerator CheckAds() {
        complete = false;
        success = false;

        if (rewardedAd.IsLoaded() == false) {
            NetworkLoading.instance.Show();
            RequestRewardedAd();
            yield return new WaitUntil(() => rewardedAd.IsLoaded());
            NetworkLoading.instance.Hide();
        }

        rewardedAd.Show();
        yield return new WaitUntil(() => complete == true);

        if (success)
            WebAds.instance.ReqIncreaseAdsView(popup, callback);
        else
            callback = null;
    }

    public override bool IsRewardedVideoLoaded() {
        if (rewardedAd == null || rewardedAd.IsLoaded() == false)
            return false;
        return true;
    }

    public bool IsInitComplete() {
        return complete;
    }

    public override bool IsInterstitialLoaded() {
        if (interstitial == null || interstitial.IsLoaded() == false)
            return false;
        return true;
    }

    public override bool IsBannerLoaded() {
        return bannerLoaded;
    }

    public override void ShowBanner() {
        if (banner == null) {
            DetermineRequestBanner();
            return;
        }

        banner.Show();
    }

    public override void HideBanner() {
        if (banner == null || IsBannerLoaded() == false)
            return;
        banner.Hide();
    }
}