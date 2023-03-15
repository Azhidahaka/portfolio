using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : UIBase {
    public Slider sldBGM;
    public Slider sldEffect;

    public Text lblBGMVolume;
    public Text lblEffectVolume;

    public GameObject objMuteBGM;
    public GameObject objMuteEffect;

    public Button btnRestore;

    public Text lblLanguage;

    public List<OptionMenuBtnFederation> federationBtns;
    public GameObject objBtnAccountLink;

    public GameObject objPuchCheckOff;
    public GameObject objPuchCheckOn;

    public List<GameObject> buttons;
    public List<GameObject> matchBlocksHideButtons;

    private void OnEnable() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugUI.instance.Show();
#endif
        PauseTime();
    }

    private void OnDisable() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugUI.instance.Hide();
#endif
        ResumeTime();
    }

    public void OnBtnCloseClick() {
        Hide();
    }

    public void SetData() {
        for (int i = 0; i < buttons.Count; i++) {
            Common.ToggleActive(buttons[i], true);
        }

        UserData.GameOptionDTO gameOptions = UserDataModel.instance.gameOptions;

        Common.ToggleActive(objMuteBGM, gameOptions.bgmVolumeRatio == 0);
        sldBGM.SetValueWithoutNotify((float)gameOptions.bgmVolumeRatio);
        lblBGMVolume.text = ((int)(sldBGM.value * 100)).ToString();

        Common.ToggleActive(objMuteEffect, gameOptions.effectVolumeRatio == 0);
        sldEffect.SetValueWithoutNotify((float)gameOptions.effectVolumeRatio);
        lblEffectVolume.text = ((int)(sldEffect.value * 100)).ToString();

        DetermineShowBtnRestore();

        SetLanguage();

        SetLinkAccountInfo();

        if (App.instance.GetSceneName() == App.SCENE_NAME.MatchBlocks) {
            for (int i = 0; i < matchBlocksHideButtons.Count; i++) {
                Common.ToggleActive(matchBlocksHideButtons[i], false);
            }
        }

        DeterminePushState();
    }

    private void SetLinkAccountInfo() {
        FEDERATION_TYPE federationType = BackendLogin.instance.GetFederationType();
        if (federationType == FEDERATION_TYPE.GUEST) {
            Common.ToggleActive(objBtnAccountLink, true);
            foreach (OptionMenuBtnFederation btn in federationBtns) {
                Common.ToggleActive(btn.gameObject, false);
            }
        }
        else {
            Common.ToggleActive(objBtnAccountLink, false);
            foreach (OptionMenuBtnFederation btn in federationBtns) {
                if (federationType == btn.federationType) {
                    btn.SetData();
                    Common.ToggleActive(btn.gameObject, true);
                }
                else
                    Common.ToggleActive(btn.gameObject, false);
            }
        }
    }

    private void SetLanguage() {
        lblLanguage.text = TermModel.instance.GetTerm($"language_{((LANGUAGE)UserDataModel.instance.gameOptions.language).ToString()}");
    }

    private void DetermineShowBtnRestore() {
#if UNITY_EDITOR || UNITY_IOS
        if (ProductUtil.IsNonConsumablePurchasedAll() == false)
            Common.ToggleActive(btnRestore.gameObject, true);
        else
            Common.ToggleActive(btnRestore.gameObject, false);
#else
        Common.ToggleActive(btnRestore.gameObject, false);
#endif
    }

    public void OnSldBGMValueChange() {
        if (UserDataModel.instance.gameOptions.bgmVolumeRatio == sldBGM.value)
            return;

        WebOption.instance.ReqChangeBGMVolume(sldBGM.value, SetData);
        EventManager.Notify(EventEnum.BGMVolumeChanged, sldBGM.value);
    }

    public void OnSldEffectValueChange() {
        if (UserDataModel.instance.gameOptions.effectVolumeRatio == sldEffect.value)
            return;

        WebOption.instance.ReqChangeEffectVolume(sldEffect.value, SetData);
        EventManager.Notify(EventEnum.EffectVolumeChanged, sldEffect.value);
    }

    public void OnBtnEffectMuteClick() {
        UserData.GameOptionDTO gameOptions = UserDataModel.instance.gameOptions;
        //뮤트 해제시 저장된 볼륨정보를 가져와 슬라이더에 셋팅
        WebOption.instance.ReqMuteEffect(OnResMuteEffect);
    }

    private void OnResMuteEffect() {
        SetData();
        EventManager.Notify(EventEnum.EffectMute);
    }
    
    public void OnBtnBGMMuteClick() {
        UserData.GameOptionDTO gameOptions = UserDataModel.instance.gameOptions;
        //뮤트 해제시 저장된 볼륨정보를 가져와 슬라이더에 셋팅
        WebOption.instance.ReqMuteBGM(OnResMuteBGM);
    }

    private void OnResMuteBGM() {
        SetData();
        EventManager.Notify(EventEnum.BGMMute);
    }

    public void OnBtnSaveClick() {
        if (UserDataModel.instance.RevisionChanged == false) {
            string msg = TermModel.instance.GetTerm("msg_save_warning");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        BackendRequest.instance.ReqSyncUserData(false, true, OnResSetUserDataSuccess);
    }

    private void OnResSetUserDataFail() {
        string msg = TermModel.instance.GetTerm("msg_save_fail");
        MessageUtil.ShowSimpleWarning(msg);
    }

    private void OnResSetUserDataSuccess() {
        SetData();
        string msg = TermModel.instance.GetTerm("msg_save_success");
        MessageUtil.ShowSimpleWarning(msg);
    }

    public void OnBtnQuitClick() {
        MessageUtil.ShowQuitPopup();
    }

    public void OnBtnAskClick() {
        BackendQuestion.instance.ShowQuestion();
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }

    public void OnBtnRestore() {
        //모든 비소모성 상품을 구매한 경우 리턴
        if (ProductUtil.IsNonConsumablePurchasedAll()) {
            string msg = TermModel.instance.GetTerm("msg_purchase_restoration_fail");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

#if UNITY_IOS 
        IAPManager.instance.RestorePurchase();
#endif
    }

    public void OnBtnLanguageClick() {
        SelectLanguagePopup selectLanguagePopup = UIManager.instance.GetUI<SelectLanguagePopup>(UI_NAME.SelectLanguagePopup);
        selectLanguagePopup.SetData();
        selectLanguagePopup.Show();
    }

    public void OnBtnInfoClick() {
        InAppBrowser.ClearCache();
        LANGUAGE language = (LANGUAGE)UserDataModel.instance.gameOptions.language;
        InAppBrowser.OpenURL(Constant.INFO_URL_DIC[language]);
    }

    public void OnBtnAccountLinkClick() {
        AccountLinkSelectPopup accountLinkSelectPopup = UIManager.instance.GetUI<AccountLinkSelectPopup>(UI_NAME.AccountLinkSelectPopup);
        accountLinkSelectPopup.SetData();
        accountLinkSelectPopup.Show();
    }

    public void OnBtnTogglePushClick() {
        if (UserDataModel.instance.userProfile.agreeNightPush == (long)POLICY_AGREE_STATE.AGREE) {
            WebUser.instance.ReqAgreePush((long)POLICY_AGREE_STATE.DISAGREE, OnResAgreePush);
        }
        else {
            WebUser.instance.ReqAgreePush((long)POLICY_AGREE_STATE.AGREE, OnResAgreePush);
        }
    }

    public void OnResAgreePush() {
        DeterminePushState();
    }

    private void DeterminePushState() {
        if (UserDataModel.instance.userProfile.agreeNightPush == (long)POLICY_AGREE_STATE.AGREE) {
            Common.ToggleActive(objPuchCheckOff, false);
            Common.ToggleActive(objPuchCheckOn, true);
        }
        else {
            Common.ToggleActive(objPuchCheckOff, true);
            Common.ToggleActive(objPuchCheckOn, false);
        }
    }
}
