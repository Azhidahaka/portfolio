using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountLinkSelectPopup : UIBase {
    public GameObject objBtnGoogle;
    public GameObject objBtnApple;
    public GameObject objBtnFacebook;

    public void SetData() {
#if UNITY_ANDROID
        Common.ToggleActive(objBtnGoogle.gameObject, true);
        Common.ToggleActive(objBtnFacebook.gameObject, true);
        Common.ToggleActive(objBtnApple.gameObject, false);
#elif UNITY_IOS
        Common.ToggleActive(objBtnApple.gameObject, true);
        Common.ToggleActive(objBtnFacebook.gameObject, true);
        Common.ToggleActive(objBtnGoogle.gameObject, false);
#endif
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }

    public void OnBtnGoogleClick() {
        BackendLogin.instance.ChangeCustomToGoogle();
    }

    public void OnBtnFacebookClick() {
        BackendLogin.instance.ChangeCustomToFacebook();
    }

    public void OnBtnAppleClick() {
        BackendLogin.instance.ChangeCustomToApple();
    }

}
