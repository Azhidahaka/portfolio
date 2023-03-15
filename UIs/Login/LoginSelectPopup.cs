using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSelectPopup : UIBase {
    public GameObject googleObject;
    public GameObject gameCenterObject;
    public GameObject appleObject;

    public void SetData() {
#if UNITY_ANDROID
        Common.ToggleActive(googleObject, true);
        Common.ToggleActive(appleObject, false);
        Common.ToggleActive(gameCenterObject, false);
#elif UNITY_IOS
        Common.ToggleActive(gameCenterObject, true);
        Common.ToggleActive(appleObject, true);
        Common.ToggleActive(googleObject, false);
#endif
    }

    public void OnBtnGuestLoginClick() {
        Hide();
#if UNITY_EDITOR
        BackendLogin.instance.OnCustomLoginClick();
#else
        BackendLogin.instance.OnGuestLoginClick();
#endif
    }

    public void OnBtnGoogleLoginClick() {
        Hide();
#if UNITY_EDITOR
        BackendLogin.instance.OnCustomLoginClick();
#else
        BackendLogin.instance.OnBtnGoogleLoginClick();
#endif
    }

    public void OnBtnFaceBookLoginClick() {
        Hide();
#if UNITY_EDITOR
        BackendLogin.instance.OnCustomLoginClick();
#else
        BackendLogin.instance.OnBtnFacebookLoginClick();     
#endif   
    }

    public void OnBtnGameCenterClick() {
        Hide();
#if UNITY_EDITOR
        BackendLogin.instance.OnCustomLoginClick();
#else
        BackendLogin.instance.OnBtnGameCenterLoginClick();
#endif
    }

    public void OnBtnAppleClick() {
        Hide();
#if UNITY_EDITOR
        BackendLogin.instance.OnCustomLoginClick();
#else
        BackendLogin.instance.OnBtnAppleLoginClick();
#endif
    }

    public void OnBtnBackClick() {
        Hide();
        EventManager.Notify(EventEnum.LoginSelectBtnBackClick);
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }
}
