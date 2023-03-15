using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLanguagePopup : UIBase {
    public void SetData() {

    }

    public void OnBtnKoreanClick() {
        WebUser.instance.ReqChangeLanguage(LANGUAGE.kor, OnResChangeLanguage);
    }

    private void OnResChangeLanguage() {
        App.instance.ChangeScene(App.SCENE_NAME.Home);
    }

    public void OnBtnEnglishClick() {
        WebUser.instance.ReqChangeLanguage(LANGUAGE.eng, OnResChangeLanguage);
    }
}
