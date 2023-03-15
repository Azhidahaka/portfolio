using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : UIBase {
    public GameObject objBtnLogin;
    public GameObject objBtnLogout;
    public GameObject objBtnGameStart;

    public RawImage imgTitle;

    private void OnEnable() {
        EventManager.Register(EventEnum.AuthStateChanged, OnAuthStateChanged);
        EventManager.Register(EventEnum.LoginSelectBtnBackClick, OnLoginSelectBtnBackClick);
    }

    private void OnLoginSelectBtnBackClick(object[] args) {
        Common.ToggleActive(objBtnLogin, true);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.AuthStateChanged, OnAuthStateChanged);
        EventManager.Remove(EventEnum.LoginSelectBtnBackClick, OnLoginSelectBtnBackClick);
    }

    public void SetData() { 
        imgTitle.texture = ResourceManager.instance.GetGameTitleTexture();
        DetermineShowUI(false);
    }

    public void OnAuthStateChanged(object[] args = null) {
        DetermineShowUI(true);
    }

    private void DetermineShowUI(bool byEvent) {
        if (string.IsNullOrEmpty(BackendLogin.instance.UserInDate)) {
            Common.ToggleActive(objBtnLogin, true);
            Common.ToggleActive(objBtnLogout, false);
            Common.ToggleActive(objBtnGameStart, false);
            if (byEvent)
                EventManager.Notify(EventEnum.SkinChanged, true);
        }
        else {
            Common.ToggleActive(objBtnLogin, false);
            Common.ToggleActive(objBtnLogout, true);
            Common.ToggleActive(objBtnGameStart, true);
            if (byEvent)
                EventManager.Notify(EventEnum.SkinChanged, true);
        }
    }

    public void OnBtnLoginClick() {
        Common.ToggleActive(objBtnLogin, false);

        LoginSelectPopup loginSelectPopup = UIManager.instance.GetUI<LoginSelectPopup>(UI_NAME.LoginSelectPopup);
        loginSelectPopup.SetData();
        loginSelectPopup.Show();
    }

    public void OnBtnLogoutClick() {
        Common.ToggleActive(objBtnGameStart, false);
        Common.ToggleActive(objBtnLogout, false);

        BackendLogin.instance.ReqLogout();
    }

    public void OnBtnGameStartClick() {
        Common.ToggleActive(objBtnGameStart, false);
        Common.ToggleActive(objBtnLogout, false);
        WebUser.instance.ReqAgreePolicy(null);

        WebUser.instance.ReqSetStatistics(1, 
                                            STATISTICS_TYPE.ACC_LOGIN_COUNT, 
                                            STATISTICS_TYPE.DAILY_LOGIN_COUNT);

        long prevLoginUtcZero = UserDataModel.instance.userProfile.loginUtcZero;
        long loginUtcZero = Common.GetUTCDateZero(Common.GetUTCNow());
        //날짜가 지났으면 Daily변수 리셋
        if (prevLoginUtcZero + 86400 <= loginUtcZero) 
            WebUser.instance.ReqRefreshDate();

        //하루전에 로그인 기록이 있으면 DAILY_CONTINUOUS_LOGIN_COUNT 추가
        if (prevLoginUtcZero + 86400 == loginUtcZero) {
            WebUser.instance.ReqSetStatistics(1,                                             
                                              STATISTICS_TYPE.DAILY_CONTINUOUS_LOGIN_COUNT);
        }

        UserDataModel.instance.userProfile.loginUtcZero = loginUtcZero;
        AchievementManager.instance.StartCheckDate();

        //DetermineRewardAdsCooldownEnd();

        BackendRequest.instance.DetermineNation(() => {
            App.instance.ChangeScene(App.SCENE_NAME.Home);
            BackendRequest.instance.ReqUserNumber(ResUserNumber);
        });
        

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugUI.instance.Hide();
#endif
    }

    private void ResUserNumber() {

    }

    private void DetermineRewardAdsCooldownEnd() {
        long newCooldownEnd = Common.GetUnixTimeNow() + Constant.SHOP_REWARD_ADS_START_COOLDOWN;
        long currentCooldownEnd = UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd;
        if (newCooldownEnd > currentCooldownEnd)
            UserDataModel.instance.userProfile.shopRewardAdsCooldownEnd = newCooldownEnd;
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }
}
