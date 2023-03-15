using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour {
    public static ADS_TYPE adsType = ADS_TYPE.ADMOB;
    public static AdsManager GetLoadedInstance(ADS_PLACEMENT adsPlacement) {
        if (adsPlacement == ADS_PLACEMENT.BANNER || 
            UserDataModel.instance.DailyAdsViewCount <= Constant.DAILY_ADS_VIEW_MAX) 
            return GetInstanceAdmob(adsPlacement);

        return GetInstanceUnity(adsPlacement);   
    }

    private static AdsManager GetInstanceAdmob(ADS_PLACEMENT adsPlacement) {
        if (adsPlacement == ADS_PLACEMENT.REWARDED_VIDEO) {
            if (AdsManagerAdmob.instance.IsRewardedVideoLoaded())
                return AdsManagerAdmob.instance;

            if (AdsManagerUnity.instance.IsRewardedVideoLoaded())
                return AdsManagerUnity.instance;

            return null;
        }
        else if (adsPlacement == ADS_PLACEMENT.BANNER) {
            if (AdsManagerAdmob.instance.IsBannerLoaded())
                return AdsManagerAdmob.instance;

            if (AdsManagerUnity.instance.IsBannerLoaded())
                return AdsManagerUnity.instance;

            return null;
        }
        else {
            if (AdsManagerAdmob.instance.IsInterstitialLoaded())
                return AdsManagerAdmob.instance;

            if (AdsManagerUnity.instance.IsInterstitialLoaded())
                return AdsManagerUnity.instance;

            return null;
        }
    }

    private static AdsManager GetInstanceUnity(ADS_PLACEMENT adsPlacement) {
        if (adsPlacement == ADS_PLACEMENT.REWARDED_VIDEO) {
            if (AdsManagerUnity.instance.IsRewardedVideoLoaded())
                return AdsManagerUnity.instance;

            if (AdsManagerAdmob.instance.IsRewardedVideoLoaded())
                return AdsManagerAdmob.instance;

            return null;
        }
        else if (adsPlacement == ADS_PLACEMENT.BANNER) {
            if (AdsManagerUnity.instance.IsBannerLoaded())
                return AdsManagerUnity.instance;

            if (AdsManagerAdmob.instance.IsBannerLoaded())
                return AdsManagerAdmob.instance;

            return null;
        }
        else {
            if (AdsManagerUnity.instance.IsInterstitialLoaded())
                return AdsManagerUnity.instance;

            if (AdsManagerAdmob.instance.IsInterstitialLoaded())
                return AdsManagerAdmob.instance;

            return null;
        }
    }
    public virtual void ShowRewardedAd(bool popup, Callback callback) {
    }

    public virtual void ShowInterstitial(Callback callback) {
    }

    public virtual bool IsRewardedVideoLoaded() {
        return true;
    }

    public virtual bool IsInterstitialLoaded() {
        return true;
    }

    public virtual bool IsBannerLoaded() {
        return true;
    }

    public virtual void ShowBanner() {

    }

    public virtual void HideBanner() {

    }
}
