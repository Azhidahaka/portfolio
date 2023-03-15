using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebAds : MonoBehaviour {
    public static WebAds instance;

    private void Awake() {
        instance = this;
    }

    public void ReqIncreaseAdsView(bool popup, Callback successCallback = null) {
        long dailyAdsViewCount = UserDataModel.instance.DailyAdsViewCount;

        UserDataModel.instance.DailyAdsViewCount = dailyAdsViewCount + 1;
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_VIEW_ADS_COUNT, 1);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_VIEW_ADS_COUNT, 1);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.UserDataUserInfoUpdate);

        if (successCallback != null)
            successCallback();
    }

    public void ReqGetShopAdsReward(Callback successCallback = null) {
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_SHOP_REWARD_ADS_VIEW_COUNT, 1, false);
        UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd = Common.GetUnixTimeNow() + Constant.SHOP_REWARD_ADS_COOLDOWN;
        UserDataModel.instance.AddDiamond(Constant.SHOP_REWARD_ADS_DIAMOND);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        if (successCallback != null)
            successCallback();
    }
}
