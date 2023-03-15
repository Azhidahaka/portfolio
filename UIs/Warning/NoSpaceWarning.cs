using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoSpaceWarning : UIBase {
    public GameObject goBtnAds;

    public void SetData() {
        Common.ToggleActive(goBtnAds, MatchBlocksReferee.instance.ShowNoSpaceAdsCount == 0);
    }

    public void OnBtnAdsClick() {
        //@todo : 광고보고 500골드 충전
        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.REWARDED_VIDEO);

        Callback callback = ()=> {
            WebStage.instance.ReqGetNoSpaceGold(OnResGetNoSpaceGold);
        };

        if (adsInstance != null && adsInstance.IsRewardedVideoLoaded())
            adsInstance.ShowRewardedAd(false, callback);
        else {
            string msg = TermModel.instance.GetTerm("msg_no_ads");
            MessageUtil.ShowSimpleWarning(msg);
        }
    }

    private void OnResGetNoSpaceGold() {
        string format = TermModel.instance.GetTerm("format_get_no_space_gold");
        string msg = string.Format(format, Constant.NO_SPACE_GET_GOLD);
        MessageUtil.ShowSimpleWarning(msg);

        SetData();
    }

    public void OnBtnShopClick() {
        ShopPopup shopPopup = UIManager.instance.GetUI<ShopPopup>(UI_NAME.ShopPopup);
        shopPopup.SetData(PRODUCT_COST_TYPE.GOLD);
        shopPopup.Show();
    }

    public void OnBtnEndClick() {
        string format = TermModel.instance.GetTerm("format_give_up_confirm");
        long score = MatchBlocksReferee.instance.GetRefereeNote().totalScore;
        string msg = string.Format(format, Common.GetCommaFormat(score));
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, OnBtnEndConfirm);
    }

    private void OnBtnEndConfirm() {
        Hide();
        EventManager.Notify(EventEnum.MatchBlocksGameOver);
    }
}
