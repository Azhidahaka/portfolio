using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenuDailyAds : MonoBehaviour {
    public Text lblRewardAmount;

    public GameObject objCooldown;
    public Text lblCooldown;
    public Text lblAdsViewCount;

    private void OnEnable() {
        StartCoroutine(JobUpdateState());
    }

    private IEnumerator JobUpdateState() {
        while (true) {
            yield return new WaitForSeconds(1.0f);
            UpdateState();
        }
    }

    public void SetData() {
        lblRewardAmount.text = Constant.SHOP_REWARD_ADS_DIAMOND.ToString();

        UpdateState();
    }

    private void UpdateState() {
        long viewCount = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.DAILY_SHOP_REWARD_ADS_VIEW_COUNT);
        long viewCountMax = Constant.SHOP_REWARD_ADS_VIEW_MAX;

        lblAdsViewCount.text = $"({viewCount}/{viewCountMax})";

        if (viewCount < viewCountMax) {
            if (IsAvailable())
                Common.ToggleActive(objCooldown, false);
            else {
                long cooldownEnd = UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd;
                lblCooldown.text = Common.GetTimerFormat(cooldownEnd - Common.GetUnixTimeNow());
                Common.ToggleActive(objCooldown, true);
            }
        }
        //광고를 더이상 볼수 없는상황
        else {
            long tomorrow = UserDataModel.instance.userProfile.loginUtcZero + 86400;
            lblCooldown.text = Common.GetTimerFormat(tomorrow - Common.GetUTCNow());
            Common.ToggleActive(objCooldown, true);
        }
    }

    private bool IsAvailable() {
        long cooldownEnd = UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd;
        if (Common.GetUnixTimeNow() >= cooldownEnd)
            return true;
        return false;
    }

    public void OnBtnShowAdsClick() {
        long viewCount = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.DAILY_SHOP_REWARD_ADS_VIEW_COUNT);
        long viewCountMax = Constant.SHOP_REWARD_ADS_VIEW_MAX;

        if (IsAvailable() == false || viewCount >= viewCountMax) {
            string format = TermModel.instance.GetTerm("format_cooldown_warning");

            string cooldownStr;
            if (viewCount < viewCountMax) {
                long cooldownEnd = UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd;
                cooldownStr = Common.GetTimerFormat(cooldownEnd - Common.GetUnixTimeNow());
            }
            //광고를 더이상 볼수 없는상황
            else {
                long tomorrow = UserDataModel.instance.userProfile.loginUtcZero + 86400;
                cooldownStr = Common.GetTimerFormat(tomorrow - Common.GetUTCNow());
            }

            MessageUtil.ShowSimpleWarning(string.Format(format, cooldownStr));
            return;
        }

        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.REWARDED_VIDEO);
        Callback callback = () => {
            AnalyticsUtil.LogAdImpressionDailyReward();
            WebAds.instance.ReqGetShopAdsReward(OnResGetShopAdsReward);
        };

        if (adsInstance != null && adsInstance.IsRewardedVideoLoaded())
            adsInstance.ShowRewardedAd(false, callback);
        else
            callback();
    }

    private void OnResGetShopAdsReward() {
        SetData();
    }
}
