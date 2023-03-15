using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using QuantumTek.EncryptedSave;

public class AgreementPopup : UIBase {
    public GameObject termCheckBoxOn;
    public GameObject termCheckBoxOff;

    public GameObject privacyCheckBoxOn;
    public GameObject privacyCheckBoxOff;

    public GameObject pushCheckBoxOn;
    public GameObject pushCheckBoxOff;

    private long agreeTerm;
    private long agreePrivacy;
    private long agreePush;

    private LANGUAGE language;

    public void SetData() {
        if (Application.systemLanguage == SystemLanguage.Korean)
            language = LANGUAGE.kor;
        else
            language = LANGUAGE.eng;

        agreeTerm = (long)POLICY_AGREE_STATE.DISAGREE;
        agreePrivacy = (long)POLICY_AGREE_STATE.DISAGREE;
        agreePush = (long)POLICY_AGREE_STATE.DISAGREE;
        DetermineCheckBoxState();
    }

    private void DetermineCheckBoxState() {
        if (agreeTerm == (long)POLICY_AGREE_STATE.DISAGREE) {
            Common.ToggleActive(termCheckBoxOn.gameObject, false);
            Common.ToggleActive(termCheckBoxOff.gameObject, true);
        }
        else {
            Common.ToggleActive(termCheckBoxOn.gameObject, true);
            Common.ToggleActive(termCheckBoxOff.gameObject, false);
        }

        if (agreePrivacy == (long)POLICY_AGREE_STATE.DISAGREE) {
            Common.ToggleActive(privacyCheckBoxOn.gameObject, false);
            Common.ToggleActive(privacyCheckBoxOff.gameObject, true);
        }
        else {
            Common.ToggleActive(privacyCheckBoxOn.gameObject, true);
            Common.ToggleActive(privacyCheckBoxOff.gameObject, false);
        }

        if (agreePush == (long)POLICY_AGREE_STATE.DISAGREE) {
            Common.ToggleActive(pushCheckBoxOn.gameObject, false);
            Common.ToggleActive(pushCheckBoxOff.gameObject, true);
        }
        else {
            Common.ToggleActive(pushCheckBoxOn.gameObject, true);
            Common.ToggleActive(pushCheckBoxOff.gameObject, false);
        }
    }

    public void OnBtnTermDetailClick() {
        InAppBrowser.ClearCache();
        InAppBrowser.OpenURL(Constant.TERM_OF_SERVICE_URL_DIC[language]);
    }

    public void OnBtnTermAgreeClick() {
        if (agreeTerm == (long)POLICY_AGREE_STATE.DISAGREE) 
            agreeTerm = (long)POLICY_AGREE_STATE.AGREE;
        else
            agreeTerm = (long)POLICY_AGREE_STATE.DISAGREE;

        DetermineCheckBoxState();
    }

    public void OnBtnPrivacyDetailClick() {
        InAppBrowser.ClearCache();
        InAppBrowser.OpenURL(Constant.PRIVACY_POLICY_URL_DIC[language]);
    }

    public void OnBtnPrivacyAgreeClick() {
        if (agreePrivacy == (long)POLICY_AGREE_STATE.DISAGREE) 
            agreePrivacy = (long)POLICY_AGREE_STATE.AGREE;
        else
            agreePrivacy = (long)POLICY_AGREE_STATE.DISAGREE;

        DetermineCheckBoxState();
    }

    public void OnBtnPushAgreeClick() {
        if (agreePush == (long)POLICY_AGREE_STATE.DISAGREE) 
            agreePush = (long)POLICY_AGREE_STATE.AGREE;
        else
            agreePush = (long)POLICY_AGREE_STATE.DISAGREE;

        DetermineCheckBoxState();
    }

    public void OnBtnAgreeAllClick() {
        agreePush = (long)POLICY_AGREE_STATE.AGREE;
        agreePrivacy = (long)POLICY_AGREE_STATE.AGREE;
        agreeTerm = (long)POLICY_AGREE_STATE.AGREE;
        
        DetermineCheckBoxState();
        OnBtnConfirmClick();
    }

    public void OnBtnConfirmClick() {
        if (agreeTerm == (long)POLICY_AGREE_STATE.DISAGREE || agreePrivacy == (long)POLICY_AGREE_STATE.DISAGREE) {
            string error = TermModel.instance.GetTerm("msg_agree_all");
            MessageUtil.ShowSimpleWarning(error);
            return;
        }
            
        ES_Save.Save(agreeTerm, Constant.PATH_AGREE_TERM);
        ES_Save.Save(agreePrivacy, Constant.PATH_AGREE_PRIVACY);
        ES_Save.Save(agreePush, Constant.PATH_AGREE_NIGHT_PUSH);
        ES_Save.Save((long)1, Constant.PATH_FORCE_REFRESH_PUSH);

        string msg;
        if (agreePush == (long)POLICY_AGREE_STATE.DISAGREE)
            msg = TermModel.instance.GetTerm("msg_push_disabled");
        else
            msg = TermModel.instance.GetTerm("msg_push_enabled");
        MessageUtil.ShowSimpleWarning(msg);

        Hide();
    }

    public override List<object> GetCopyDatas() {
        return null;
    }

    public override void OnCopy(List<object> datas) {
        SetData();
    }
}
