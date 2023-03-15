using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManagerUnity : AdsManager, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener {
    public static AdsManagerUnity instance;

    private string appleGameID = "4089104";
    private string googleGameID = "4089105";
    private string rewardedVideoID = "rewardedVideo";

    private bool popup = false;
    private Callback callback;
    private bool success = false;
    private bool rewardedVideoDidClose = false;

    private string interstitialID = "video";
    private bool interstitialDidClose = false;
    private Callback interstitialCallback;

    private string bannerID = "banner";
    private List<string> listLoaded = new List<string>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Init();
    }

    public void Init() {
        string gameID;
#if UNITY_EDITOR || UNITY_ANDROID
        gameID = googleGameID;
#elif UNITY_IOS
        gameID = appleGameID;
#endif

        bool testMode = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        testMode = true;
#endif
        Advertisement.Initialize(gameID, testMode, this);
    }

    public override void ShowRewardedAd(bool popup, Callback callback) {
        this.popup = popup;

        Debug.Log("AdsManagerUnity::ShowRewardedAd");
        
        this.callback = callback;
        StartCoroutine(CheckAds());
    }

    private IEnumerator CheckAds() {
        success = false;    
        rewardedVideoDidClose = false;
        
        Debug.Log("AdsManagerUnity::CheckAds");

        
        NetworkLoading.instance.Show();
        yield return new WaitUntil(() => listLoaded.Contains(rewardedVideoID));
        NetworkLoading.instance.Hide();

        Advertisement.Show(rewardedVideoID);
        Debug.Log("AdsManagerUnity::CheckAds::rewardedVideoAd.Show()");
        yield return new WaitUntil(() => rewardedVideoDidClose == true);

        if (success) {
            WebAds.instance.ReqIncreaseAdsView(popup, callback);
            Debug.Log("AdsManagerUnity::success");
        }
        else
            callback = null;
    }

    public void OnUnityAdsDidError(string message) {
        Debug.Log("OnUnityAdsDidError::" + message);
    }

    public override bool IsRewardedVideoLoaded() {
#if UNITY_EDITOR
        return true;
#else
        return listLoaded.Contains(rewardedVideoID);
#endif
    }

    private void OnDestroy() {
        //Advertisement.RemoveListener(this);
    }

    public override void ShowInterstitial(Callback callback) {
        interstitialCallback = callback;

        StartCoroutine(CheckInterstitial());
    }

    private IEnumerator CheckInterstitial() {
        interstitialDidClose = false;

        if (IsInterstitialLoaded()) {
            Advertisement.Show(interstitialID);
            Debug.Log("AdsManagerUnity::CheckAds::interstitial.Show()");
            yield return new WaitUntil(() => interstitialDidClose == true);

            interstitialCallback();
            interstitialCallback = null;
            Debug.Log("AdsManagerUnity::CheckAds::interstitialClosed");
        }
        else {
            interstitialCallback();
            interstitialCallback = null;
        }
    }

    public override bool IsInterstitialLoaded() {
#if UNITY_EDITOR
        return true;
#else
        return listLoaded.Contains(interstitialID);
#endif
    }

    public override bool IsBannerLoaded() {
#if UNITY_EDITOR
        return true;
#else
        return listLoaded.Contains(bannerID);
#endif
    }

    public override void ShowBanner() {
        if (UserDataModel.instance.IsAdsRemoved())
            return;

        Advertisement.Banner.SetPosition (BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(bannerID);
    }

    public override void HideBanner() {
        if (IsBannerLoaded() == false)
            return;
        Advertisement.Banner.Hide();
    }

    public void OnInitializationComplete() {
        Debug.Log($"OnInitializationComplete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) {
        listLoaded.Clear();
        Debug.Log($"OnInitializationFailed::{message}");
    }

    public void OnUnityAdsAdLoaded(string placementId) {
        if (listLoaded.Contains(placementId) == false)
            listLoaded.Add(placementId);
        Debug.Log($"OnUnityAdsAdLoaded::{placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) {
        if (listLoaded.Contains(placementId))
            listLoaded.Remove(placementId);
        Debug.Log($"OnUnityAdsFailedToLoad::{placementId}::{message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) {
        Debug.Log($"OnUnityAdsShowFailure::{placementId}::{message}");
    }

    public void OnUnityAdsShowStart(string placementId) {
        Debug.Log($"OnUnityAdsShowStart::{placementId}");
    }

    public void OnUnityAdsShowClick(string placementId) {
        Debug.Log($"OnUnityAdsShowClick::{placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState) {
        if (placementId == rewardedVideoID)
            rewardedVideoDidClose = true;
        else
            interstitialDidClose = true;

        HandleShowResult(placementId, showCompletionState);
    }

    /*
    SKIPPED	
    A state that indicates that the user skipped the ad.
    0

    COMPLETED	
    A state that indicates that the ad was played entirely.
    1

    UNKNOWN	
    Default value / Used when no mapping available
    2
    */
    private void HandleShowResult(string placementID, UnityAdsShowCompletionState state) {
        switch ((int)state) {
            case 1: //COMPLETED
                Debug.Log("The ad was successfully shown.");
                if (placementID == rewardedVideoID)
                    success = true;
                break;

            case 0: //SKIPPED
                Debug.Log("The ad was skipped before reaching the end.");
                break;

            case 2: //UNKNOWN
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
}
