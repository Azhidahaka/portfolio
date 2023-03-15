using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScenePresenter : MonoBehaviour {
    public static TitleScenePresenter instance;


    private void Awake() {
        instance = this;
    }

    public void SetData() {
        App.instance.PlayMainBGM();

        DetermineShowPolicyPopup();
        DetermineShowUpdateUI();

        Title loginMenu = UIManager.instance.GetUI<Title>(UI_NAME.Title);
        loginMenu.SetData();
        loginMenu.Show();
    }

    private void DetermineShowPolicyPopup() {
        if (ES_Save.Exists(Constant.PATH_AGREE_TERM) == false ||
            ES_Save.Load<long>(Constant.PATH_AGREE_TERM) == (long)POLICY_AGREE_STATE.NOT_SET ||
            ES_Save.Exists(Constant.PATH_AGREE_PRIVACY) == false ||
            ES_Save.Load<long>(Constant.PATH_AGREE_PRIVACY) == (long)POLICY_AGREE_STATE.NOT_SET ||
            ES_Save.Exists(Constant.PATH_AGREE_NIGHT_PUSH) == false ||
            ES_Save.Load<long>(Constant.PATH_AGREE_NIGHT_PUSH) == (long)POLICY_AGREE_STATE.NOT_SET) {
            AgreementPopup agreementPopup = UIManager.instance.GetUI<AgreementPopup>(UI_NAME.AgreementPopup);
            agreementPopup.SetData();
            agreementPopup.Show();
        }
    }

    private void DetermineShowUpdateUI() {
        if (BackendLogin.instance.updateState == BackendLogin.UPDATE_STATE.FORCED) {
            string msg = TermModel.instance.GetTerm("msg_update_notice");
            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, ShowLastVersionPage);
        }
        else if (BackendLogin.instance.updateState == BackendLogin.UPDATE_STATE.SELECTABLE) {
            string msg = TermModel.instance.GetTerm("msg_update_confirm");
            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, ShowLastVersionPage);
        }
    }

    private void ShowLastVersionPage() {
#if UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#elif UNITY_IOS
        LANGUAGE language = LANGUAGE.eng;
        if (Application.systemLanguage == SystemLanguage.Korean)
            language = LANGUAGE.kor;
        Application.OpenURL(Constant.APPLE_APP_STORE_DIC[language]);
#endif
    }


}
